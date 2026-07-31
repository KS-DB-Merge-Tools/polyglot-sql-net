using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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

        [StructLayout(LayoutKind.Sequential)]
        private struct PolyglotValidationResult
        {
            public int Valid;
            public IntPtr ErrorsJson;
            public IntPtr Error;
            public int Status;
        }

        private const int STATUS_VALIDATION_ERROR = 4;

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
        private delegate PolyglotResult PolyglotOpenLineageColumnLineageDelegate(IntPtr sql, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotOpenLineageJobEventDelegate(IntPtr sql, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotOpenLineageRunEventDelegate(IntPtr sql, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotAnnotateTypesDelegate(IntPtr sql, IntPtr dialect, IntPtr schemaJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotGenerateDelegate(IntPtr astJson, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotResult PolyglotAnalyzeQueryDelegate(IntPtr sql, IntPtr optionsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PolyglotValidationResult PolyglotValidateDelegate(IntPtr sql, IntPtr dialect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr PolyglotDialectListDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PolyglotDialectCountDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr PolyglotVersionDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeStringDelegate(IntPtr s);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeResultDelegate(PolyglotResult result);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PolyglotFreeValidationResultDelegate(PolyglotValidationResult result);

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
        private PolyglotOpenLineageColumnLineageDelegate _openLineageColumnLineage;
        private PolyglotOpenLineageJobEventDelegate _openLineageJobEvent;
        private PolyglotOpenLineageRunEventDelegate _openLineageRunEvent;
        private PolyglotAnnotateTypesDelegate _annotateTypes;
        private PolyglotGenerateDelegate _generate;
        private PolyglotAnalyzeQueryDelegate _analyzeQuery;
        private PolyglotValidateDelegate _validate;
        private PolyglotDialectListDelegate _dialectList;
        private PolyglotDialectCountDelegate _dialectCount;
        private PolyglotVersionDelegate _version;
        private PolyglotFreeStringDelegate _freeString;
        private PolyglotFreeResultDelegate _freeResult;
        private PolyglotFreeValidationResultDelegate _freeValidationResult;

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
            _openLineageColumnLineage = LoadDelegate<PolyglotOpenLineageColumnLineageDelegate>("polyglot_openlineage_column_lineage");
            _openLineageJobEvent = LoadDelegate<PolyglotOpenLineageJobEventDelegate>("polyglot_openlineage_job_event");
            _openLineageRunEvent = LoadDelegate<PolyglotOpenLineageRunEventDelegate>("polyglot_openlineage_run_event");
            _annotateTypes = LoadDelegate<PolyglotAnnotateTypesDelegate>("polyglot_annotate_types");
            _generate = LoadDelegate<PolyglotGenerateDelegate>("polyglot_generate");
            _analyzeQuery = LoadDelegate<PolyglotAnalyzeQueryDelegate>("polyglot_analyze_query");
            _validate = LoadDelegate<PolyglotValidateDelegate>("polyglot_validate");
            _dialectList = LoadDelegate<PolyglotDialectListDelegate>("polyglot_dialect_list");
            _dialectCount = LoadDelegate<PolyglotDialectCountDelegate>("polyglot_dialect_count");
            _version = LoadDelegate<PolyglotVersionDelegate>("polyglot_version");
            _freeString = LoadDelegate<PolyglotFreeStringDelegate>("polyglot_free_string");
            _freeResult = LoadDelegate<PolyglotFreeResultDelegate>("polyglot_free_result");
            _freeValidationResult = LoadDelegate<PolyglotFreeValidationResultDelegate>("polyglot_free_validation_result");
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
            string optionsJson = JsonSerializer.Serialize(options, PolyglotJsonContext.Default.TranspileOptions);
            return CallNativeArray(_transpileWithOptions, sql, fromDialect.ToString().ToLowerInvariant(), toDialect.ToString().ToLowerInvariant(), optionsJson);
        }

        public string[] Format(string sql, Dialect dialect)
            => CallNativeArray(_format, sql, dialect.ToString().ToLowerInvariant());

        public string[] FormatWithOptions(string sql, Dialect dialect, FormatGuardOptions options)
        {
            string optionsJson = JsonSerializer.Serialize(options, PolyglotJsonContext.Default.FormatGuardOptions);
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
            return JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.DataType);
        }

        public string GenerateDataType(DataType dataType, Dialect dialect = Dialect.Generic)
        {
            string json = JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType);
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
            string astJson = JsonSerializer.Serialize(ast, PolyglotJsonContext.Default.ExpressionArray);
            string optionsJson = JsonSerializer.Serialize(options ?? new QualifyTablesOptions(), PolyglotJsonContext.Default.QualifyTablesOptions);
            return CallNativeQualifyTables(_qualifyTables, astJson, optionsJson);
        }

        public Expression[] RenameTablesWithOptions(Expression[] ast, Dictionary<string, string> mapping, RenameTablesOptions options = null)
        {
            string astJson = JsonSerializer.Serialize(ast, PolyglotJsonContext.Default.ExpressionArray);
            string mappingJson = JsonSerializer.Serialize(mapping ?? new Dictionary<string, string>(), PolyglotJsonContext.Default.DictionaryStringString);
            string optionsJson = JsonSerializer.Serialize(options ?? new RenameTablesOptions(), PolyglotJsonContext.Default.RenameTablesOptions);
            return CallNativeRenameTables(_renameTables, astJson, mappingJson, optionsJson);
        }

        public OpenLineageColumnLineageResult OpenLineageColumnLineage(string sql, OpenLineageOptions options = null)
        {
            string optionsJson = JsonSerializer.Serialize(options ?? new OpenLineageOptions(), PolyglotJsonContext.Default.OpenLineageOptions);
            return CallNativeOpenLineage(_openLineageColumnLineage, sql, optionsJson, PolyglotJsonContext.Default.OpenLineageColumnLineageResult);
        }

        public OpenLineageEventResult OpenLineageJobEvent(string sql, OpenLineageOptions options = null)
        {
            string optionsJson = JsonSerializer.Serialize(options ?? new OpenLineageOptions(), PolyglotJsonContext.Default.OpenLineageOptions);
            return CallNativeOpenLineage(_openLineageJobEvent, sql, optionsJson, PolyglotJsonContext.Default.OpenLineageEventResult);
        }

        public OpenLineageEventResult OpenLineageRunEvent(string sql, OpenLineageOptions options = null)
        {
            string optionsJson = JsonSerializer.Serialize(options ?? new OpenLineageOptions(), PolyglotJsonContext.Default.OpenLineageOptions);
            return CallNativeOpenLineage(_openLineageRunEvent, sql, optionsJson, PolyglotJsonContext.Default.OpenLineageEventResult);
        }

        public Expression[] AnnotateTypes(string sql, Dialect dialect = Dialect.Generic, ValidationSchema schema = null)
        {
            string schemaJson = schema == null
                ? string.Empty
                : JsonSerializer.Serialize(schema, PolyglotJsonContext.Default.ValidationSchema);
            return CallNativeAnnotateTypes(_annotateTypes, sql, dialect.ToString().ToLowerInvariant(), schemaJson);
        }

        public string[] Generate(Expression[] ast, Dialect dialect = Dialect.Generic)
        {
            string astJson = JsonSerializer.Serialize(ast, PolyglotJsonContext.Default.ExpressionArray);
            return CallNativeGenerate(_generate, astJson, dialect.ToString().ToLowerInvariant());
        }

        public QueryAnalysis AnalyzeQuery(string sql, AnalyzeQueryOptions options = null)
        {
            string optionsJson = JsonSerializer.Serialize(options ?? new AnalyzeQueryOptions(), PolyglotJsonContext.Default.AnalyzeQueryOptions);
            return CallNativeAnalyzeQuery(_analyzeQuery, sql, optionsJson, PolyglotJsonContext.Default.QueryAnalysis);
        }

        public ValidationResult Validate(string sql, Dialect dialect = Dialect.Generic)
        {
            return CallNativeValidate(_validate, sql, dialect.ToString().ToLowerInvariant());
        }

        public string[] DialectList()
        {
            IntPtr ptr = _dialectList();
            if (ptr == IntPtr.Zero)
                return Array.Empty<string>();

            try
            {
                string json = Marshal.PtrToStringAnsi(ptr) ?? "[]";
                return JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.StringArray) ?? Array.Empty<string>();
            }
            finally
            {
                _freeString(ptr);
            }
        }

        public int DialectCount()
        {
            return _dialectCount();
        }

        public string Version()
        {
            IntPtr ptr = _version();
            return ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
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

        private T CallNativeOpenLineage<T>(PolyglotOpenLineageColumnLineageDelegate del, string sql, string optionsJson, JsonTypeInfo<T> typeInfo)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(optionsJson);
            try
            {
                var result = del(sqlPtr, optPtr);
                return HandleResultOpenLineage(result, typeInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(optPtr);
            }
        }

        private T CallNativeOpenLineage<T>(PolyglotOpenLineageJobEventDelegate del, string sql, string optionsJson, JsonTypeInfo<T> typeInfo)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(optionsJson);
            try
            {
                var result = del(sqlPtr, optPtr);
                return HandleResultOpenLineage(result, typeInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(optPtr);
            }
        }

        private T CallNativeOpenLineage<T>(PolyglotOpenLineageRunEventDelegate del, string sql, string optionsJson, JsonTypeInfo<T> typeInfo)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(optionsJson);
            try
            {
                var result = del(sqlPtr, optPtr);
                return HandleResultOpenLineage(result, typeInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(optPtr);
            }
        }

        private T HandleResultOpenLineage<T>(PolyglotResult result, JsonTypeInfo<T> typeInfo)
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

            try
            {
                return JsonSerializer.Deserialize(json, typeInfo)
                    ?? throw new PolyglotException(-1, "OpenLineage result deserialization returned null");
            }
            catch (JsonException ex)
            {
                throw new PolyglotException(-1, $"Failed to deserialize OpenLineage result: {ex.Message}");
            }
        }

        private Expression[] CallNativeAnnotateTypes(PolyglotAnnotateTypesDelegate del, string sql, string dialect, string schemaJson)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            IntPtr schemaPtr = Marshal.StringToHGlobalAnsi(schemaJson);
            try
            {
                var result = del(sqlPtr, dialectPtr, schemaPtr);
                return HandleResultExpressionArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
                Marshal.FreeHGlobal(schemaPtr);
            }
        }

        private string[] CallNativeGenerate(PolyglotGenerateDelegate del, string astJson, string dialect)
        {
            IntPtr astPtr = Marshal.StringToHGlobalAnsi(astJson);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(astPtr, dialectPtr);
                return HandleResultArray(result);
            }
            finally
            {
                Marshal.FreeHGlobal(astPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private QueryAnalysis CallNativeAnalyzeQuery(PolyglotAnalyzeQueryDelegate del, string sql, string optionsJson, JsonTypeInfo<QueryAnalysis> typeInfo)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr optPtr = Marshal.StringToHGlobalAnsi(optionsJson);
            try
            {
                var result = del(sqlPtr, optPtr);
                return HandleResultOpenLineage(result, typeInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(optPtr);
            }
        }

        private ValidationResult CallNativeValidate(PolyglotValidateDelegate del, string sql, string dialect)
        {
            IntPtr sqlPtr = Marshal.StringToHGlobalAnsi(sql);
            IntPtr dialectPtr = Marshal.StringToHGlobalAnsi(dialect);
            try
            {
                var result = del(sqlPtr, dialectPtr);
                return HandleResultValidation(result);
            }
            finally
            {
                Marshal.FreeHGlobal(sqlPtr);
                Marshal.FreeHGlobal(dialectPtr);
            }
        }

        private ValidationResult HandleResultValidation(PolyglotValidationResult result)
        {
            // status 0 = valid, status 4 (STATUS_VALIDATION_ERROR) = invalid but parsed.
            // Both are normal validation outcomes carrying the errors in `errors_json`.
            // Any other status indicates a hard error (unknown dialect, panic, ...).
            if (result.Status != 0 && result.Status != STATUS_VALIDATION_ERROR)
            {
                string error = result.Error != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Error) ?? "Unknown error"
                    : "Unknown error";

                _freeValidationResult(result);

                throw new PolyglotException(result.Status, $"Polyglot error (status {result.Status}): {error}");
            }

            string errorsJson = result.ErrorsJson != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(result.ErrorsJson) ?? "[]"
                : "[]";

            bool valid = result.Valid != 0;

            _freeValidationResult(result);

            try
            {
                var errors = JsonSerializer.Deserialize(errorsJson, PolyglotJsonContext.Default.ValidationErrorArray)
                    ?? Array.Empty<ValidationError>();
                return new ValidationResult { Valid = valid, Errors = errors };
            }
            catch (JsonException ex)
            {
                throw new PolyglotException(-1, $"Failed to deserialize validation result: {ex.Message}");
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
            return JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.StringArray)!;
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