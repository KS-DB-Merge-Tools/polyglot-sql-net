#!/usr/bin/env python3
"""Detect API drift between the Rust Polyglot library and the PolyglotSql .NET wrapper.

Compares a Rust source tree (default: ../polyglot-main) against the .NET wrapper
source tree (default: ../src) and reports:
  - FFI functions the Rust library exposes that the .NET wrapper doesn't call, and
    vice versa.
  - Field-level drift on "options" structs (new/removed/type-changed parameters),
    e.g. TranspileOptions.
  - Member-level drift on enums and serde-tagged-union types (e.g. new SQL dialect,
    new DataType variant).

Type mappings between Rust names and .NET class/enum names are hand-maintained in
polyglot_api_drift_manifest.json next to this script (Rust type names don't always
match .NET class names 1:1 - see that file's "notes" fields for known exceptions).

This is a maintainer tool, not a build-time check: nothing currently calls it from
CI. Run it after bumping the vendored Rust source to see what the wrapper needs.

Implementation note: Rust/.NET source is parsed with regexes and brace/paren-depth
scanning, not a real parser, so it can't see macro-injected fields and treats
custom (de)serializer functions as opaque. See the "Limitations" section of the
report for the full list of known gaps.
"""
from __future__ import annotations

import argparse
import dataclasses
import json
import re
import sys
from pathlib import Path
from typing import Optional

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent

PRIMITIVE_RUST_TYPES = {
    "bool", "String", "str", "char",
    "u8", "u16", "u32", "u64", "u128", "usize",
    "i8", "i16", "i32", "i64", "i128", "isize",
    "f32", "f64",
}
CONTAINER_RUST_TYPES = {
    "Option", "Vec", "HashMap", "BTreeMap", "HashSet", "BTreeSet", "Box", "Arc", "Rc",
}

RUST_TO_CS_PRIMITIVES = {
    "bool": {"bool"},
    "String": {"string"},
    "str": {"string"},
    "usize": {"int", "uint", "long", "ulong"},
    "u32": {"int", "uint"},
    "u16": {"int", "uint", "short", "ushort"},
    "u8": {"int", "uint", "byte"},
    "u64": {"long", "ulong"},
    "i32": {"int"},
    "i16": {"short", "int"},
    "i8": {"sbyte", "int"},
    "i64": {"long"},
    "f32": {"float"},
    "f64": {"double"},
}
CS_CONTAINER_NAMES = {"List", "IReadOnlyList", "IEnumerable", "ICollection", "Dictionary", "IReadOnlyDictionary"}

# Populated by run() from the manifest before diffing, so soft_type_check() can resolve
# Rust type names that are intentionally renamed on the .NET side (e.g. DialectType -> Dialect).
_MANIFEST_DOTNET_BY_RUST = {}


# --------------------------------------------------------------------------- #
# Data structures
# --------------------------------------------------------------------------- #

@dataclasses.dataclass
class RustField:
    name: str
    json_key: str
    rust_type: str
    optional: bool


@dataclasses.dataclass
class RustStruct:
    name: str
    fields: list
    source_file: str
    source_kind: str  # "ffi_local" | "core"


@dataclasses.dataclass
class RustEnumVariant:
    name: str
    json_value: str
    fields: list


@dataclasses.dataclass
class RustEnum:
    name: str
    tag_key: Optional[str]
    variants: list
    source_file: str
    source_kind: str


@dataclasses.dataclass
class DotnetProperty:
    json_key: str
    dotnet_type: str
    name: str


@dataclasses.dataclass
class DotnetOptionsType:
    name: str
    file: str
    properties: list


@dataclasses.dataclass
class DotnetEnumType:
    name: str
    file: str
    members: list
    snake_case_converter: bool = False


@dataclasses.dataclass
class DotnetTaggedUnionType:
    name: str
    file: str
    tag_key: Optional[str]
    variants: dict  # json_value -> list[DotnetProperty]


@dataclasses.dataclass
class Finding:
    category: str
    severity: str  # "info" | "warning" | "drift"
    rust_name: str
    dotnet_name: Optional[str]
    message: str
    detail: dict = dataclasses.field(default_factory=dict)

    def to_dict(self):
        return dataclasses.asdict(self)


# --------------------------------------------------------------------------- #
# Small text-parsing helpers shared by the Rust and .NET extractors
# --------------------------------------------------------------------------- #

