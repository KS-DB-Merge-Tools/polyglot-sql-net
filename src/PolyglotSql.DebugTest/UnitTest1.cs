using System;
using System.IO;
using System.Runtime.InteropServices;
using PolyglotSql;
using PolyglotSql.Bundle;

namespace SqlGlotDotNet.DebugTest;

public class TokenizerTests
{
    static TokenizerTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestNativeLibraryLoad()
    {
        Console.WriteLine("Testing native library load...");
        try
        {
            string json = Polyglot.Tokenize("SELECT 1");
            Console.WriteLine($"SUCCESS: Got JSON response: {json.Substring(0, Math.Min(100, json.Length))}...");
            Assert.False(string.IsNullOrEmpty(json), "JSON should not be empty");
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"DLL NOT FOUND: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTokenRepr()
    {
        var tokens = new Tokenizer().Tokenize("foo");

        Console.WriteLine($"Token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Line}, col={t.Col}, start={t.Start}, end={t.End}");
        }

        Assert.Single(tokens);
        Assert.Equal(TokenType.VAR, tokens[0].TokenType);
        Assert.Equal(0, tokens[0].Line);  // 0-based line
        Assert.Equal(3, tokens[0].Col);   // 0-based col (position after last char)
        Assert.Equal(0, tokens[0].Start);
        Assert.Equal(2, tokens[0].End);   // 0-based end (last char position)
    }

    [Fact]
    public void TestSimpleWord()
    {
        var tokens = new Tokenizer().Tokenize("foo");

        Assert.Single(tokens);
        Assert.Equal(TokenType.VAR, tokens[0].TokenType);
        Assert.Equal(3, tokens[0].Col);  // 0-based: position after last char
    }

    [Fact]
    public void TestNumber()
    {
        var tokens = new Tokenizer().Tokenize("123");

        Assert.Single(tokens);
        Assert.Equal(TokenType.NUMBER, tokens[0].TokenType);
        Assert.Equal(3, tokens[0].Col);  // 0-based: position after last char
    }

    [Fact]
    public void TestSelectKeyword()
    {
        var tokens = new Tokenizer().Tokenize("SELECT");

        Assert.Single(tokens);
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(6, tokens[0].Col);  // 0-based: position after last char
    }

    [Fact]
    public void TestSelectOne()
    {
        var tokens = new Tokenizer().Tokenize("SELECT 1");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(TokenType.NUMBER, tokens[1].TokenType);
        Assert.Equal(6, tokens[0].Col);  // 0-based: position after last char
        Assert.Equal(8, tokens[1].Col);  // 0-based: position after last char
    }

    [Fact]
    public void TestCRLF()
    {
        var tokens = new Tokenizer().Tokenize("SELECT\r\n1");

        Console.WriteLine($"CRLF test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Line}, col={t.Col}, start={t.Start}, end={t.End}");
        }

        Assert.True(tokens.Count >= 2, $"Expected at least 2 tokens, got {tokens.Count}");
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(TokenType.NUMBER, tokens[1].TokenType);
    }

    [Fact]
    public void TestMultiline()
    {
        var tokens = new Tokenizer().Tokenize("SELECT\n1\n+2");

        Console.WriteLine($"Multiline test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Line}, col={t.Col}, start={t.Start}, end={t.End}");
        }

