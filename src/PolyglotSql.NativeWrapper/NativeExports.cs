using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SqlGlotDotNet.NativeWrapper
{
    public static class NativeExports
    {
        private static readonly Dictionary<string, int> TokenTypeMap = new Dictionary<string, int>
        {
            {"L_PAREN", 1},
            {"R_PAREN", 2},
            {"L_BRACKET", 3},
            {"R_BRACKET", 4},
            {"L_BRACE", 5},
            {"R_BRACE", 6},
            {"COMMA", 7},
            {"DOT", 8},
            {"DASH", 9},
            {"PLUS", 10},
            {"COLON", 11},
            {"DOTCOLON", 12},
            {"DOTCARET", 13},
            {"DCOLON", 14},
            {"DCOLONDOLLAR", 15},
            {"DCOLONPERCENT", 16},
            {"DCOLONQMARK", 17},
            {"DQMARK", 18},
            {"SEMICOLON", 19},
            {"STAR", 20},
            {"BACKSLASH", 21},
            {"SLASH", 22},
            {"LT", 23},
            {"LTE", 24},
            {"GT", 25},
            {"GTE", 26},
            {"NOT", 27},
            {"EQ", 28},
            {"NEQ", 29},
            {"NULLSAFE_EQ", 30},
            {"COLON_EQ", 31},
            {"COLON_GT", 32},
            {"NCOLON_GT", 33},
            {"AND", 34},
            {"OR", 35},
            {"AMP", 36},
            {"DPIPE", 37},
            {"PIPE_GT", 38},
            {"PIPE", 39},
            {"PIPE_SLASH", 40},
            {"DPIPE_SLASH", 41},
            {"CARET", 42},
            {"CARET_AT", 43},
            {"TILDE", 44},
            {"ARROW", 45},
            {"DARROW", 46},
            {"FARROW", 47},
            {"HASH", 48},
            {"HASH_ARROW", 49},
            {"DHASH_ARROW", 50},
            {"LR_ARROW", 51},
            {"LLRR_ARROW", 52},
            {"DAT", 53},
            {"LT_AT", 54},
            {"AT_GT", 55},
            {"DOLLAR", 56},
            {"PARAMETER", 57},
            {"SESSION", 58},
            {"SESSION_PARAMETER", 59},
            {"SESSION_USER", 60},
            {"DAMP", 61},
            {"AMP_LT", 62},
            {"AMP_GT", 63},
            {"ADJACENT", 64},
            {"XOR", 65},
            {"DSTAR", 66},
            {"QMARK_AMP", 67},
            {"QMARK_PIPE", 68},
            {"HASH_DASH", 69},
            {"EXCLAMATION", 70},
            {"URI_START", 71},
            {"BLOCK_START", 72},
            {"BLOCK_END", 73},
            {"SPACE", 74},
            {"BREAK", 75},
            {"STRING", 76},
            {"NUMBER", 77},
            {"IDENTIFIER", 78},
            {"DATABASE", 79},
            {"COLUMN", 80},
            {"COLUMN_DEF", 81},
            {"SCHEMA", 82},
            {"TABLE", 83},
            {"WAREHOUSE", 84},
            {"STAGE", 85},
            {"STREAM", 86},
            {"STREAMLIT", 87},
            {"VAR", 88},
            {"BIT_STRING", 89},
            {"HEX_STRING", 90},
            {"BYTE_STRING", 91},
            {"NATIONAL_STRING", 92},
            {"RAW_STRING", 93},
            {"HEREDOC_STRING", 94},
            {"UNICODE_STRING", 95},
            {"BIT", 96},
            {"BOOLEAN", 97},
            {"TINYINT", 98},
            {"UTINYINT", 99},
            {"SMALLINT", 100},
            {"USMALLINT", 101},
            {"MEDIUMINT", 102},
            {"UMEDIUMINT", 103},
            {"INT", 104},
            {"UINT", 105},
            {"BIGINT", 106},
            {"UBIGINT", 107},
            {"BIGNUM", 108},
            {"INT128", 109},
            {"UINT128", 110},
            {"INT256", 111},
            {"UINT256", 112},
            {"FLOAT", 113},
            {"DOUBLE", 114},
            {"UDOUBLE", 115},
            {"DECIMAL", 116},
            {"DECIMAL32", 117},
            {"DECIMAL64", 118},
            {"DECIMAL128", 119},
            {"DECIMAL256", 120},
            {"DECFLOAT", 121},
            {"UDECIMAL", 122},
            {"BIGDECIMAL", 123},
            {"CHAR", 124},
            {"NCHAR", 125},
            {"VARCHAR", 126},
            {"NVARCHAR", 127},
            {"BPCHAR", 128},
            {"TEXT", 129},
            {"MEDIUMTEXT", 130},
            {"LONGTEXT", 131},
            {"BLOB", 132},
            {"MEDIUMBLOB", 133},
            {"LONGBLOB", 134},
            {"TINYBLOB", 135},
            {"TINYTEXT", 136},
            {"NAME", 137},
            {"BINARY", 138},
            {"VARBINARY", 139},
            {"JSON", 140},
            {"JSONB", 141},
            {"TIME", 142},
            {"TIMETZ", 143},
            {"TIME_NS", 144},
            {"TIMESTAMP", 145},
            {"TIMESTAMPTZ", 146},
            {"TIMESTAMPLTZ", 147},
            {"TIMESTAMPNTZ", 148},
            {"TIMESTAMP_S", 149},
            {"TIMESTAMP_MS", 150},
            {"TIMESTAMP_NS", 151},
            {"DATETIME", 152},
            {"DATETIME2", 153},
            {"DATETIME64", 154},
            {"SMALLDATETIME", 155},
            {"DATE", 156},
            {"DATE32", 157},
            {"INT4RANGE", 158},
            {"INT4MULTIRANGE", 159},
            {"INT8RANGE", 160},
            {"INT8MULTIRANGE", 161},
            {"NUMRANGE", 162},
            {"NUMMULTIRANGE", 163},
            {"TSRANGE", 164},
            {"TSMULTIRANGE", 165},
            {"TSTZRANGE", 166},
            {"TSTZMULTIRANGE", 167},
            {"DATERANGE", 168},
            {"DATEMULTIRANGE", 169},
            {"UUID", 170},
            {"GEOGRAPHY", 171},
            {"GEOGRAPHYPOINT", 172},
            {"NULLABLE", 173},
            {"GEOMETRY", 174},
            {"POINT", 175},
            {"RING", 176},
            {"LINESTRING", 177},
            {"LOCALTIME", 178},
            {"LOCALTIMESTAMP", 179},
            {"SYSTIMESTAMP", 180},
            {"MULTILINESTRING", 181},
            {"POLYGON", 182},
            {"MULTIPOLYGON", 183},
            {"HLLSKETCH", 184},
            {"HSTORE", 185},
            {"SUPER", 186},
            {"SERIAL", 187},
            {"SMALLSERIAL", 188},
            {"BIGSERIAL", 189},
            {"XML", 190},
            {"YEAR", 191},
            {"USERDEFINED", 192},
            {"MONEY", 193},
            {"SMALLMONEY", 194},
            {"ROWVERSION", 195},
            {"IMAGE", 196},
            {"VARIANT", 197},
            {"OBJECT", 198},
            {"INET", 199},
            {"IPADDRESS", 200},
            {"IPPREFIX", 201},
            {"IPV4", 202},
            {"IPV6", 203},
            {"ENUM", 204},
            {"ENUM8", 205},
            {"ENUM16", 206},
            {"FIXEDSTRING", 207},
            {"LOWCARDINALITY", 208},
            {"NESTED", 209},
            {"AGGREGATEFUNCTION", 210},
            {"SIMPLEAGGREGATEFUNCTION", 211},
            {"TDIGEST", 212},
            {"UNKNOWN", 213},
            {"VECTOR", 214},
            {"DYNAMIC", 215},
            {"VOID", 216},
            {"ALIAS", 217},
            {"ALTER", 218},
            {"ALL", 219},
            {"ANTI", 220},
            {"ANY", 221},
            {"APPLY", 222},
            {"ARRAY", 223},
            {"ASC", 224},
            {"ASOF", 225},
            {"ATTACH", 226},
            {"AUTO_INCREMENT", 227},
            {"BEGIN", 228},
            {"BETWEEN", 229},
            {"BULK_COLLECT_INTO", 230},
            {"CACHE", 231},
            {"CASE", 232},
            {"CHARACTER_SET", 233},
            {"CLUSTER_BY", 234},
            {"COLLATE", 235},
            {"COMMAND", 236},
            {"COMMENT", 237},
            {"COMMIT", 238},
            {"CONNECT_BY", 239},
            {"CONSTRAINT", 240},
            {"COPY", 241},
            {"CREATE", 242},
            {"CROSS", 243},
            {"CUBE", 244},
            {"CURRENT_DATE", 245},
            {"CURRENT_DATETIME", 246},
            {"CURRENT_SCHEMA", 247},
            {"CURRENT_TIME", 248},
            {"CURRENT_TIMESTAMP", 249},
            {"CURRENT_USER", 250},
            {"CURRENT_USER_ID", 251},
            {"CURRENT_ROLE", 252},
            {"CURRENT_CATALOG", 253},
            {"DECLARE", 254},
            {"DEFAULT", 255},
            {"DELETE", 256},
            {"DESC", 257},
            {"DESCRIBE", 258},
            {"DETACH", 259},
            {"DICTIONARY", 260},
            {"DISTINCT", 261},
            {"DISTRIBUTE_BY", 262},
            {"DIV", 263},
            {"DROP", 264},
            {"ELSE", 265},
            {"END", 266},
            {"ESCAPE", 267},
            {"EXCEPT", 268},
            {"EXECUTE", 269},
            {"EXISTS", 270},
            {"FALSE", 271},
            {"FETCH", 272},
            {"FILE", 273},
            {"FILE_FORMAT", 274},
            {"FILTER", 275},
            {"FINAL", 276},
            {"FIRST", 277},
            {"FOR", 278},
            {"FORCE", 279},
            {"FOREIGN_KEY", 280},
            {"FORMAT", 281},
            {"FROM", 282},
            {"FULL", 283},
            {"FUNCTION", 284},
            {"GET", 285},
            {"GLOB", 286},
            {"GLOBAL", 287},
            {"GRANT", 288},
            {"GROUP_BY", 289},
            {"GROUPING_SETS", 290},
            {"HAVING", 291},
            {"HINT", 292},
            {"IGNORE", 293},
            {"ILIKE", 294},
            {"IN", 295},
            {"INDEX", 296},
            {"INDEXED_BY", 297},
            {"INNER", 298},
            {"INSERT", 299},
            {"INSTALL", 300},
            {"INTEGRATION", 301},
            {"INTERSECT", 302},
            {"INTERVAL", 303},
            {"INTO", 304},
            {"INTRODUCER", 305},
            {"IRLIKE", 306},
            {"IS", 307},
            {"ISNULL", 308},
            {"JOIN", 309},
            {"JOIN_MARKER", 310},
            {"KEEP", 311},
            {"KEY", 312},
            {"KILL", 313},
            {"LANGUAGE", 314},
            {"LATERAL", 315},
            {"LEFT", 316},
            {"LIKE", 317},
            {"LIMIT", 318},
            {"LIST", 319},
            {"LOAD", 320},
            {"LOCK", 321},
            {"MAP", 322},
            {"MATCH", 323},
            {"MATCH_CONDITION", 324},
            {"MATCH_RECOGNIZE", 325},
            {"MEMBER_OF", 326},
            {"MERGE", 327},
            {"MOD", 328},
            {"MODEL", 329},
            {"NATURAL", 330},
            {"NEXT", 331},
            {"NOTHING", 332},
            {"NOTNULL", 333},
            {"NULL", 334},
            {"OBJECT_IDENTIFIER", 335},
            {"OFFSET", 336},
            {"ON", 337},
            {"ONLY", 338},
            {"OPERATOR", 339},
            {"ORDER_BY", 340},
            {"ORDER_SIBLINGS_BY", 341},
            {"ORDERED", 342},
            {"ORDINALITY", 343},
            {"OUT", 344},
            {"INOUT", 345},
            {"OUTER", 346},
            {"OVER", 347},
            {"OVERLAPS", 348},
            {"OVERWRITE", 349},
            {"PACKAGE", 350},
            {"PARTITION", 351},
            {"PARTITION_BY", 352},
            {"PERCENT", 353},
            {"PIVOT", 354},
            {"PLACEHOLDER", 355},
            {"POLICY", 356},
            {"POOL", 357},
            {"POSITIONAL", 358},
            {"PRAGMA", 359},
            {"PREWHERE", 360},
            {"PRIMARY_KEY", 361},
            {"PROCEDURE", 362},
            {"PROPERTIES", 363},
            {"PSEUDO_TYPE", 364},
            {"PUT", 365},
            {"QUALIFY", 366},
            {"QUOTE", 367},
            {"QDCOLON", 368},
            {"RANGE", 369},
            {"RECURSIVE", 370},
            {"REFRESH", 371},
            {"RENAME", 372},
            {"REPLACE", 373},
            {"RETURNING", 374},
            {"REVOKE", 375},
            {"REFERENCES", 376},
            {"RIGHT", 377},
            {"RLIKE", 378},
            {"ROLE", 379},
            {"ROLLBACK", 380},
            {"ROLLUP", 381},
            {"ROW", 382},
            {"ROWS", 383},
            {"RULE", 384},
            {"SELECT", 385},
            {"SEMI", 386},
            {"SEPARATOR", 387},
            {"SEQUENCE", 388},
            {"SERDE_PROPERTIES", 389},
            {"SET", 390},
            {"SETTINGS", 391},
            {"SHOW", 392},
            {"SIMILAR_TO", 393},
            {"SOME", 394},
            {"SORT_BY", 395},
            {"SOUNDS_LIKE", 396},
            {"SQL_SECURITY", 397},
            {"START_WITH", 398},
            {"STORAGE_INTEGRATION", 399},
            {"STRAIGHT_JOIN", 400},
            {"STRUCT", 401},
            {"SUMMARIZE", 402},
            {"TABLE_SAMPLE", 403},
            {"TAG", 404},
            {"TEMPORARY", 405},
            {"TOP", 406},
            {"THEN", 407},
            {"TRUE", 408},
            {"TRUNCATE", 409},
            {"TRIGGER", 410},
            {"TYPE", 411},
            {"UNCACHE", 412},
            {"UNION", 413},
            {"UNNEST", 414},
            {"UNPIVOT", 415},
            {"UPDATE", 416},
            {"USE", 417},
            {"USING", 418},
            {"VALUES", 419},
            {"VARIADIC", 420},
            {"VIEW", 421},
            {"SEMANTIC_VIEW", 422},
            {"VOLATILE", 423},
            {"VOLUME", 424},
            {"WHEN", 425},
            {"WHERE", 426},
            {"WINDOW", 427},
            {"WITH", 428},
            {"UNIQUE", 429},
            {"UTC_DATE", 430},
            {"UTC_TIME", 431},
            {"UTC_TIMESTAMP", 432},
            {"VERSION_SNAPSHOT", 433},
            {"TIMESTAMP_SNAPSHOT", 434},
            {"OPTION", 435},
            {"SINK", 436},
            {"SOURCE", 437},
            {"ANALYZE", 438},
            {"NAMESPACE", 439},
            {"EXPORT", 440},
            {"HIVE_TOKEN_STREAM", 441},
            {"SENTINEL", 442},
        };

        private static int GetTokenTypeId(string name)
        {
            return TokenTypeMap.TryGetValue(name, out int value) ? value : 88;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PolyglotResult
        {
            public IntPtr Data;
            public IntPtr Error;
            public int Status;
        }

        private delegate PolyglotResult polyglot_tokenize_delegate(IntPtr sql, IntPtr dialect);
        private delegate PolyglotResult polyglot_transpile_delegate(IntPtr sql, IntPtr fromDialect, IntPtr toDialect);
        private delegate PolyglotResult polyglot_transpile_with_options_delegate(IntPtr sql, IntPtr fromDialect, IntPtr toDialect, IntPtr optionsJson);
        private delegate PolyglotResult polyglot_parse_delegate(IntPtr sql, IntPtr dialect);
        private delegate PolyglotResult polyglot_parse_one_delegate(IntPtr sql, IntPtr dialect);
        private delegate PolyglotResult polyglot_diff_delegate(IntPtr sql1, IntPtr sql2, IntPtr dialect);
        private delegate void polyglot_free_result_delegate(PolyglotResult result);
        private delegate void polyglot_free_string_delegate(IntPtr s);

        private static polyglot_tokenize_delegate _tokenize;
        private static polyglot_transpile_delegate _transpile;
        private static polyglot_transpile_with_options_delegate _transpileWithOptions;
        private static polyglot_parse_delegate _parse;
        private static polyglot_parse_one_delegate _parseOne;
        private static polyglot_diff_delegate _diff;
        private static polyglot_free_result_delegate _freeResult;
        private static polyglot_free_string_delegate _freeString;

        static NativeExports()
        {
            IntPtr lib = dlopen(GetRustLibraryPath(), RTLD_NOW);
            if (lib == IntPtr.Zero)
            {
                throw new DllNotFoundException("Cannot load " + GetRustLibraryPath());
            }
            _tokenize = Marshal.GetDelegateForFunctionPointer<polyglot_tokenize_delegate>(dlsym(lib, "polyglot_tokenize"));
            _transpile = Marshal.GetDelegateForFunctionPointer<polyglot_transpile_delegate>(dlsym(lib, "polyglot_transpile"));
            _transpileWithOptions = Marshal.GetDelegateForFunctionPointer<polyglot_transpile_with_options_delegate>(dlsym(lib, "polyglot_transpile_with_options"));
            _parse = Marshal.GetDelegateForFunctionPointer<polyglot_parse_delegate>(dlsym(lib, "polyglot_parse"));
            _parseOne = Marshal.GetDelegateForFunctionPointer<polyglot_parse_one_delegate>(dlsym(lib, "polyglot_parse_one"));
            _diff = Marshal.GetDelegateForFunctionPointer<polyglot_diff_delegate>(dlsym(lib, "polyglot_diff"));
            _freeResult = Marshal.GetDelegateForFunctionPointer<polyglot_free_result_delegate>(dlsym(lib, "polyglot_free_result"));
            _freeString = Marshal.GetDelegateForFunctionPointer<polyglot_free_string_delegate>(dlsym(lib, "polyglot_free_string"));
        }

        private static string GetRustLibraryPath()
        {
            Dl_info info;
            dladdr(typeof(NativeExports).TypeHandle.Value, out info);
            string binaryPath = info.dli_fname;
            string directory = Path.GetDirectoryName(binaryPath) ?? "";
            return Path.Combine(directory, "libpolyglot_sql_ffi.so");
        }

        private const int RTLD_NOW = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct Dl_info
        {
            public string dli_fname;
            public IntPtr dli_fbase;
            public string dli_sname;
            public IntPtr dli_saddr;
        }

        [DllImport("libc", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libc", EntryPoint = "dlsym")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libc", EntryPoint = "dladdr")]
        private static extern int dladdr(IntPtr addr, out Dl_info info);

        [UnmanagedCallersOnly(EntryPoint = "tokenize_sql")]
        public static IntPtr tokenize_sql(IntPtr sqlPtr)
        {
            try
            {
                string sql = Marshal.PtrToStringUTF8(sqlPtr) ?? "";
                string rustJson = RustTokenize(sql);
                string convertedJson = ConvertRustJsonToPythonFormat(rustJson);
                return MarshalStringToPtr(convertedJson);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "free_string")]
        public static void free_string(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }

        [UnmanagedCallersOnly(EntryPoint = "transpile_sql")]
        public static IntPtr transpile_sql(IntPtr sqlPtr, IntPtr fromDialectPtr, IntPtr toDialectPtr)
        {
            try
            {
                string sql = Marshal.PtrToStringUTF8(sqlPtr) ?? "";
                string fromDialect = Marshal.PtrToStringUTF8(fromDialectPtr) ?? "generic";
                string toDialect = Marshal.PtrToStringUTF8(toDialectPtr) ?? "generic";
                string result = RustTranspile(sql, fromDialect, toDialect);
                return MarshalStringToPtr(result);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "parse_sql")]
        public static IntPtr parse_sql(IntPtr sqlPtr, IntPtr dialectPtr)
        {
            try
            {
                string sql = Marshal.PtrToStringUTF8(sqlPtr) ?? "";
                string dialect = Marshal.PtrToStringUTF8(dialectPtr) ?? "generic";
                string result = RustParse(sql, dialect);
                return MarshalStringToPtr(result);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "diff_sql")]
        public static IntPtr diff_sql(IntPtr sql1Ptr, IntPtr sql2Ptr, IntPtr dialectPtr)
        {
            try
            {
                string sql1 = Marshal.PtrToStringUTF8(sql1Ptr) ?? "";
                string sql2 = Marshal.PtrToStringUTF8(sql2Ptr) ?? "";
                string dialect = Marshal.PtrToStringUTF8(dialectPtr) ?? "generic";
                string result = RustDiff(sql1, sql2, dialect);
                return MarshalStringToPtr(result);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        private static string RustTokenize(string sql)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi("generic");

            try
            {
                PolyglotResult result = _tokenize(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    if (result.Error != IntPtr.Zero)
                        _freeString(result.Error);
                    return "[]";
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                _freeResult(result);
                return json;
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private static string RustTranspile(string sql, string fromDialect, string toDialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr fromDialectPtr = Marshal.StringToHGlobalAnsi(fromDialect);
            IntPtr toDialectPtr = Marshal.StringToHGlobalAnsi(toDialect);

            try
            {
                PolyglotResult result = _transpile(sqlPtr, fromDialectPtr, toDialectPtr);

                if (result.Status != 0)
                {
                    if (result.Error != IntPtr.Zero)
                        _freeString(result.Error);
                    return "[]";
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                _freeResult(result);
                return json;
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(fromDialectPtr);
                Marshal.FreeHGlobal(toDialectPtr);
            }
        }

        private static string RustParse(string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);

            try
            {
                PolyglotResult result = _parse(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    if (result.Error != IntPtr.Zero)
                        _freeString(result.Error);
                    return "[]";
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                _freeResult(result);
                return json;
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private static string RustDiff(string sql1, string sql2, string dialect)
        {
            IntPtr sql1Ptr = Marshal.StringToHGlobalAnsi(sql1);
            IntPtr sql2Ptr = Marshal.StringToHGlobalAnsi(sql2);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);

            try
            {
                PolyglotResult result = _diff(sql1Ptr, sql2Ptr, dialectPtr);

                if (result.Status != 0)
                {
                    if (result.Error != IntPtr.Zero)
                        _freeString(result.Error);
                    return "[]";
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                _freeResult(result);
                return json;
            }
            finally
            {
                Marshal.FreeHGlobal(sql1Ptr);
                Marshal.FreeHGlobal(sql2Ptr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private static string ConvertRustJsonToPythonFormat(string rustJson)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            int i = 0;
            bool first = true;
            while (i < rustJson.Length)
            {
                int objStart = rustJson.IndexOf('{', i);
                if (objStart == -1) break;

                int objEnd = FindMatchingBrace(rustJson, objStart);
                if (objEnd == -1) break;

                string tokenObj = rustJson.Substring(objStart + 1, objEnd - objStart - 1);

                string tokenTypeStr = ExtractJsonStringField(tokenObj, "token_type");
                int tokenTypeId = GetTokenTypeId(tokenTypeStr);
                string text = ExtractJsonStringField(tokenObj, "text");
                string comments = ExtractJsonArrayField(tokenObj, "comments");

                string spanObj = ExtractJsonObjectField(tokenObj, "span");
                string start = "0";
                string end = "0";
                string line = "1";
                string col = "1";

                if (!string.IsNullOrEmpty(spanObj))
                {
                    int rustStart = int.Parse(ExtractJsonField(spanObj, "start"));
                    int rustEnd = int.Parse(ExtractJsonField(spanObj, "end"));
                    int rustLine = int.Parse(ExtractJsonField(spanObj, "line"));
                    int rustCol = int.Parse(ExtractJsonField(spanObj, "column"));

                    start = rustStart.ToString();
                    end = (rustEnd - 1).ToString();
                    line = rustLine.ToString();
                    col = (rustCol - 1).ToString();
                }

                if (!first) sb.Append(",");
                sb.Append("{");
                sb.Append("\"token_type\":");
                sb.Append(tokenTypeId);
                sb.Append(",\"text\":\"");
                sb.Append(EscapeJson(text));
                sb.Append("\",\"line\":");
                sb.Append(line);
                sb.Append(",\"col\":");
                sb.Append(col);
                sb.Append(",\"start\":");
                sb.Append(start);
                sb.Append(",\"end\":");
                sb.Append(end);

                if (!string.IsNullOrEmpty(comments) && comments != "[]")
                {
                    sb.Append(",\"comments\":");
                    sb.Append(comments);
                }

                sb.Append("}");

                first = false;
                i = objEnd + 1;
            }

            sb.Append("]");
            return sb.ToString();
        }

        private static int FindMatchingBrace(string s, int start)
        {
            int depth = 0;
            bool inString = false;
            for (int i = start; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"' && (i == 0 || s[i - 1] != '\\'))
                    inString = !inString;
                else if (!inString)
                {
                    if (c == '{') depth++;
                    else if (c == '}') depth--;
                }
                if (depth == 0) return i;
            }
            return -1;
        }

        private static string ExtractJsonField(string json, string field)
        {
            string search = "\"" + field + "\":";
            int idx = json.IndexOf(search);
            if (idx == -1) return "0";

            int start = idx + search.Length;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t'))
                start++;

            int end = start;
            while (end < json.Length && json[end] != ',' && json[end] != '}')
                end++;

            return json.Substring(start, end - start).Trim();
        }

        private static string ExtractJsonStringField(string json, string field)
        {
            string search = "\"" + field + "\":\"";
            int idx = json.IndexOf(search);
            if (idx == -1) return "";

            int start = idx + search.Length;
            int end = start;
            while (end < json.Length)
            {
                if (json[end] == '"' && (end == 0 || json[end - 1] != '\\'))
                    break;
                end++;
            }

            return json.Substring(start, end - start);
        }

        private static string ExtractJsonArrayField(string json, string field)
        {
            string search = "\"" + field + "\":";
            int idx = json.IndexOf(search);
            if (idx == -1) return "[]";

            int start = idx + search.Length;
            while (start < json.Length && json[start] == ' ')
                start++;

            if (start >= json.Length || json[start] != '[')
                return "[]";

            int end = start + 1;
            int depth = 1;
            bool inString = false;
            while (end < json.Length && depth > 0)
            {
                char c = json[end];
                if (c == '"' && (end == 0 || json[end - 1] != '\\'))
                    inString = !inString;
                else if (!inString)
                {
                    if (c == '[') depth++;
                    else if (c == ']') depth--;
                }
                end++;
            }

            return json.Substring(start, end - start);
        }

        private static string ExtractJsonObjectField(string json, string field)
        {
            string search = "\"" + field + "\":{";
            int idx = json.IndexOf(search);
            if (idx == -1) return "";

            int start = idx + search.Length - 1;
            int end = FindMatchingBrace(json, start);
            if (end == -1) return "";

            return json.Substring(start, end - start + 1);
        }

        private static string EscapeJson(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private static IntPtr MarshalStringToPtr(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str + '\0');
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return ptr;
        }
    }
}
