"""Regenerate src/PolyglotSql.Core/Models/TokenType.cs from the Rust TokenType enum.

- Member names are the serde SCREAMING_SNAKE_CASE wire names (what polyglot_tokenize emits).
- Members that already exist in .NET (exactly, or with the same letters but different
  underscores, e.g. DCOLON -> D_COLON) keep their numeric value.
- New members get values after the current maximum.
- Members follow the Rust declaration order.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
RUST_FILE = ROOT / "polyglot-main/crates/polyglot-sql/src/tokens.rs"
CS_FILE = ROOT / "src/PolyglotSql.Core/Models/TokenType.cs"


def screaming(name):
    out = []
    for i, c in enumerate(name):
        if c.isupper() and i > 0:
            out.append("_")
        out.append(c.upper())
    return "".join(out)


def rust_variants():
    text = RUST_FILE.read_text(encoding="utf-8")
    start = text.index("{", text.index("pub enum TokenType {")) + 1
    depth, i = 1, start
    while depth:
        depth += {"{": 1, "}": -1}.get(text[i], 0)
        i += 1
    body = re.sub(r"//[^\n]*", "", text[start:i - 1])
    body = re.sub(r"#\[[^\]]*\]", "", body)
    variants = [v.strip() for v in body.split(",") if v.strip()]
    assert all(re.fullmatch(r"\w+", v) for v in variants), "unexpected variant syntax"
    return [screaming(v) for v in variants]


def cs_members():
    text = CS_FILE.read_text(encoding="utf-8-sig")
    body = text[text.index("{", text.index("enum TokenType")) + 1:]
    body = body[:body.index("}")]
    members = {}
    for line in body.splitlines():
        line = re.sub(r"//.*", "", line).strip().rstrip(",")
        if line:
            name, _, value = line.partition("=")
            members[name.strip()] = int(value)
    return members


def main():
    rust = rust_variants()
    existing = cs_members()
    by_norm = {n.replace("_", ""): v for n, v in existing.items()}
    next_value = max(existing.values()) + 1
    lines = []
    used = set()
    for name in rust:
        value = existing.get(name, by_norm.get(name.replace("_", "")))
        # new member, or a value already taken (the old file had GROUP_BY = GROUPING_SETS = 289)
        if value is None or value in used:
            value, next_value = next_value, next_value + 1
        used.add(value)
        lines.append(f"        {name} = {value},")
    values = [int(l.rsplit("=", 1)[1].rstrip(",")) for l in lines]
    assert len(values) == len(set(values)), "duplicate numeric values"

    removed = sorted(n for n in existing if n not in rust and n.replace("_", "") not in {r.replace("_", "") for r in rust})
    content = (
        "namespace PolyglotSql.Models\n"
        "{\n"
        "    // Mirrors the native Rust `TokenType` enum. Member names are the serde\n"
        "    // SCREAMING_SNAKE_CASE wire names emitted by polyglot_tokenize (e.g. DColon -> D_COLON).\n"
        "    // Numeric values are .NET-only and kept stable across updates; new members are appended.\n"
        "    public enum TokenType\n"
        "    {\n"
        + "\n".join(lines).rstrip(",") + "\n"
        "    }\n"
        "}\n"
    )
    if "--write" in sys.argv:
        CS_FILE.write_text(content, encoding="utf-8")
    print(f"rust: {len(rust)}, existing: {len(existing)}, new values: {next_value - max(existing.values()) - 1}, removed: {removed}")
    print("GROUP_BY / GROUPING_SETS:", [l.strip() for l in lines if "GROUP_BY" in l or "GROUPING_SETS" in l])


if __name__ == "__main__":
    main()