        Assert.True(tokens.Count >= 3, $"Expected at least 3 tokens, got {tokens.Count}");
        Assert.Equal(0, tokens[0].Line);  // 0-based line
    }

    [Fact]
    public void TestComments()
    {
        var tokens = new Tokenizer().Tokenize("SELECT -- comment\n1");

        Console.WriteLine($"Comment test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Line}, col={t.Col}, comments={t.Comments?.Count ?? 0}");
        }

        Assert.True(tokens.Count >= 2, $"Expected at least 2 tokens, got {tokens.Count}");
    }

    [Fact]
    public void TestSpaceKeywords()
    {
        var tokens = new Tokenizer().Tokenize("CHARACTER VARYING");

        Console.WriteLine($"Space keyword test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
        }
    }

    [Fact]
    public void TestJinja()
    {
        var tokens = new Tokenizer().Tokenize("{{ foo }}");

        Console.WriteLine($"Jinja test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
        }
    }

    [Fact]
    public void TestCommand()
    {
        Console.WriteLine("Command test - checking what happens with backslash command");
        try
        {
            var tokens = new Tokenizer().Tokenize("\\foo");
            Console.WriteLine($"Command test - token count: {tokens.Count}");
            foreach (var t in tokens)
            {
                Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Expected error for command syntax: {ex.Message}");
        }
    }

    [Fact]
    public void TestUnicodeIdentifiers()
    {
        var tokens = new Tokenizer().Tokenize("SELECT 日本語");

        Console.WriteLine($"Unicode test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
        }

        Assert.True(tokens.Count >= 2, $"Expected at least 2 tokens, got {tokens.Count}");
    }

    [Fact]
    public void TestComplexQuery()
    {
        var sql = "SELECT a, b FROM t WHERE c = 1 AND d > 2";
        var tokens = new Tokenizer().Tokenize(sql);

        Console.WriteLine($"Complex query test - token count: {tokens.Count}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Line}, col={t.Col}");
        }

        Assert.True(tokens.Count > 5, $"Expected more than 5 tokens, got {tokens.Count}");
    }

    [Fact]
    public void TestErrorMessage()
    {
        Console.WriteLine("Error handling test");
        try
        {
            var tokens = new Tokenizer().Tokenize("SELECT <<--\n1");
            Console.WriteLine($"  Token count: {tokens.Count}");
            foreach (var t in tokens)
            {
                Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Exception: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [Fact]
    public void TestRawJsonOutput()
    {
        string json = Polyglot.Tokenize("SELECT 1");
        Console.WriteLine($"Raw JSON: {json}");
        Assert.False(string.IsNullOrEmpty(json), "JSON output should not be empty");
    }

    [Fact]
    public void TestRawJsonError()
    {
            string json = Polyglot.Tokenize("SELECT <<--\n1");
        Console.WriteLine($"Raw JSON (error): {json}");
        Assert.False(string.IsNullOrEmpty(json), "JSON output should not be empty");
    }
}

public class TranspileTests
{
    static TranspileTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestTranspileBasic()
    {
        Console.WriteLine("=== TestTranspileBasic ===");
        try
        {
            string result = Polyglot.Transpile("SELECT 1", Dialect.Generic, Dialect.Generic);
            Console.WriteLine($"Result: {result}");
            Assert.False(string.IsNullOrEmpty(result), "Result should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileSelect()
    {
        Console.WriteLine("=== TestTranspileSelect ===");
        try
        {
            string result = Polyglot.Transpile("SELECT a, b FROM t", Dialect.Generic, Dialect.MySQL);
            Console.WriteLine($"Result: {result}");
            Assert.False(string.IsNullOrEmpty(result), "Result should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileWithOptions()
    {
        Console.WriteLine("=== TestTranspileWithOptions ===");
        try
        {
            string optionsJson = "{\"pretty\": true}";
            string result = Polyglot.TranspileWithOptions("SELECT 1", Dialect.Generic, Dialect.Generic, optionsJson);
            Console.WriteLine($"Result: {result}");
            Assert.False(string.IsNullOrEmpty(result), "Result should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileDialectConversion()
    {
        Console.WriteLine("=== TestTranspileDialectConversion ===");
        try
        {
            string sql = "SELECT * FROM users WHERE id = 1";
            string result = Polyglot.Transpile(sql, Dialect.MySQL, Dialect.PostgreSQL);
            Console.WriteLine($"Result: {result}");
            Assert.False(string.IsNullOrEmpty(result), "Result should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileComplexQuery()
    {
        Console.WriteLine("=== TestTranspileComplexQuery ===");
        try
        {
            string sql = "SELECT COUNT(*) FROM users GROUP BY status ORDER BY count DESC LIMIT 10";
            string result = Polyglot.Transpile(sql, Dialect.Generic, Dialect.BigQuery);
            Console.WriteLine($"Result: {result}");
            Assert.False(string.IsNullOrEmpty(result), "Result should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileError()
    {
        Console.WriteLine("=== TestTranspileError ===");
        try
        {
            string result = Polyglot.Transpile("SELECT <<--", Dialect.Generic, Dialect.Generic);
            Console.WriteLine($"Result (error case): {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Expected error: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

public class AstTests
{
    static AstTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseBasic()
    {
        Console.WriteLine("=== TestParseBasic ===");
        try
        {
            string json = Polyglot.Parse("SELECT 1");
            Console.WriteLine($"AST JSON length: {json.Length}");
            Console.WriteLine($"AST JSON preview: {json.Substring(0, Math.Min(200, json.Length))}...");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.False(string.IsNullOrEmpty(json), "JSON should not be empty");
            Assert.True(nodes.Count > 0, "Should have at least one AST node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseSelectWithColumns()
    {
        Console.WriteLine("=== TestParseSelectWithColumns ===");
        try
        {
            string sql = "SELECT a, b, c FROM users WHERE id = 1";
            string json = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"AST JSON length: {json.Length}");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.True(nodes.Count > 0, "Should have at least one AST node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseOne()
    {
        Console.WriteLine("=== TestParseOne ===");
        try
        {
            string json = Polyglot.ParseOne("SELECT 1");
            Console.WriteLine($"ParseOne JSON: {json.Substring(0, Math.Min(200, json.Length))}...");
            Assert.False(string.IsNullOrEmpty(json), "JSON should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseComplexQuery()
    {
        Console.WriteLine("=== TestParseComplexQuery ===");
        try
        {
            string sql = "SELECT COUNT(*) FROM users GROUP BY status ORDER BY count DESC LIMIT 10";
            string json = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"AST JSON length: {json.Length}");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.True(nodes.Count > 0, "Should have at least one AST node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseMultipleStatements()
    {
        Console.WriteLine("=== TestParseMultipleStatements ===");
        try
        {
            string sql = "SELECT 1; SELECT 2";
            string json = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"AST JSON length: {json.Length}");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.True(nodes.Count >= 2, "Should have at least 2 AST nodes for 2 statements");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}

public class DiffTests
{
    static DiffTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestDiffIdentical()
    {
        Console.WriteLine("=== TestDiffIdentical ===");
        try
        {
            string sql = "SELECT 1";
            string json = Polyglot.Diff(sql, sql);
            Console.WriteLine($"Diff JSON: {json}");

            var result = DiffParser.Parse(json);
            Console.WriteLine($"Diff result: {result}");

            Assert.True(result.AreEqual, "Identical SQL should have no differences");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestDiffDifferentColumns()
    {
        Console.WriteLine("=== TestDiffDifferentColumns ===");
        try
        {
            string sql1 = "SELECT a FROM t";
            string sql2 = "SELECT b FROM t";
            string json = Polyglot.Diff(sql1, sql2);
            Console.WriteLine($"SQL1: {sql1}");
            Console.WriteLine($"SQL2: {sql2}");
            Console.WriteLine($"Diff JSON: {json}");

            var result = DiffParser.Parse(json);
            Console.WriteLine($"Diff result: {result}");
            foreach (var edit in result.Edits)
            {
                Console.WriteLine($"  Edit: {edit}");
            }

            Assert.False(result.AreEqual, "Different SQL should have differences");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestDiffAddedWhere()
    {
        Console.WriteLine("=== TestDiffAddedWhere ===");
        try
        {
            string sql1 = "SELECT * FROM users";
            string sql2 = "SELECT * FROM users WHERE id = 1";
            string json = Polyglot.Diff(sql1, sql2);
            Console.WriteLine($"SQL1: {sql1}");
            Console.WriteLine($"SQL2: {sql2}");
            Console.WriteLine($"Diff JSON: {json}");

            var result = DiffParser.Parse(json);
            Console.WriteLine($"Diff result: {result}");
            foreach (var edit in result.Edits)
            {
                Console.WriteLine($"  Edit: {edit}");
            }

            Assert.True(result.InsertCount > 0 || result.UpdateCount > 0, "Should have inserts or updates for added WHERE clause");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestDiffComplexQueries()
    {
        Console.WriteLine("=== TestDiffComplexQueries ===");
        try
        {
            string sql1 = "SELECT a, b FROM t WHERE c = 1";
            string sql2 = "SELECT a, b, c FROM t WHERE c = 1 AND d = 2";
            string json = Polyglot.Diff(sql1, sql2);
            Console.WriteLine($"SQL1: {sql1}");
            Console.WriteLine($"SQL2: {sql2}");
            Console.WriteLine($"Diff JSON: {json}");

            var result = DiffParser.Parse(json);
            Console.WriteLine($"Diff result: {result}");
            foreach (var edit in result.Edits)
            {
                Console.WriteLine($"  Edit: {edit}");
            }

            Assert.False(result.AreEqual, "Different SQL should have differences");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestDiffWithDialect()
    {
        Console.WriteLine("=== TestDiffWithDialect ===");
        try
        {
            string sql1 = "SELECT * FROM users LIMIT 10";
            string sql2 = "SELECT * FROM users LIMIT 20";
            string json = Polyglot.Diff(sql1, sql2, Dialect.MySQL);
            Console.WriteLine($"SQL1: {sql1}");
            Console.WriteLine($"SQL2: {sql2}");
            Console.WriteLine($"Dialect: mysql");
            Console.WriteLine($"Diff JSON: {json}");

            var result = DiffParser.Parse(json);
            Console.WriteLine($"Diff result: {result}");
            foreach (var edit in result.Edits)
            {
                Console.WriteLine($"  Edit: {edit}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
