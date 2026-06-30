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
        Assert.Equal(0, tokens[0].Line);
        Assert.Equal(3, tokens[0].Col);
        Assert.Equal(0, tokens[0].Start);
        Assert.Equal(2, tokens[0].End);
    }

    [Fact]
    public void TestSimpleWord()
    {
        var tokens = new Tokenizer().Tokenize("foo");

        Assert.Single(tokens);
        Assert.Equal(TokenType.VAR, tokens[0].TokenType);
        Assert.Equal(3, tokens[0].Col);
    }

    [Fact]
    public void TestNumber()
    {
        var tokens = new Tokenizer().Tokenize("123");

        Assert.Single(tokens);
        Assert.Equal(TokenType.NUMBER, tokens[0].TokenType);
        Assert.Equal(3, tokens[0].Col);
    }

    [Fact]
    public void TestSelectKeyword()
    {
        var tokens = new Tokenizer().Tokenize("SELECT");

        Assert.Single(tokens);
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(6, tokens[0].Col);
    }

    [Fact]
    public void TestSelectOne()
    {
        var tokens = new Tokenizer().Tokenize("SELECT 1");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.SELECT, tokens[0].TokenType);
        Assert.Equal(TokenType.NUMBER, tokens[1].TokenType);
        Assert.Equal(6, tokens[0].Col);
        Assert.Equal(8, tokens[1].Col);
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
        Assert.Equal(0, tokens[0].Line);
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
