using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PolyglotSql
{
    /// <summary>
    /// Dynamic P/Invoke using LoadLibrary / dlopen
    /// </summary>
    public class DynamicRustProvider : INativePolyglot, IDisposable
    {
        private IntPtr _libHandle;
        private bool _disposed;

        [StructLayout(LayoutKind.Sequential)]
        private struct PolyglotResult
        {
            public IntPtr Data;
            public IntPtr Error;
            public int Status;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotTokenizeDelegate(IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotTranspileDelegate(IntPtr sql, IntPtr fromDialect, IntPtr toDialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotTranspileWithOptionsDelegate(IntPtr sql, IntPtr fromDialect, IntPtr toDialect, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotParseDelegate(IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotParseOneDelegate(IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotDiffDelegate(IntPtr sql1, IntPtr sql2, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeStringDelegate(IntPtr s);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeResultDelegate(PolyglotResult result);

        private PolyglotTokenizeDelegate _tokenize;
        private PolyglotTranspileDelegate _transpile;
        private PolyglotTranspileWithOptionsDelegate _transpileWithOptions;
        private PolyglotParseDelegate _parse;
        private PolyglotParseOneDelegate _parseOne;
        private PolyglotDiffDelegate _diff;
        private PolyglotFreeStringDelegate _freeString;
        private PolyglotFreeResultDelegate _freeResult;

        public DynamicRustProvider(string libPath)
        {
            _libHandle = NativeLibLoader.Load(libPath);

            _tokenize = LoadDelegate<PolyglotTokenizeDelegate>("polyglot_tokenize");
            _transpile = LoadDelegate<PolyglotTranspileDelegate>("polyglot_transpile");
            _transpileWithOptions = LoadDelegate<PolyglotTranspileWithOptionsDelegate>("polyglot_transpile_with_options");
            _parse = LoadDelegate<PolyglotParseDelegate>("polyglot_parse");
            _parseOne = LoadDelegate<PolyglotParseOneDelegate>("polyglot_parse_one");
            _diff = LoadDelegate<PolyglotDiffDelegate>("polyglot_diff");
            _freeString = LoadDelegate<PolyglotFreeStringDelegate>("polyglot_free_string");
            _freeResult = LoadDelegate<PolyglotFreeResultDelegate>("polyglot_free_result");
        }

        private T LoadDelegate<T>(string name) where T : Delegate
        {
            IntPtr ptr = NativeLibLoader.GetSymbol(_libHandle, name);
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public string Tokenize(string sql, Dialect dialect = Dialect.Generic)
            => CallNative(_tokenize, sql, dialect.ToString().ToLowerInvariant());

        public string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect)
            => CallNativeArray(_transpile, sql, fromDialect.ToString().ToLowerInvariant(), toDialect.ToString().ToLowerInvariant());

        public string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, string optionsJson)
            => CallNativeArray(_transpileWithOptions, sql, fromDialect.ToString().ToLowerInvariant(), toDialect.ToString().ToLowerInvariant(), optionsJson);

        public string Parse(string sql, Dialect dialect = Dialect.Generic)
            => CallNative(_parse, sql, dialect.ToString().ToLowerInvariant());

        public string ParseOne(string sql, Dialect dialect = Dialect.Generic)
            => CallNative(_parseOne, sql, dialect.ToString().ToLowerInvariant());

        public string Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic)
            => CallNative(_diff, sql1, sql2, dialect.ToString().ToLowerInvariant());

        private string CallNative(PolyglotTokenizeDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string CallNative(PolyglotParseDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string CallNative(PolyglotParseOneDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string CallNative(PolyglotDiffDelegate del, string sql1, string sql2, string dialect)
        {
            IntPtr sql1Ptr = Marshal.StringToHGlobalAnsi(sql1);
            IntPtr sql2Ptr = Marshal.StringToHGlobalAnsi(sql2);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sql1Ptr, sql2Ptr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sql1Ptr);
                Marshal.FreeHGlobal(sql2Ptr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private int FindMatchingBracket(string s, int start)
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
                    if (c == '[') depth++;
                    else if (c == ']') depth--;
                }
                if (depth == 0) return i;
            }
            return -1;
        }

        private string ParseJsonString(string json, ref int i)
        {
            i = json.IndexOf('"', i);
            if (i == -1) return string.Empty;
            i++;
            int start = i;
            var sb = new System.Text.StringBuilder();
            while (i < json.Length)
            {
                char c = json[i];
                if (c == '\\' && i + 1 < json.Length)
                {
                    sb.Append(json[i + 1]);
                    i += 2;
                }
                else if (c == '"')
                {
                    i++;
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }
            return sb.ToString();
        }

        private string[] ParseStringArray(string json)
        {
            if (string.IsNullOrEmpty(json))
                return System.Array.Empty<string>();

            int i = 0;
            i = json.IndexOf('[', i);
            if (i == -1) return System.Array.Empty<string>();
            int arrEnd = FindMatchingBracket(json, i);
            if (arrEnd == -1) return System.Array.Empty<string>();

            var list = new System.Collections.Generic.List<string>();
            i++;
            while (i < arrEnd)
            {
                if (json[i] == '"')
                {
                    string s = ParseJsonString(json, ref i);
                    list.Add(s);
                }
                else
                {
                    i++;
                }
            }
            return list.ToArray();
        }

        private string[] CallNativeArray(PolyglotTranspileDelegate del, string sql, string fromDialect, string toDialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr fromPtr = Marshal.StringToHGlobalAnsi(fromDialect);
            IntPtr toPtr = Marshal.StringToHGlobalAnsi(toDialect);
            try
            {
                var result = del(sqlPtr, fromPtr, toPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(fromPtr);
                Marshal.FreeHGlobal(toPtr);
            }
        }

        private string[] CallNativeArray(PolyglotTranspileWithOptionsDelegate del, string sql, string fromDialect, string toDialect, string options)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr fromPtr = Marshal.StringToHGlobalAnsi(fromDialect);
            IntPtr toPtr = Marshal.StringToHGlobalAnsi(toDialect);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(options);
            try
            {
                var result = del(sqlPtr, fromPtr, toPtr, optPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(fromPtr);
                Marshal.FreeHGlobal(toPtr);
                Marshal.FreeHGlobal(optPtr);
            }
        }

        private string HandleResult(PolyglotResult result)
        {
            if (result.Status != 0)
            {
                string error = result.Error != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Error) ?? "Unknown error"
                    : "Unknown error";

                if (result.Error != IntPtr.Zero)
                    _freeString(result.Error);

                throw new PolyglotException(result.Status, $"Polyglot error (status {result.Status}): {error}");
            }

            string json = result.Data != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(result.Data) ?? "[]"
                : "[]";

            _freeResult(result);
            return json;
        }

        private string[] HandleResultArray(PolyglotResult result)
        {
            if (result.Status != 0)
            {
                string error = result.Error != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Error) ?? "Unknown error"
                    : "Unknown error";

                if (result.Error != IntPtr.Zero)
                    _freeString(result.Error);

                throw new PolyglotException(result.Status, $"Polyglot error (status {result.Status}): {error}");
            }

            string json = result.Data != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(result.Data) ?? "[]"
                : "[]";

            _freeResult(result);
            return ParseStringArray(json);
        }

        public void Dispose()
        {
            if (!_disposed && _libHandle != IntPtr.Zero)
            {
                NativeLibLoader.Free(_libHandle);
                _libHandle = IntPtr.Zero;
                _disposed = true;
            }
        }
    }
}
