using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace SqlGlotDotNet.DebugTest;

public class TokenizerTests
{
    static TokenizerTests()
    {
        BundleInitializer.Initialize();
    }

    private static string TokenArrayToJson(Token[] tokens)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append('[');
        for (int i = 0; i < tokens.Length; i++)
        {
            if (i > 0) sb.Append(',');
            var token = tokens[i];
            sb.Append("{\"token_type\":");
            sb.Append((int)token.TokenType);
            sb.Append(",\"text\":\"");
            sb.Append(EscapeJson(token.Text));
            sb.Append("\",\"line\":");
            sb.Append(token.Span.Line);
            sb.Append(",\"col\":");
            sb.Append(token.Span.Column);
            sb.Append(",\"start\":");
            sb.Append(token.Span.Start);
            sb.Append(",\"end\":");
            sb.Append(token.Span.End);
            sb.Append('}');
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    [Fact]
    public void TestNativeLibraryLoad()
    {
        Console.WriteLine("Testing native library load...");
        try
        {
            Token[] tokens = Polyglot.Tokenize("SELECT 1");
            Console.WriteLine($"SUCCESS: Got {tokens.Length} tokens");
            Assert.True(tokens.Length > 0, "Should have at least one token");
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
        var tokens = Polyglot.Tokenize("foo", Dialect.Generic);

        Console.WriteLine($"Token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Span.Line} , col= {t.Span.Column}, start={t.Span.Start}, end={t.Span.End}");
        }

        Assert.Single(tokens);
        Assert.Equal(TokenType.VAR, tokens[0].TokenType);
        Assert.Equal(1, tokens[0].Span.Line);
        Assert.Equal(4, tokens[0].Span.Column);
        Assert.Equal(0, tokens[0].Span.Start);
        Assert.Equal(3, tokens[0].Span.End);
    }

    [Fact]
    public void TestSimpleWord()
    {
        var tokens = Polyglot.Tokenize("foo", Dialect.Generic);

        Assert.Single(tokens);
        Assert.Equal(TokenType.VAR, tokens[0].TokenType);
        Assert.Equal(4, tokens[0].Span.Column);
    }

    [Fact]
    public void TestNumber()
    {
        var tokens = Polyglot.Tokenize("123", Dialect.Generic);

        Assert.Single(tokens);
        Assert.Equal(TokenType.NUMBER, tokens[0].TokenType);
        Assert.Equal(4, tokens[0].Span.Column);
    }

    [Fact]
    public void TestSelectKeyword()
    {
        var tokens = Polyglot.Tokenize("SELECT", Dialect.Generic);

        Assert.Single(tokens);
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(7, tokens[0].Span.Column);
    }

    [Fact]
    public void TestSelectOne()
    {
        var tokens = Polyglot.Tokenize("SELECT 1", Dialect.Generic);

        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(TokenType.NUMBER, tokens[1].TokenType);
        Assert.Equal(7, tokens[0].Span.Column);
        Assert.Equal(9, tokens[1].Span.Column);
    }

    [Fact]
    public void TestCRLF()
    {
        var tokens = Polyglot.Tokenize("SELECT\r\n1", Dialect.Generic);

        Console.WriteLine($"CRLF test - token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Span.Line}, col={t.Span.Column}, start={t.Span.Start}, end={t.Span.Column}");
        }

        Assert.True(tokens.Length >= 2, $"Expected at least 2 tokens, got {tokens.Length}");
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(TokenType.NUMBER, tokens[1].TokenType);
    }

    [Fact]
    public void TestMultiline()
    {
        var tokens = Polyglot.Tokenize("SELECT\n1\n+2", Dialect.Generic);

        Console.WriteLine($"Multiline test - token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Span.Line} , col= {t.Span.Column}, start={t.Span.Start}, end={t.Span.Column}");
        }

        Assert.True(tokens.Length >= 3, $"Expected at least 3 tokens, got {tokens.Length}");
        Assert.Equal(1, tokens[0].Span.Line);
    }

    [Fact]
    public void TestComments()
    {
        var tokens = Polyglot.Tokenize("SELECT -- comment\n1", Dialect.Generic);

        Console.WriteLine($"Comment test - token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Span.Line}, col={t.Span.Column}, comments={t.Comments?.Length ?? 0}");
        }

        Assert.True(tokens.Length >= 2, $"Expected at least 2 tokens, got {tokens.Length}");
    }

    [Fact]
    public void TestSpaceKeywords()
    {
        var tokens = Polyglot.Tokenize("CHARACTER VARYING", Dialect.Generic);

        Console.WriteLine($"Space keyword test - token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
        }
    }

    [Fact]
    public void TestJinja()
    {
        var tokens = Polyglot.Tokenize("{{ foo }}", Dialect.Generic);

        Console.WriteLine($"Jinja test - token count: {tokens.Length}");
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
            var tokens = Polyglot.Tokenize("\\foo", Dialect.Generic);
            Console.WriteLine($"Command test - token count: {tokens.Length}");
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
        var tokens = Polyglot.Tokenize("SELECT 日本語", Dialect.Generic);

        Console.WriteLine($"Unicode test - token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}'");
        }

        Assert.True(tokens.Length >= 2, $"Expected at least 2 tokens, got {tokens.Length}");
    }

    [Fact]
    public void TestComplexQuery()
    {
        var sql = "SELECT a, b FROM t WHERE c = 1 AND d > 2";
        var tokens = Polyglot.Tokenize(sql, Dialect.Generic);

        Console.WriteLine($"Complex query test - token count: {tokens.Length}");
        foreach (var t in tokens)
        {
            Console.WriteLine($"  Token: type={t.TokenType}, text='{t.Text}', line={t.Span.Line}, col={t.Span.Column}");
        }

        Assert.True(tokens.Length > 5, $"Expected more than 5 tokens, got {tokens.Length}");
    }

    [Fact]
    public void TestErrorMessage()
    {
        Console.WriteLine("Error handling test");
        try
        {
            var tokens = Polyglot.Tokenize("SELECT <<--\n1", Dialect.Generic);
            Console.WriteLine($"  Token count: {tokens.Length}");
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
        Token[] tokens = Polyglot.Tokenize("SELECT 1");
        string json = TokenArrayToJson(tokens);
        Console.WriteLine($"Raw JSON: {json}");
        Assert.False(string.IsNullOrEmpty(json), "JSON output should not be empty");
        Assert.Contains("SELECT", json);
    }

    [Fact]
    public void TestRawJsonError()
    {
        Token[] tokens = Polyglot.Tokenize("SELECT <<--\n1");
        Console.WriteLine($"Raw JSON (error): {tokens.Length} tokens");
        Assert.True(tokens.Length > 0, "Should have tokens");
    }
}
