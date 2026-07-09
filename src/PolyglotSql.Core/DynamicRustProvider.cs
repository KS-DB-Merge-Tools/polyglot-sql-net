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
        private delegate PolyglotResult PolyglotFormatDelegate(IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotFormatWithOptionsDelegate(IntPtr sql, IntPtr dialect, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotOptimizeDelegate(IntPtr sql, IntPtr dialect);

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
        private delegate PolyglotResult PolyglotLineageDelegate(IntPtr columnName, IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotLineageWithSchemaDelegate(IntPtr columnName, IntPtr sql, IntPtr schemaJson, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotQualifyTablesDelegate(IntPtr astJson, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotRenameTablesDelegate(IntPtr astJson, IntPtr mappingJson, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeStringDelegate(IntPtr s);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeResultDelegate(PolyglotResult result);

        private PolyglotTokenizeDelegate _tokenize;
        private PolyglotTranspileDelegate _transpile;
        private PolyglotTranspileWithOptionsDelegate _transpileWithOptions;
        private PolyglotFormatDelegate _format;
        private PolyglotFormatWithOptionsDelegate _formatWithOptions;
        private PolyglotOptimizeDelegate _optimize;
        private PolyglotParseDelegate _parse;
        private PolyglotParseOneDelegate _parseOne;
        private PolyglotDiffDelegate _diff;
        private PolyglotParseDataTypeDelegate _parseDataType;
        private PolyglotGenerateDataTypeDelegate _generateDataType;
        private PolyglotLineageDelegate _lineage;
        private PolyglotLineageWithSchemaDelegate _lineageWithSchema;
        private PolyglotLineageDelegate _sourceTables;
        private PolyglotQualifyTablesDelegate _qualifyTables;
        private PolyglotRenameTablesDelegate _renameTables;
        private PolyglotFreeStringDelegate _freeString;
        private PolyglotFreeResultDelegate _freeResult;

        public DynamicRustProvider(string libPath)
        {
            _libHandle = NativeLibLoader.Load(libPath);

            _tokenize = LoadDelegate<PolyglotTokenizeDelegate>("polyglot_tokenize");
            _transpile = LoadDelegate<PolyglotTranspileDelegate>("polyglot_transpile");
            _transpileWithOptions = LoadDelegate<PolyglotTranspileWithOptionsDelegate>("polyglot_transpile_with_options");
            _format = LoadDelegate<PolyglotFormatDelegate>("polyglot_format");
            _formatWithOptions = LoadDelegate<PolyglotFormatWithOptionsDelegate>("polyglot_format_with_options");
            _optimize = LoadDelegate<PolyglotOptimizeDelegate>("polyglot_optimize");
            _parse = LoadDelegate<PolyglotParseDelegate>("polyglot_parse");
            _parseOne = LoadDelegate<PolyglotParseOneDelegate>("polyglot_parse_one");
            _diff = LoadDelegate<PolyglotDiffDelegate>("polyglot_diff");
            _parseDataType = LoadDelegate<PolyglotParseDataTypeDelegate>("polyglot_parse_data_type");
            _generateDataType = LoadDelegate<PolyglotGenerateDataTypeDelegate>("polyglot_generate_data_type");
            _lineage = LoadDelegate<PolyglotLineageDelegate>("polyglot_lineage");
            _lineageWithSchema = LoadDelegate<PolyglotLineageWithSchemaDelegate>("polyglot_lineage_with_schema");
            _sourceTables = LoadDelegate<PolyglotLineageDelegate>("polyglot_source_tables");
            _qualifyTables = LoadDelegate<PolyglotQualifyTablesDelegate>("polyglot_qualify_tables");
            _renameTables = LoadDelegate<PolyglotRenameTablesDelegate>("polyglot_rename_tables_with_options");
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

        public string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, TranspileOptions options)
        {
            string optionsJson = JsonSerializer.Serialize(options);
            return CallNativeArray(_transpileWithOptions, sql, fromDialect.ToString().ToLowerInvariant(), toDialect.ToString().ToLowerInvariant(), optionsJson);
        }

        public string[] Format(string sql, Dialect dialect)
            => CallNativeArray(_format, sql, dialect.ToString().ToLowerInvariant());

        public string[] FormatWithOptions(string sql, Dialect dialect, FormatGuardOptions options)
        {
            string optionsJson = JsonSerializer.Serialize(options);
            return CallNativeArray(_formatWithOptions, sql, dialect.ToString().ToLowerInvariant(), optionsJson);
        }

        public string[] Optimize(string sql, Dialect dialect)
            => CallNativeArray(_optimize, sql, dialect.ToString().ToLowerInvariant());

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

        public LineageNode Lineage(string columnName, string sql, Dialect dialect = Dialect.Generic)
        {
            string json = CallNativeLineage(_lineage, columnName, sql, dialect.ToString().ToLowerInvariant());
            return LineageParser.Parse(json);
        }

        public LineageNode LineageWithSchema(string columnName, string sql, ValidationSchema schema, Dialect dialect = Dialect.Generic)
        {
            string schemaJson = JsonSerializer.Serialize(schema, PolyglotJsonContext.Default.ValidationSchema);
            string json = CallNativeLineageWithSchema(_lineageWithSchema, columnName, sql, schemaJson, dialect.ToString().ToLowerInvariant());
            return LineageParser.Parse(json);
        }

        public string[] SourceTables(string columnName, string sql, Dialect dialect = Dialect.Generic)
        {
            return CallNativeSourceTables(_sourceTables, columnName, sql, dialect.ToString().ToLowerInvariant());
        }

        public Expression[] QualifyTables(Expression[] ast, QualifyTablesOptions options = null)
        {
            string astJson = JsonSerializer.Serialize(ast);
            string optionsJson = JsonSerializer.Serialize(options ?? new QualifyTablesOptions(), PolyglotJsonContext.Default.QualifyTablesOptions);
            return CallNativeQualifyTables(_qualifyTables, astJson, optionsJson);
        }

        public Expression[] RenameTablesWithOptions(Expression[] ast, Dictionary<string, string> mapping, RenameTablesOptions options = null)
        {
            string astJson = JsonSerializer.Serialize(ast);
            string mappingJson = JsonSerializer.Serialize(mapping ?? new Dictionary<string, string>(), PolyglotJsonContext.Default.DictionaryStringString);
            string optionsJson = JsonSerializer.Serialize(options ?? new RenameTablesOptions(), PolyglotJsonContext.Default.RenameTablesOptions);
            return CallNativeRenameTables(_renameTables, astJson, mappingJson, optionsJson);
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

        private string[] CallNativeArray(PolyglotFormatDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string[] CallNativeArray(PolyglotOptimizeDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string[] CallNativeArray(PolyglotFormatWithOptionsDelegate del, string sql, string dialect, string options)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(options);
            try
            {
                var result = del(sqlPtr, dialectPtr, optPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
                Marshal.FreeHGlobal(optPtr);
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

        private string CallNativeLineage(PolyglotLineageDelegate del, string columnName, string sql, string dialect)
        {
            IntPtr colPtr = Marshal.StringToHGlobalAnsi(columnName);
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(colPtr, sqlPtr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(colPtr);
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string CallNativeLineageWithSchema(PolyglotLineageWithSchemaDelegate del, string columnName, string sql, string schemaJson, string dialect)
        {
            IntPtr colPtr = Marshal.StringToHGlobalAnsi(columnName);
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr schemaPtr = Marshal.StringToHGlobalAnsi(schemaJson);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(colPtr, sqlPtr, schemaPtr, dialectPtr);
                return HandleResult(result);
            }
            finally
            {
                Marshal.FreeHGlobal(colPtr);
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(schemaPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private string[] CallNativeSourceTables(PolyglotLineageDelegate del, string columnName, string sql, string dialect)
        {
            IntPtr colPtr = Marshal.StringToHGlobalAnsi(columnName);
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(colPtr, sqlPtr, dialectPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(colPtr);
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private Expression[] CallNativeQualifyTables(PolyglotQualifyTablesDelegate del, string astJson, string optionsJson)
        {
            IntPtr astPtr = Marshal.StringToHGlobalAnsi(astJson);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(optionsJson);
            try
            {
                var result = del(astPtr, optPtr);
                return HandleResultExpressionArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(astPtr);
                Marshal.FreeHGlobal(optPtr);
            }
        }

        private Expression[] CallNativeRenameTables(PolyglotRenameTablesDelegate del, string astJson, string mappingJson, string optionsJson)
        {
            IntPtr astPtr = Marshal.StringToHGlobalAnsi(astJson);
            IntPtr mappingPtr = Marshal.StringToHGlobalAnsi(mappingJson);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(optionsJson);
            try
            {
                var result = del(astPtr, mappingPtr, optPtr);
                return HandleResultExpressionArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(astPtr);
                Marshal.FreeHGlobal(mappingPtr);
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

        private Expression[] HandleResultExpressionArray(PolyglotResult result)
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