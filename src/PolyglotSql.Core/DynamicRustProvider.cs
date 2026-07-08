using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

using PolyglotSql.Models;

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
        private delegate PolyglotResult PolyglotParseDataTypeDelegate(IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotGenerateDataTypeDelegate(IntPtr dataTypeJson, IntPtr dialect);

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
        private PolyglotParseDataTypeDelegate _parseDataType;
        private PolyglotGenerateDataTypeDelegate _generateDataType;
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
            _parseDataType = LoadDelegate<PolyglotParseDataTypeDelegate>("polyglot_parse_data_type");
            _generateDataType = LoadDelegate<PolyglotGenerateDataTypeDelegate>("polyglot_generate_data_type");
            _freeString = LoadDelegate<PolyglotFreeStringDelegate>("polyglot_free_string");
            _freeResult = LoadDelegate<PolyglotFreeResultDelegate>("polyglot_free_result");
        }

        private T LoadDelegate<T>(string name) where T : Delegate
        {
            IntPtr ptr = NativeLibLoader.GetSymbol(_libHandle, name);
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public Token[] Tokenize(string sql, Dialect dialect = Dialect.Generic)
            => CallNativeTokenize(_tokenize, sql, dialect.ToString().ToLowerInvariant());

        public string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect)
            => CallNativeArray(_transpile, sql, fromDialect.ToString().ToLowerInvariant(), toDialect.ToString().ToLowerInvariant());

        public string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, string optionsJson)
            => CallNativeArray(_transpileWithOptions, sql, fromDialect.ToString().ToLowerInvariant(), toDialect.ToString().ToLowerInvariant(), optionsJson);

        public Expression[] Parse(string sql, Dialect dialect = Dialect.Generic)
            => CallNativeParse(_parse, sql, dialect.ToString().ToLowerInvariant());

        public Expression ParseOne(string sql, Dialect dialect = Dialect.Generic)
            => CallNativeParseOne(_parseOne, sql, dialect.ToString().ToLowerInvariant());

        public DiffResult Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic)
        {
            string json = CallNativeThreeStrings(_diff, sql1, sql2, dialect.ToString().ToLowerInvariant());
            var result = DiffParser.Parse(json);
            return result;
        }

        public DataType ParseDataType(string sql, Dialect dialect = Dialect.Generic)
        {
            string json = CallNative(_parseDataType, sql, dialect.ToString().ToLowerInvariant());
            return JsonSerializer.Deserialize<DataType>(json)!;
        }

        public string GenerateDataType(DataType dataType, Dialect dialect = Dialect.Generic)
        {
            string json = JsonSerializer.Serialize(dataType);
            return CallNative(_generateDataType, json, dialect.ToString().ToLowerInvariant());
        }

        private Token[] CallNativeTokenize(PolyglotTokenizeDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResultTokenize(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private Expression[] CallNativeParse(PolyglotParseDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResultParseArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private Expression CallNativeParseOne(PolyglotParseOneDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResultParseOne(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string CallNative(PolyglotParseDataTypeDelegate del, string sql, string dialect)
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

        private string CallNative(PolyglotGenerateDataTypeDelegate del, string dataTypeJson, string dialect)
        {
            IntPtr jsonPtr = Marshal.StringToHGlobalAnsi(dataTypeJson);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(jsonPtr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(jsonPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string CallNativeThreeStrings(PolyglotDiffDelegate del, string sql1, string sql2, string dialect)
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

        private Token[] HandleResultTokenize(PolyglotResult result)
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

            try
            {
                return JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.TokenArray) ?? Array.Empty<Token>();
            }
            catch (JsonException ex)
            {
                throw new PolyglotException(-1, $"Failed to deserialize tokens: {ex.Message}");
            }
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
            return JsonSerializer.Deserialize<string[]>(json)!;
        }

        private Expression[] HandleResultParseArray(PolyglotResult result)
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
            
            var doc = JsonDocument.Parse(json);
            var expressions = new Expression[doc.RootElement.GetArrayLength()];
            int i = 0;
            foreach (var elem in doc.RootElement.EnumerateArray())
            {
                expressions[i++] = new Expression(elem.Clone());
            }
            return expressions;
        }

        private Expression HandleResultParseOne(PolyglotResult result)
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
                ? Marshal.PtrToStringAnsi(result.Data) ?? "{}"
                : "{}";

            _freeResult(result);
            
            var doc = JsonDocument.Parse(json);
            return new Expression(doc.RootElement.Clone());
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
