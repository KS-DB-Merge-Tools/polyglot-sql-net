using System.Collections.Generic;

using PolyglotSql.Models;

namespace PolyglotSql
{
    public interface INativePolyglot
    {
        Token[] Tokenize(string sql, Dialect dialect = Dialect.Generic);
        string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect);
        string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, TranspileOptions options);
        string[] Format(string sql, Dialect dialect);
        string[] FormatWithOptions(string sql, Dialect dialect, FormatGuardOptions options);
        string[] Optimize(string sql, Dialect dialect);
        Expression[] Parse(string sql, Dialect dialect = Dialect.Generic);
        Expression ParseOne(string sql, Dialect dialect = Dialect.Generic);
        DiffResult Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic);
        DataType ParseDataType(string sql, Dialect dialect = Dialect.Generic);
        string GenerateDataType(DataType dataType, Dialect dialect = Dialect.Generic);
        LineageNode Lineage(string columnName, string sql, Dialect dialect = Dialect.Generic);
        LineageNode LineageWithSchema(string columnName, string sql, ValidationSchema schema, Dialect dialect = Dialect.Generic);
        string[] SourceTables(string columnName, string sql, Dialect dialect = Dialect.Generic);
        Expression[] QualifyTables(Expression[] ast, QualifyTablesOptions options = null);
        Expression[] RenameTablesWithOptions(Expression[] ast, Dictionary<string, string> mapping, RenameTablesOptions options = null);
        OpenLineageColumnLineageResult OpenLineageColumnLineage(string sql, OpenLineageOptions options = null);
        OpenLineageEventResult OpenLineageJobEvent(string sql, OpenLineageOptions options = null);
        OpenLineageEventResult OpenLineageRunEvent(string sql, OpenLineageOptions options = null);
        Expression[] AnnotateTypes(string sql, Dialect dialect = Dialect.Generic, ValidationSchema schema = null);
        string[] Generate(Expression[] ast, Dialect dialect = Dialect.Generic);
        QueryAnalysis AnalyzeQuery(string sql, AnalyzeQueryOptions options = null);
        ValidationResult Validate(string sql, Dialect dialect = Dialect.Generic);
        string[] DialectList();
        int DialectCount();
        string Version();
    }
}