def extract_braced_block(text: str, open_brace_idx: int):
    """Given the index of a '{' in text, return (inner_text, index_after_matching_'}')."""
    depth = 0
    i = open_brace_idx
    n = len(text)
    while i < n:
        c = text[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                return text[open_brace_idx + 1:i], i + 1
        i += 1
    raise ValueError("unbalanced braces")


def split_top_level(text: str, sep: str = ",") -> list:
    """Split text on `sep` only at bracket depth 0 (so generics/nested braces survive)."""
    parts = []
    depth = 0
    current = []
    for ch in text:
        if ch in "<([{":
            depth += 1
        elif ch in ">)]}":
            depth -= 1
        if ch == sep and depth == 0:
            parts.append("".join(current))
            current = []
        else:
            current.append(ch)
    if current:
        parts.append("".join(current))
    return [p for p in parts if p.strip()]


def strip_doc_comments(text: str) -> str:
    """Drop whole-line comments, both doc (`///`) and plain section comments (`//`) -
    e.g. `// Numeric` above `Boolean,` in the DataType enum - since a leftover comment
    line makes the next chunk fail the "is this a bare identifier" check and get
    silently dropped as a parsed variant/field."""
    return "\n".join(line for line in text.splitlines() if not line.strip().startswith("//"))


ATTR_RE = re.compile(r"#\[([^\]]*)\]")


def snake_to_camel(name: str) -> str:
    parts = name.split("_")
    return parts[0] + "".join(p[:1].upper() + p[1:] for p in parts[1:] if p)


def pascal_to_snake(name: str) -> str:
    """Underscore before every uppercase letter after position 0 - matches both serde's
    actual rename_all="snake_case" algorithm and .NET's SnakeCaseJsonEnumConverter,
    so e.g. "NChar" -> "n_char" (not a smarter acronym-aware "nchar")."""
    return "".join(("_" + c.lower()) if c.isupper() and i > 0 else c.lower() for i, c in enumerate(name))


def field_json_key(field_name: str, explicit_rename: Optional[str], rename_all: Optional[str]) -> str:
    if explicit_rename:
        return explicit_rename
    if rename_all == "camelCase":
        return snake_to_camel(field_name)
    if rename_all in ("snake_case", None):
        return field_name
    if rename_all == "SCREAMING_SNAKE_CASE":
        return field_name.upper()
    if rename_all == "lowercase":
        return field_name.lower()
    return field_name


def variant_json_value(variant_name: str, explicit_rename: Optional[str], rename_all: Optional[str]) -> str:
    if explicit_rename:
        return explicit_rename
    if rename_all == "snake_case":
        return pascal_to_snake(variant_name)
    if rename_all == "SCREAMING_SNAKE_CASE":
        return pascal_to_snake(variant_name).upper()
    if rename_all == "camelCase":
        return variant_name[:1].lower() + variant_name[1:] if variant_name else variant_name
    if rename_all == "lowercase":
        return variant_name.lower()
    if rename_all == "kebab-case":
        return pascal_to_snake(variant_name).replace("_", "-")
    return variant_name


def extract_custom_type_names(raw_type: str) -> list:
    """Pull out non-primitive, non-container identifiers referenced by a Rust type string."""
    idents = re.findall(r"[A-Za-z_][A-Za-z0-9_]*", raw_type)
    return [i for i in idents if i not in PRIMITIVE_RUST_TYPES and i not in CONTAINER_RUST_TYPES]


def parse_container_attrs(attrs: list):
    rename_all = None
    tag = None
    for a in attrs:
        m = re.search(r'rename_all\s*=\s*"([^"]+)"', a)
        if m:
            rename_all = m.group(1)
        m = re.search(r'\btag\s*=\s*"([^"]+)"', a)
        if m:
            tag = m.group(1)
    return rename_all, tag


def extract_preceding_attrs(text: str, def_start: int) -> list:
    """Walk backwards line-by-line collecting contiguous #[...] attribute lines above def_start.

    def_start points at the `struct`/`enum` keyword itself, which is usually preceded
    by `pub ` on the same line (e.g. "pub enum DataType {") - that partial line isn't
    an attribute or blank, so the walk must start from the line *before* it, not from
    def_start's exact character offset, or it stops immediately.
    """
    line_start = text.rfind("\n", 0, def_start) + 1
    lines_before = text[:line_start].splitlines()
    attrs = []
    i = len(lines_before) - 1
    while i >= 0:
        line = lines_before[i].strip()
        if line.startswith("#[") or line.startswith("///") or line == "":
            if line.startswith("#["):
                attrs.append(line)
            i -= 1
            continue
        break
    attrs.reverse()
    return attrs


def parse_field_chunk(chunk: str, rename_all: Optional[str]) -> Optional[RustField]:
    chunk_clean = strip_doc_comments(chunk)
    attrs = ATTR_RE.findall(chunk_clean)
    text_wo_attrs = ATTR_RE.sub("", chunk_clean)
    # `pub` is required on struct fields but absent on enum variant fields
    # (e.g. `TinyInt { length: Option<u32> }` has no `pub`), so it's optional here.
    m = re.search(r"(?:pub\s+)?(\w+)\s*:\s*(.+)", text_wo_attrs, re.DOTALL)
    if not m:
        return None
    field_name = m.group(1)
    raw_type = m.group(2).strip()
    # `#[serde(skip)]` fields never appear in JSON (e.g. SchemaValidationOptions.function_catalog).
    if any(re.search(r"\bserde\s*\((?:[^)]*,\s*)?skip\s*(?:[,)])", a) for a in attrs):
        return None
    explicit_rename = None
    for a in attrs:
        rm = re.search(r'\brename\s*=\s*"([^"]+)"', a)
        if rm:
            explicit_rename = rm.group(1)
    optional = bool(re.match(r"Option\s*<", raw_type))
    json_key = field_json_key(field_name, explicit_rename, rename_all)
    return RustField(name=field_name, json_key=json_key, rust_type=raw_type, optional=optional)


# --------------------------------------------------------------------------- #
# Rust-side extraction
# --------------------------------------------------------------------------- #

def load_capability_ffi_symbols(rust_root: Path):
    """Returns {symbol: capability_id} from docs/api-capabilities.json, or None if missing."""
    path = rust_root / "docs" / "api-capabilities.json"
    if not path.is_file():
        return None
    data = json.loads(path.read_text(encoding="utf-8"))
    symbols = {}
    for cap in data.get("capabilities", []):
        ffi = cap.get("layers", {}).get("ffi", {})
        if ffi.get("status") in ("supported", "partial"):
            for sym in ffi.get("symbols", []):
                symbols[sym] = cap.get("id", "?")
    return symbols


NO_MANGLE_FN_RE = re.compile(
    r'#\[no_mangle\]\s*(?:///[^\n]*\n\s*)*pub\s+extern\s+"C"\s+fn\s+(\w+)\s*\(',
)


def extract_no_mangle_symbols(rust_root: Path):
    ffi_src = rust_root / "crates" / "polyglot-sql-ffi" / "src"
    symbols = set()
    if not ffi_src.is_dir():
        return symbols
    for path in sorted(ffi_src.rglob("*.rs")):
        text = path.read_text(encoding="utf-8", errors="replace")
        symbols.update(NO_MANGLE_FN_RE.findall(text))
    return symbols


def find_function_body(text: str, fn_name: str) -> Optional[str]:
    m = re.search(rf"\bfn\s+{re.escape(fn_name)}\s*\(", text)
    if not m:
        return None
    paren_start = m.end() - 1
    depth = 0
    i = paren_start
    n = len(text)
    while i < n:
        if text[i] == "(":
            depth += 1
        elif text[i] == ")":
            depth -= 1
            if depth == 0:
                break
        i += 1
    else:
        return None
    j = text.find("{", i)
    if j == -1:
        return None
    body, _ = extract_braced_block(text, j)
    return body


TURBOFISH_RE = re.compile(r"serde_json::from_str::<([\w:]+)>")
ANNOTATED_RE = re.compile(r"let\s+\w+(?:\s*:\s*([\w:]+))?\s*=\s*(?:match\s+)?serde_json::from_str")


def _search_options_type_in_body(body: str) -> Optional[str]:
    m = TURBOFISH_RE.search(body)
    if m:
        return m.group(1).split("::")[-1]
    m = ANNOTATED_RE.search(body)
    if m and m.group(1):
        return m.group(1).split("::")[-1]
    return None


def find_options_type_for_function(ffi_files: dict, fn_name: str) -> Optional[str]:
    """Search FFI crate source for the struct an FFI function deserializes its JSON arg into.

    Follows one level of `*_impl(...)` helper-function indirection, since some FFI
    entry points (e.g. polyglot_rename_tables_with_options) just delegate to a
    private `_impl` function that does the actual serde_json::from_str call.
    """
    for text in ffi_files.values():
        body = find_function_body(text, fn_name)
        if body is None:
            continue
        found = _search_options_type_in_body(body)
        if found:
            return found
        for call_m in re.finditer(r"\b(\w+_impl)\s*\(", body):
            helper_body = find_function_body(text, call_m.group(1))
            if helper_body:
                found = _search_options_type_in_body(helper_body)
                if found:
                    return found
    return None


_RUST_FILE_TEXT_CACHE = {}


def _read_cached(path: Path) -> str:
    text = _RUST_FILE_TEXT_CACHE.get(path)
    if text is None:
        text = path.read_text(encoding="utf-8", errors="replace")
        _RUST_FILE_TEXT_CACHE[path] = text
    return text


def find_type_definition(rust_root: Path, type_name: str):
    """Returns (kind, path, text, def_start, source_kind) for the first `struct`/`enum
    {type_name}` found, searching the FFI crate before the core crate."""
    ffi_dir = rust_root / "crates" / "polyglot-sql-ffi" / "src"
    core_dir = rust_root / "crates" / "polyglot-sql" / "src"
    for directory, source_kind in ((ffi_dir, "ffi_local"), (core_dir, "core")):
        if not directory.is_dir():
            continue
        for path in sorted(directory.rglob("*.rs")):
            text = _read_cached(path)
            m = re.search(rf"\bstruct\s+{re.escape(type_name)}\b", text)
            if m:
                return "struct", path, text, m.start(), source_kind
            m = re.search(rf"\benum\s+{re.escape(type_name)}\b", text)
            if m:
                return "enum", path, text, m.start(), source_kind
    return None


def parse_rust_struct(type_name: str, path: Path, text: str, def_start: int, source_kind: str, rust_root: Path) -> RustStruct:
    attrs = extract_preceding_attrs(text, def_start)
    rename_all, _tag = parse_container_attrs(attrs)
    brace_idx = text.index("{", def_start)
    body, _ = extract_braced_block(text, brace_idx)
    body = strip_doc_comments(body)
    fields = [f for f in (parse_field_chunk(c, rename_all) for c in split_top_level(body, ",")) if f]
    try:
        rel = path.relative_to(rust_root)
    except ValueError:
        rel = path
    return RustStruct(name=type_name, fields=fields, source_file=str(rel), source_kind=source_kind)


def parse_rust_enum(type_name: str, path: Path, text: str, def_start: int, source_kind: str, rust_root: Path) -> RustEnum:
    attrs = extract_preceding_attrs(text, def_start)
    rename_all, tag = parse_container_attrs(attrs)
    brace_idx = text.index("{", def_start)
    body, _ = extract_braced_block(text, brace_idx)
    body = strip_doc_comments(body)
    variants = []
    for chunk in split_top_level(body, ","):
        # Only attributes *before* the variant name belong to the variant. Attributes inside
        # a struct variant's body belong to its fields (e.g. OutputColumn::Wildcard's
        # `#[serde(rename = "startOrdinal")] start_ordinal`) and must stay for parse_field_chunk.
        variant_attrs = []
        chunk_no_attrs = chunk.strip()
        while True:
            am = ATTR_RE.match(chunk_no_attrs)
            if not am:
                break
            variant_attrs.append(am.group(1))
            chunk_no_attrs = chunk_no_attrs[am.end():].strip()
        if not chunk_no_attrs:
            continue
        explicit_rename = None
        for a in variant_attrs:
            rm = re.search(r'\brename\s*=\s*"([^"]+)"', a)
            if rm:
                explicit_rename = rm.group(1)
        vfields = []
        brace_pos = chunk_no_attrs.find("{")
        paren_pos = chunk_no_attrs.find("(")
        if brace_pos != -1 and (paren_pos == -1 or brace_pos < paren_pos):
            vname = chunk_no_attrs[:brace_pos].strip()
            vbody = chunk_no_attrs[brace_pos + 1:chunk_no_attrs.rfind("}")]
            vfields = [f for f in (parse_field_chunk(c, rename_all) for c in split_top_level(vbody, ",")) if f]
        elif paren_pos != -1:
            vname = chunk_no_attrs[:paren_pos].strip()
        else:
            vname = chunk_no_attrs.strip()
        if not vname or not re.match(r"^\w+$", vname):
            continue
        json_value = variant_json_value(vname, explicit_rename, rename_all)
        variants.append(RustEnumVariant(name=vname, json_value=json_value, fields=vfields))
    try:
        rel = path.relative_to(rust_root)
    except ValueError:
        rel = path
    return RustEnum(name=type_name, tag_key=tag, variants=variants, source_file=str(rel), source_kind=source_kind)


def resolve_rust_type(rust_root: Path, type_name: str):
    """Find and parse a Rust struct or enum by name. Returns (kind, RustStruct|RustEnum) or None."""
    found = find_type_definition(rust_root, type_name)
    if not found:
        return None
    kind, path, text, def_start, source_kind = found
    if kind == "struct":
        return "struct", parse_rust_struct(type_name, path, text, def_start, source_kind, rust_root)
    return "enum", parse_rust_enum(type_name, path, text, def_start, source_kind, rust_root)


# --------------------------------------------------------------------------- #
# .NET-side extraction
# --------------------------------------------------------------------------- #

def extract_loaded_symbols(dotnet_root: Path) -> set:
    path = dotnet_root / "PolyglotSql.Core" / "DynamicRustProvider.cs"
    if not path.is_file():
        return set()
    text = path.read_text(encoding="utf-8", errors="replace")
    return set(re.findall(r'LoadDelegate<\w+>\("(\w+)"\)', text))


DOTNET_PROP_RE = re.compile(
    r'\[JsonPropertyName\("([^"]+)"\)\]'
    r'(?:\s*\[[^\]]*\]\s*)*'
    r"\s*public\s+([\w<>\?\.\[\],\s]+?)\s+(\w+)\s*\{\s*get",
    re.DOTALL,
)


def find_dotnet_options_type(dotnet_root: Path, dotnet_file: str, type_name: str) -> Optional[DotnetOptionsType]:
    path = dotnet_root / dotnet_file
    if not path.is_file():
        return None
    text = path.read_text(encoding="utf-8", errors="replace")
    m = re.search(rf"\b(?:record|class)\s+{re.escape(type_name)}\b", text)
    if not m:
        return None
    # Scope the property scan to this type's body so sibling types in the same file
    # (e.g. DataType.cs) don't leak into each other's field lists.
    brace_idx = text.find("{", m.end())
    if brace_idx == -1:
        return DotnetOptionsType(name=type_name, file=dotnet_file, properties=[])
    body, _ = extract_braced_block(text, brace_idx)
    properties = [
        DotnetProperty(json_key=pm.group(1), dotnet_type=pm.group(2).strip(), name=pm.group(3))
        for pm in DOTNET_PROP_RE.finditer(body)
    ]
    return DotnetOptionsType(name=type_name, file=dotnet_file, properties=properties)


def find_dotnet_enum_type(dotnet_root: Path, dotnet_file: str, type_name: str) -> Optional[DotnetEnumType]:
    path = dotnet_root / dotnet_file
    if not path.is_file():
        return None
    text = path.read_text(encoding="utf-8", errors="replace")
    m = re.search(rf"\benum\s+{re.escape(type_name)}\s*\{{(.*?)\}}", text, re.DOTALL)
    if not m:
        return None
    members = []
    for chunk in split_top_level(m.group(1), ","):
        chunk = re.sub(r"//.*", "", chunk).strip()
        if not chunk:
            continue
        name = chunk.split("=")[0].strip()
        # Strip the '@' verbatim-identifier escape C# uses for reserved words (e.g.
        # `@virtual`); Enum.GetName()/JSON serialization sees "virtual", not "@virtual".
        name = name.lstrip("@")
        if name:
            members.append(name)
    # Some enums (Oracle-specific ones in DataType.cs) keep PascalCase member names
    # but serialize via SnakeCaseJsonEnumConverter, which snake_cases the member name
    # at write time - so the wire value isn't the literal member text lowercased.
    snake_case_converter = bool(re.search(
        rf"SnakeCaseJsonEnumConverter<{re.escape(type_name)}>.*?\benum\s+{re.escape(type_name)}\b",
        text, re.DOTALL,
    ))
    return DotnetEnumType(name=type_name, file=dotnet_file, members=members, snake_case_converter=snake_case_converter)


def find_record_body(text: str, class_name: str) -> Optional[str]:
    m = re.search(rf"\brecord\s+{re.escape(class_name)}\b\s*:\s*\w+", text)
    if not m:
        m = re.search(rf"\brecord\s+{re.escape(class_name)}\b", text)
        if not m:
            return None
    i = m.end()
    n = len(text)
    while i < n and text[i] in " \t\r\n":
        i += 1
    if i < n and text[i] == ";":
        return ""
    if i < n and text[i] == "{":
        body, _ = extract_braced_block(text, i)
        return body
    return None


POLY_ANCHOR_RE = re.compile(r'\[JsonPolymorphic\(TypeDiscriminatorPropertyName\s*=\s*"([^"]+)"\)\]')
DERIVED_LINE_RE = re.compile(r'\[JsonDerivedType\(typeof\((\w+)\),\s*"([^"]+)"\)\]')
RECORD_DECL_RE = re.compile(r"public\s+(?:abstract\s+|sealed\s+)*(?:partial\s+)*record\s+(\w+)\b")
WS_RE = re.compile(r"\s*")


def find_dotnet_tagged_union_type(dotnet_root: Path, dotnet_file: str, type_name: str) -> Optional[DotnetTaggedUnionType]:
    """Locate a [JsonPolymorphic]/[JsonDerivedType] declaration by name.

    Walked line-by-line (via successive anchored .match() calls at growing offsets)
    rather than one combined regex: a single regex with a repeated
    `[JsonDerivedType(...)]` group and a permissive `[^\\]]*` inside it is
    catastrophically slow on files with many derived-type attributes in a row
    (e.g. DataType.cs), since the engine tries every way to partition the run.
    """
    path = dotnet_root / dotnet_file
    if not path.is_file():
        return None
    text = path.read_text(encoding="utf-8", errors="replace")
    for anchor in POLY_ANCHOR_RE.finditer(text):
        pos = anchor.end()
        derived_pairs = []
        while True:
            pos = WS_RE.match(text, pos).end()
            dm = DERIVED_LINE_RE.match(text, pos)
            if not dm:
                break
            derived_pairs.append((dm.group(1), dm.group(2)))
            pos = dm.end()
        rm = RECORD_DECL_RE.match(text, pos)
        if not rm or rm.group(1) != type_name:
            continue
        tag_key = anchor.group(1)
        variants = {}
        for class_name, json_value in derived_pairs:
            body = find_record_body(text, class_name)
            props = []
            if body:
                props = [
                    DotnetProperty(json_key=pm.group(1), dotnet_type=pm.group(2).strip(), name=pm.group(3))
                    for pm in DOTNET_PROP_RE.finditer(body)
                ]
            variants[json_value] = props
        return DotnetTaggedUnionType(name=type_name, file=dotnet_file, tag_key=tag_key, variants=variants)
    return None


# --------------------------------------------------------------------------- #
# Soft type compatibility check
# --------------------------------------------------------------------------- #

def unwrap_rust_type(raw_type: str) -> str:
    """Strip Option<...> (nullability) and Box/Rc/Arc<...> (transparent for JSON shape)."""
    t = raw_type.strip()
    while True:
        m = re.match(r"(?:Option|Box|Rc|Arc)\s*<\s*(.+)\s*>$", t)
        if not m:
            return t
        t = m.group(1).strip()


def unwrap_cs_type(cs_type: str) -> str:
    cs_type = cs_type.strip()
    if cs_type.endswith("?"):
        cs_type = cs_type[:-1].strip()
    return cs_type


def rust_container(raw_type: str) -> Optional[str]:
    m = re.match(r"(\w+)\s*<", raw_type)
    return m.group(1) if m else None


def cs_container(cs_type: str) -> Optional[str]:
    if cs_type.endswith("[]"):
        return "Array"
    m = re.match(r"(\w+)\s*<", cs_type)
    if m and m.group(1) in CS_CONTAINER_NAMES:
        return "List" if m.group(1) in ("List", "IReadOnlyList", "IEnumerable", "ICollection") else "Dictionary"
    return None


def soft_type_check(rust_type: str, cs_type: str) -> bool:
    """Best-effort compatibility check; False means 'possible type drift', not a hard fact."""
    # Drop module paths (e.g. `crate::ComplexityGuardOptions`) - only the type name matters.
    rust_type = re.sub(r"\b(?:\w+::)+", "", rust_type)
    rust_inner = unwrap_rust_type(rust_type)
    cs_inner = unwrap_cs_type(cs_type)

    r_container = rust_container(rust_inner)
    if r_container == "Option":
        rust_inner = unwrap_rust_type(rust_inner)
        r_container = rust_container(rust_inner)

    if r_container in ("Vec", "HashSet", "BTreeSet"):
        return cs_container(cs_inner) in ("Array", "List")
    if r_container in ("HashMap", "BTreeMap"):
        return cs_container(cs_inner) == "Dictionary"

    if rust_inner in RUST_TO_CS_PRIMITIVES:
        return cs_inner in RUST_TO_CS_PRIMITIVES[rust_inner]

    # Custom type (struct/enum name): a manifest entry (e.g. DialectType -> Dialect)
    # means the names are expected to differ, so resolve through it before comparing.
    cs_bare = re.sub(r"[<>\[\]]", "", cs_inner).rstrip("[]").lower()
    mapped_dotnet_type = _MANIFEST_DOTNET_BY_RUST.get(rust_inner)
    if mapped_dotnet_type:
        return mapped_dotnet_type.lower() == cs_bare
    return rust_inner.lower() == cs_bare


# --------------------------------------------------------------------------- #
# Manifest
# --------------------------------------------------------------------------- #

def load_manifest(path: Path) -> dict:
    data = json.loads(path.read_text(encoding="utf-8"))
    return data.get("types", {})


# --------------------------------------------------------------------------- #
# Diff engine
# --------------------------------------------------------------------------- #

def diff_ffi_functions(capability_symbols: dict, loaded_symbols: set) -> list:
    findings = []
    if capability_symbols is None:
        findings.append(Finding(
            category="ffi_function", severity="warning", rust_name="docs/api-capabilities.json",
            dotnet_name=None,
            message="api-capabilities.json not found under the given --rust-path; "
                    "skipped FFI function inventory check.",
        ))
        return findings
    missing_in_dotnet = sorted(set(capability_symbols) - loaded_symbols)
    extra_in_dotnet = sorted(loaded_symbols - set(capability_symbols))
    for sym in missing_in_dotnet:
        findings.append(Finding(
            category="ffi_function", severity="drift", rust_name=sym, dotnet_name=None,
            message=f"Rust FFI exposes '{sym}' (capability: {capability_symbols[sym]}) "
                    f"but DynamicRustProvider.cs never loads it.",
        ))
    for sym in extra_in_dotnet:
        findings.append(Finding(
            category="ffi_function", severity="warning", rust_name=sym, dotnet_name=sym,
            message=f".NET wrapper loads '{sym}' which api-capabilities.json does not list as "
                    f"supported/partial - possibly renamed or removed upstream.",
        ))
    return findings


def diff_no_mangle_advisory(capability_symbols: Optional[dict], no_mangle_symbols: set) -> list:
    findings = []
    if capability_symbols is None or not no_mangle_symbols:
        return findings
    cap_set = set(capability_symbols)
    for sym in sorted(no_mangle_symbols - cap_set):
        findings.append(Finding(
            category="ffi_contract_advisory", severity="info", rust_name=sym, dotnet_name=None,
            message=f"'{sym}' is exported (#[no_mangle]) but not listed in api-capabilities.json "
                    f"- the upstream contract file may be stale.",
        ))
    return findings


def diff_field_sets(rust_type_name: str, dotnet_type_name: Optional[str],
                     rust_fields: list, dotnet_properties: list, category: str) -> list:
    findings = []
    rust_by_key = {f.json_key: f for f in rust_fields}
    dotnet_by_key = {p.json_key: p for p in dotnet_properties}

    for key in sorted(set(rust_by_key) - set(dotnet_by_key)):
        f = rust_by_key[key]
        nested = extract_custom_type_names(f.rust_type)
        note = f" (nested type: {nested[0]})" if nested else ""
        findings.append(Finding(
            category=category, severity="drift", rust_name=rust_type_name, dotnet_name=dotnet_type_name,
            message=f"Rust field '{key}' ({f.rust_type}) has no matching property on {dotnet_type_name or '(unmapped)'}{note}.",
            detail={"json_key": key, "rust_type": f.rust_type},
        ))
    for key in sorted(set(dotnet_by_key) - set(rust_by_key)):
        p = dotnet_by_key[key]
        findings.append(Finding(
            category=category, severity="drift", rust_name=rust_type_name, dotnet_name=dotnet_type_name,
            message=f".NET property '{p.name}' (json key '{key}') on {dotnet_type_name} has no "
                    f"matching field on Rust {rust_type_name} - may be obsolete.",
            detail={"json_key": key, "dotnet_type": p.dotnet_type},
        ))
    for key in sorted(set(rust_by_key) & set(dotnet_by_key)):
        f = rust_by_key[key]
        p = dotnet_by_key[key]
        if not soft_type_check(f.rust_type, p.dotnet_type):
            findings.append(Finding(
                category=category, severity="warning", rust_name=rust_type_name, dotnet_name=dotnet_type_name,
                message=f"Field '{key}': Rust type '{f.rust_type}' and .NET type '{p.dotnet_type}' "
                        f"may no longer be compatible (soft check, verify manually).",
                detail={"json_key": key, "rust_type": f.rust_type, "dotnet_type": p.dotnet_type},
            ))
    return findings


def diff_options_struct(rust_struct: RustStruct, dotnet_type: Optional[DotnetOptionsType], dotnet_name: Optional[str]) -> list:
    dotnet_props = dotnet_type.properties if dotnet_type else []
    return diff_field_sets(rust_struct.name, dotnet_name, rust_struct.fields, dotnet_props, category="options_field")


def diff_plain_enum(rust_enum: RustEnum, dotnet_type: Optional[DotnetEnumType], dotnet_name: Optional[str],
                    dotnet_only_members: Optional[list] = None) -> list:
    """Compare wire-value sets case-insensitively.

    `dotnet_only_members` (manifest key of the same name) lists .NET members that
    intentionally have no Rust counterpart (e.g. a parse fallback like
    DiffEditType.unknown); they are not reported as obsolete.

    Some .NET enums (e.g. Dialect) keep PascalCase member names but serialize via a
    custom converter that lowercases on write (see DialectJsonConverter) - so the
    literal member text legitimately differs in case from the Rust wire value even
    when there's no real drift. Others (UnsupportedLevel, OpenLineageRunEventType)
    already declare members in the exact wire casing. Comparing case-insensitively
    covers both without needing to detect which converter is in play.
    """
    findings = []
    rust_values = {v.json_value: v.name for v in rust_enum.variants}
    dotnet_members = list(dotnet_type.members) if dotnet_type else []
    rust_lower = {k.lower(): (k, v) for k, v in rust_values.items()}
    if dotnet_type and dotnet_type.snake_case_converter:
        dotnet_lower = {pascal_to_snake(m).lower(): m for m in dotnet_members}
    else:
        dotnet_lower = {m.lower(): m for m in dotnet_members}

    for lower_value, (value, rust_variant_name) in sorted(rust_lower.items()):
        if lower_value not in dotnet_lower:
            findings.append(Finding(
                category="enum_member", severity="drift", rust_name=rust_enum.name, dotnet_name=dotnet_name,
                message=f"Rust variant '{rust_variant_name}' (wire value '{value}') has no matching "
                        f"member on {dotnet_name or '(unmapped)'}.",
                detail={"json_value": value},
            ))
    allowed_extra = {m.lower() for m in (dotnet_only_members or [])}
    for lower_member, member in sorted(dotnet_lower.items()):
        if lower_member not in rust_lower and lower_member not in allowed_extra:
            findings.append(Finding(
                category="enum_member", severity="drift", rust_name=rust_enum.name, dotnet_name=dotnet_name,
                message=f".NET member '{member}' on {dotnet_name} has no matching Rust variant - may be obsolete.",
                detail={"json_value": member},
            ))
    return findings


def diff_tagged_union(rust_enum: RustEnum, dotnet_type: Optional[DotnetTaggedUnionType], dotnet_name: Optional[str]) -> list:
    findings = []
    rust_variants = {v.json_value: v for v in rust_enum.variants}
    dotnet_variants = dotnet_type.variants if dotnet_type else {}

    for value, variant in sorted(rust_variants.items()):
        if value not in dotnet_variants:
            findings.append(Finding(
                category="tagged_union_variant", severity="drift", rust_name=rust_enum.name, dotnet_name=dotnet_name,
                message=f"Rust variant '{variant.name}' (discriminator '{value}') has no matching "
                        f"[JsonDerivedType] on {dotnet_name or '(unmapped)'}.",
                detail={"json_value": value},
            ))
    for value in sorted(set(dotnet_variants) - set(rust_variants)):
        findings.append(Finding(
            category="tagged_union_variant", severity="drift", rust_name=rust_enum.name, dotnet_name=dotnet_name,
            message=f".NET [JsonDerivedType] discriminator '{value}' on {dotnet_name} has no matching "
                    f"Rust variant - may be obsolete.",
            detail={"json_value": value},
        ))
    for value in sorted(set(rust_variants) & set(dotnet_variants)):
        rust_fields = rust_variants[value].fields
        dotnet_props = dotnet_variants[value]
        findings.extend(diff_field_sets(
            f"{rust_enum.name}::{rust_variants[value].name}",
            f"{dotnet_name} (discriminator '{value}')" if dotnet_name else None,
            rust_fields, dotnet_props, category="tagged_union_field",
        ))
    return findings


# --------------------------------------------------------------------------- #
# Orchestration
# --------------------------------------------------------------------------- #

def read_all(files) -> dict:
    return {p: p.read_text(encoding="utf-8", errors="replace") for p in files}


def run(rust_root: Path, dotnet_root: Path, manifest_path: Path):
    findings = []
    manifest = load_manifest(manifest_path)
    _RUST_FILE_TEXT_CACHE.clear()
    _MANIFEST_DOTNET_BY_RUST.clear()
    _MANIFEST_DOTNET_BY_RUST.update({
        rust_name: entry["dotnet_type"]
        for rust_name, entry in manifest.items()
        if entry.get("dotnet_type")
    })

    # --- 1. FFI function inventory ---
    capability_symbols = load_capability_ffi_symbols(rust_root)
    loaded_symbols = extract_loaded_symbols(dotnet_root)
    findings.extend(diff_ffi_functions(capability_symbols, loaded_symbols))
    no_mangle_symbols = extract_no_mangle_symbols(rust_root)
    findings.extend(diff_no_mangle_advisory(capability_symbols, no_mangle_symbols))

    # --- 2. Discover which Rust "options" types are actually reachable from FFI calls ---
    ffi_src_dir = rust_root / "crates" / "polyglot-sql-ffi" / "src"
    ffi_files = read_all(sorted(ffi_src_dir.rglob("*.rs"))) if ffi_src_dir.is_dir() else {}
    discovered_options_types = set()
    if capability_symbols:
        for symbol in capability_symbols:
            found_type = find_options_type_for_function(ffi_files, symbol)
            if found_type:
                discovered_options_types.add(found_type)

    # --- 3. Compare every manifest entry (this is the primary coverage mechanism) ---
    resolved_cache = {}
    referenced_type_names = set()

    def resolve_and_cache(name):
        if name not in resolved_cache:
            resolved_cache[name] = resolve_rust_type(rust_root, name)
        return resolved_cache[name]

    for rust_name, entry in sorted(manifest.items()):
        resolved = resolve_and_cache(rust_name)
        dotnet_type_name = entry.get("dotnet_type")
        kind = entry.get("kind")
        dotnet_file = entry.get("dotnet_file")

        if resolved is None:
            findings.append(Finding(
                category="rust_type_missing", severity="drift", rust_name=rust_name, dotnet_name=dotnet_type_name,
                message=f"Rust type '{rust_name}' referenced by the manifest was not found in "
                        f"--rust-path - it may have been renamed or removed upstream.",
            ))
            continue

        actual_kind, parsed = resolved
        # Opaque types are exposed to .NET as a thin JSON wrapper (like Expression), so
        # neither their fields nor the types those fields reference are compared.
        if kind == "opaque":
            continue
        for f in (parsed.fields if actual_kind == "struct" else
                  [f for v in parsed.variants for f in v.fields]):
            referenced_type_names.update(extract_custom_type_names(f.rust_type))

        if dotnet_type_name is None:
            note = entry.get("notes", "")
            field_count = len(parsed.fields) if actual_kind == "struct" else len(parsed.variants)
            findings.append(Finding(
                category="unimplemented", severity="info", rust_name=rust_name, dotnet_name=None,
                message=f"'{rust_name}' has no .NET mapping yet ({field_count} "
                        f"{'field' if actual_kind == 'struct' else 'variant'}(s) in Rust). {note}".strip(),
            ))
            continue

        if kind == "options":
            if actual_kind != "struct":
                findings.append(Finding(
                    category="manifest_kind_mismatch", severity="warning", rust_name=rust_name,
                    dotnet_name=dotnet_type_name,
                    message=f"Manifest says '{rust_name}' is kind=options but it resolved to a Rust enum.",
                ))
                continue
            dotnet_type = find_dotnet_options_type(dotnet_root, dotnet_file, dotnet_type_name) if dotnet_file else None
            if dotnet_file and dotnet_type is None:
                findings.append(Finding(
                    category="dotnet_type_missing", severity="drift", rust_name=rust_name, dotnet_name=dotnet_type_name,
                    message=f".NET type '{dotnet_type_name}' not found in {dotnet_file}.",
                ))
            findings.extend(diff_options_struct(parsed, dotnet_type, dotnet_type_name))

        elif kind == "enum":
            if actual_kind != "enum":
                findings.append(Finding(
                    category="manifest_kind_mismatch", severity="warning", rust_name=rust_name,
                    dotnet_name=dotnet_type_name,
                    message=f"Manifest says '{rust_name}' is kind=enum but it resolved to a Rust struct.",
                ))
                continue
            dotnet_type = find_dotnet_enum_type(dotnet_root, dotnet_file, dotnet_type_name) if dotnet_file else None
            if dotnet_file and dotnet_type is None:
                findings.append(Finding(
                    category="dotnet_type_missing", severity="drift", rust_name=rust_name, dotnet_name=dotnet_type_name,
                    message=f".NET enum '{dotnet_type_name}' not found in {dotnet_file}.",
                ))
            findings.extend(diff_plain_enum(parsed, dotnet_type, dotnet_type_name,
                                            entry.get("dotnet_only_members")))

        elif kind == "tagged_union":
            if actual_kind != "enum":
                findings.append(Finding(
                    category="manifest_kind_mismatch", severity="warning", rust_name=rust_name,
                    dotnet_name=dotnet_type_name,
                    message=f"Manifest says '{rust_name}' is kind=tagged_union but it resolved to a Rust struct.",
                ))
                continue
            dotnet_type = find_dotnet_tagged_union_type(dotnet_root, dotnet_file, dotnet_type_name) if dotnet_file else None
            if dotnet_file and dotnet_type is None:
                findings.append(Finding(
                    category="dotnet_type_missing", severity="drift", rust_name=rust_name, dotnet_name=dotnet_type_name,
                    message=f".NET tagged union '{dotnet_type_name}' not found in {dotnet_file}.",
                ))
            findings.extend(diff_tagged_union(parsed, dotnet_type, dotnet_type_name))

    # --- 4. Surface Rust types referenced from FFI calls or mapped-type fields that
    #         aren't in the manifest at all ---
    all_discovered = discovered_options_types | referenced_type_names
    unmapped = sorted(n for n in all_discovered if n not in manifest and n[:1].isupper())
    for name in unmapped:
        findings.append(Finding(
            category="unmapped_type", severity="info", rust_name=name, dotnet_name=None,
            message=f"'{name}' is referenced from an FFI options parameter or a mapped type's field "
                    f"but has no entry in {manifest_path.name} - review whether the wrapper needs it.",
        ))

    return findings, {
        "capability_symbols_count": len(capability_symbols) if capability_symbols else 0,
        "loaded_symbols_count": len(loaded_symbols),
        "manifest_entries": len(manifest),
    }


# --------------------------------------------------------------------------- #
# Report rendering
# --------------------------------------------------------------------------- #

SEVERITY_ORDER = {"drift": 0, "warning": 1, "info": 2}

LIMITATIONS_TEXT = """\
Limitations of this tool (regex/brace-based parsing, not a real Rust/C# parser):
  - Cannot see fields injected by proc macros; only literal source text is scanned.
  - Custom `#[serde(deserialize_with = ...)]`/`serialize_with` functions are treated
    as opaque - the field's JSON key/type is still compared, but the custom logic
    behind it is not verified.
  - A field rename looks identical to a remove+add (one drift line each); read
    added/removed pairs together before concluding a field truly disappeared.
  - Generic containers are compared structurally only (Vec<T> vs List<T>/T[]), not
    recursively into T.
  - Manifest coverage is opt-in and hand-maintained (see polyglot_api_drift_manifest.json).
    Only types reachable from an FFI options parameter, or referenced by a field of
    an already-mapped type, are eligible for the automatic "unmapped type" check.
"""


def render_report(findings: list, stats: dict, rust_root: Path, dotnet_root: Path) -> str:
    lines = []
    lines.append("# Polyglot Rust <-> .NET API drift report")
    lines.append("")
    lines.append(f"- Rust source:   {rust_root}")
    lines.append(f"- .NET source:   {dotnet_root}")
    lines.append(f"- Rust FFI symbols (from api-capabilities.json): {stats['capability_symbols_count']}")
    lines.append(f"- .NET bound FFI symbols (DynamicRustProvider.cs): {stats['loaded_symbols_count']}")
    lines.append(f"- Manifest-tracked Rust types: {stats['manifest_entries']}")
    lines.append("")

    by_severity = {"drift": 0, "warning": 0, "info": 0}
    for f in findings:
        by_severity[f.severity] = by_severity.get(f.severity, 0) + 1
    lines.append(f"## Summary: {by_severity['drift']} drift, {by_severity['warning']} warning, "
                 f"{by_severity['info']} info ({len(findings)} total)")
    lines.append("")

    sections = [
        ("FFI Function Drift", {"ffi_function"}),
        ("FFI Contract Advisory", {"ffi_contract_advisory"}),
        ("Mapped Type Drift", {"options_field", "enum_member", "tagged_union_variant", "tagged_union_field",
                                "dotnet_type_missing", "rust_type_missing", "manifest_kind_mismatch"}),
        ("Not Yet Implemented in .NET", {"unimplemented"}),
        ("Unmapped Rust Types", {"unmapped_type"}),
    ]
    for title, categories in sections:
        section_findings = sorted(
            (f for f in findings if f.category in categories),
            key=lambda f: (SEVERITY_ORDER.get(f.severity, 9), f.rust_name),
        )
        lines.append(f"## {title} ({len(section_findings)})")
        if not section_findings:
            lines.append("(none)")
        for f in section_findings:
            lines.append(f"- [{f.severity.upper()}] {f.message}")
        lines.append("")

    lines.append("## Notes / Limitations")
    lines.append(LIMITATIONS_TEXT)
    return "\n".join(lines)


# --------------------------------------------------------------------------- #
# CLI
# --------------------------------------------------------------------------- #

def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--rust-path", type=Path, default=REPO_ROOT / "polyglot-main",
                         help="Path to the vendored/checked-out Rust polyglot source (default: %(default)s)")
    parser.add_argument("--dotnet-path", type=Path, default=REPO_ROOT / "src",
                         help="Path to the .NET wrapper source (default: %(default)s)")
    parser.add_argument("--manifest", type=Path, default=SCRIPT_DIR / "polyglot_api_drift_manifest.json",
                         help="Path to the Rust-type -> .NET-type mapping manifest (default: %(default)s)")
    parser.add_argument("--json-out", type=Path, default=None,
                         help="Optional path to also write a machine-readable JSON report")
    parser.add_argument("--fail-on", choices=["none", "drift", "any"], default="drift",
                         help="Exit code 1 when findings at/above this severity exist (default: %(default)s)")
    parser.add_argument("--quiet", action="store_true", help="Suppress the console report")
    args = parser.parse_args(argv)

    rust_root = args.rust_path.resolve()
    dotnet_root = args.dotnet_path.resolve()
    manifest_path = args.manifest.resolve()

    if not rust_root.is_dir():
        print(f"error: --rust-path '{rust_root}' is not a directory", file=sys.stderr)
        return 2
    if not dotnet_root.is_dir():
        print(f"error: --dotnet-path '{dotnet_root}' is not a directory", file=sys.stderr)
        return 2
    if not manifest_path.is_file():
        print(f"error: --manifest '{manifest_path}' does not exist", file=sys.stderr)
        return 2

    findings, stats = run(rust_root, dotnet_root, manifest_path)

    if not args.quiet:
        print(render_report(findings, stats, rust_root, dotnet_root))

    if args.json_out:
        payload = {
            "rust_path": str(rust_root),
            "dotnet_path": str(dotnet_root),
            "stats": stats,
            "findings": [f.to_dict() for f in findings],
        }
        args.json_out.write_text(json.dumps(payload, indent=2), encoding="utf-8")
        if not args.quiet:
            print(f"\nJSON report written to {args.json_out}")

    if args.fail_on == "none":
        return 0
    threshold = {"drift"} if args.fail_on == "drift" else {"drift", "warning"}
    return 1 if any(f.severity in threshold for f in findings) else 0


if __name__ == "__main__":
    sys.exit(main())
