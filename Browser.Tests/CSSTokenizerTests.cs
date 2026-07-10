using Xunit;

namespace Browser.Tests;

public class CSSTokenizerTests
{
    private readonly CSSTokenizer tokenizer = new();

    [Fact]
    public void ParsesIdentifier()
    {
        var tokens = tokenizer.Tokenize("body");

        Assert.Single(tokens);

        Assert.Equal(CSSTokenType.Identifier, tokens[0].Type);
        Assert.Equal("body", tokens[0].Value);
    }

    [Fact]
    public void ParsesBraces()
    {
        var tokens = tokenizer.Tokenize("{}");

        Assert.Equal(2, tokens.Count);

        Assert.Equal(CSSTokenType.OpenBrace, tokens[0].Type);
        Assert.Equal(CSSTokenType.CloseBrace, tokens[1].Type);
    }

    [Fact]
    public void ParsesColon()
    {
        var tokens = tokenizer.Tokenize(":");

        Assert.Single(tokens);

        Assert.Equal(CSSTokenType.Colon, tokens[0].Type);
    }

    [Fact]
    public void ParsesSemicolon()
    {
        var tokens = tokenizer.Tokenize(";");

        Assert.Single(tokens);

        Assert.Equal(CSSTokenType.Semicolon, tokens[0].Type);
    }

    [Fact]
    public void ParsesComma()
    {
        var tokens = tokenizer.Tokenize(",");

        Assert.Single(tokens);

        Assert.Equal(CSSTokenType.Comma, tokens[0].Type);
    }

    [Fact]
    public void ParsesParentheses()
    {
        var tokens = tokenizer.Tokenize("()");

        Assert.Equal(2, tokens.Count);

        Assert.Equal(CSSTokenType.OpenParen, tokens[0].Type);
        Assert.Equal(CSSTokenType.CloseParen, tokens[1].Type);
    }

    [Fact]
    public void ParsesSimpleRule()
    {
        var tokens = tokenizer.Tokenize(
            "body { color: red; }"
        );

        Assert.Equal(7, tokens.Count);

        Assert.Equal("body", tokens[0].Value);
        Assert.Equal(CSSTokenType.OpenBrace, tokens[1].Type);
        Assert.Equal("color", tokens[2].Value);
        Assert.Equal(CSSTokenType.Colon, tokens[3].Type);
        Assert.Equal("red", tokens[4].Value);
        Assert.Equal(CSSTokenType.Semicolon, tokens[5].Type);
        Assert.Equal(CSSTokenType.CloseBrace, tokens[6].Type);
    }

    [Fact]
    public void IgnoresWhitespace()
    {
        var tokens = tokenizer.Tokenize(
            " \n\t body \t "
        );

        Assert.Single(tokens);

        Assert.Equal("body", tokens[0].Value);
    }

    [Fact]
    public void ParsesMultipleSelectors()
    {
        var tokens = tokenizer.Tokenize(
            "h1, h2 {}"
        );

        Assert.Equal(5, tokens.Count);

        Assert.Equal("h1", tokens[0].Value);
        Assert.Equal(CSSTokenType.Comma, tokens[1].Type);
        Assert.Equal("h2", tokens[2].Value);
        Assert.Equal(CSSTokenType.OpenBrace, tokens[3].Type);
        Assert.Equal(CSSTokenType.CloseBrace, tokens[4].Type);
    }

    [Fact]
    public void ParsesPseudoClass()
    {
        var tokens = tokenizer.Tokenize(
            "a:hover {}"
        );

        Assert.Equal(5, tokens.Count);

        Assert.Equal(CSSTokenType.Identifier, tokens[0].Type);
        Assert.Equal("a", tokens[0].Value);

        Assert.Equal(CSSTokenType.Colon, tokens[1].Type);

        Assert.Equal(CSSTokenType.Identifier, tokens[2].Type);
        Assert.Equal("hover", tokens[2].Value);

        Assert.Equal(CSSTokenType.OpenBrace, tokens[3].Type);
        Assert.Equal(CSSTokenType.CloseBrace, tokens[4].Type);
    }

    [Fact]
    public void ParsesClassSelector()
    {
        var tokens = tokenizer.Tokenize(
            ".button {}"
        );

        Assert.Equal(".button", tokens[0].Value);
    }

    [Fact]
    public void ParsesIdSelector()
    {
        var tokens = tokenizer.Tokenize(
            "#main {}"
        );

        Assert.Equal("#main", tokens[0].Value);
    }

    [Fact]
    public void ParsesCssValue()
    {
        var tokens = tokenizer.Tokenize(
            "font-size:16px;"
        );

        Assert.Equal("font-size", tokens[0].Value);
        Assert.Equal(CSSTokenType.Colon, tokens[1].Type);
        Assert.Equal("16px", tokens[2].Value);
        Assert.Equal(CSSTokenType.Semicolon, tokens[3].Type);
    }

    [Fact]
    public void ParsesUrlFunction()
    {
        var tokens = tokenizer.Tokenize(
            "background: url(test.png);"
        );

        Assert.Equal(7, tokens.Count);

        Assert.Equal(CSSTokenType.Identifier, tokens[0].Type);
        Assert.Equal("background", tokens[0].Value);

        Assert.Equal(CSSTokenType.Colon, tokens[1].Type);

        Assert.Equal(CSSTokenType.Identifier, tokens[2].Type);
        Assert.Equal("url", tokens[2].Value);

        Assert.Equal(CSSTokenType.OpenParen, tokens[3].Type);

        Assert.Equal(CSSTokenType.Identifier, tokens[4].Type);
        Assert.Equal("test.png", tokens[4].Value);

        Assert.Equal(CSSTokenType.CloseParen, tokens[5].Type);

        Assert.Equal(CSSTokenType.Semicolon, tokens[6].Type);
    }
}