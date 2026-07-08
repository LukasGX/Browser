using Xunit;

namespace Browser.Tests;

public class HtmlTokenizerTests
{
    private readonly HtmlTokenizer tokenizer = new();


    [Fact]
    public void ParsesSimpleTag()
    {
        var tokens = tokenizer.Tokenize("<h1>Hello</h1>");

        Assert.Equal(3, tokens.Count);

        Assert.Equal(TokenType.StartTag, tokens[0].Type);
        Assert.Equal("h1", tokens[0].Value);

        Assert.Equal(TokenType.Text, tokens[1].Type);
        Assert.Equal("Hello", tokens[1].Value);

        Assert.Equal(TokenType.EndTag, tokens[2].Type);
        Assert.Equal("h1", tokens[2].Value);
    }


    [Fact]
    public void ParsesAttributes()
    {
        var tokens = tokenizer.Tokenize(
            """<a href="https://example.com" target="_blank">Link</a>"""
        );

        Token token = tokens[0];

        Assert.Equal("a", token.Value);

        Assert.Equal(
            "https://example.com",
            token.Attributes["href"]
        );

        Assert.Equal(
            "_blank",
            token.Attributes["target"]
        );
    }


    [Fact]
    public void ParsesSelfClosingTags()
    {
        var tokens = tokenizer.Tokenize(
            """<img src="test.png">"""
        );

        Assert.Single(tokens);

        Assert.Equal(
            TokenType.SelfClosingTag,
            tokens[0].Type
        );

        Assert.Equal(
            "test.png",
            tokens[0].Attributes["src"]
        );
    }


    [Fact]
    public void ParsesDoctype()
    {
        var tokens = tokenizer.Tokenize(
            "<!doctype html>"
        );

        Assert.Single(tokens);
        Assert.Equal(TokenType.Doctype, tokens[0].Type);
    }


    [Fact]
    public void ParsesComments()
    {
        var tokens = tokenizer.Tokenize(
            "<!-- hello -->"
        );

        Assert.Single(tokens);

        Assert.Equal(
            TokenType.Comment,
            tokens[0].Type
        );

        Assert.Equal(
            " hello ",
            tokens[0].Value
        );
    }


    [Fact]
    public void ParsesText()
    {
        var tokens = tokenizer.Tokenize(
            "Hello World"
        );

        Assert.Single(tokens);

        Assert.Equal(
            TokenType.Text,
            tokens[0].Type
        );

        Assert.Equal(
            "Hello World",
            tokens[0].Value
        );
    }

    [Fact]
    public void ParsesAttributeWithSpaces()
    {
        var tokens = tokenizer.Tokenize(
            """<div class="hello world">Test</div>"""
        );

        Assert.Equal(
            "hello world",
            tokens[0].Attributes["class"]
        );
    }

    [Fact]
    public void ParsesSingleQuotedAttributes()
    {
        var tokens = tokenizer.Tokenize(
            "<a href='https://example.com'>Link</a>"
        );

        Assert.Equal(
            "https://example.com",
            tokens[0].Attributes["href"]
        );
    }

    [Fact]
    public void ParsesBooleanAttribute()
    {
        var tokens = tokenizer.Tokenize(
            "<input disabled>"
        );

        Assert.True(tokens[0].Attributes.ContainsKey("disabled"));
        Assert.Null(tokens[0].Attributes["disabled"]);
    }

    [Fact]
    public void ParsesMultipleSpaces()
    {
        var tokens = tokenizer.Tokenize(
            """<div     class="test"      id="main">Hello</div>"""
        );

        Assert.Equal("test", tokens[0].Attributes["class"]);
        Assert.Equal("main", tokens[0].Attributes["id"]);
    }

    [Fact]
    public void ParsesTabs()
    {
        var tokens = tokenizer.Tokenize(
            "<div\tclass=\"test\"\tid=\"main\">"
        );

        Assert.Equal("test", tokens[0].Attributes["class"]);
        Assert.Equal("main", tokens[0].Attributes["id"]);
    }

    [Fact]
    public void ParsesNewlines()
    {
        var tokens = tokenizer.Tokenize(
            "<div\nclass=\"test\"\nid=\"main\">"
        );

        Assert.Equal("test", tokens[0].Attributes["class"]);
        Assert.Equal("main", tokens[0].Attributes["id"]);
    }

    [Fact]
    public void ParsesUnquotedAttribute()
    {
        var tokens = tokenizer.Tokenize(
            "<meta charset=UTF-8>"
        );

        Assert.Equal(
            "UTF-8",
            tokens[0].Attributes["charset"]
        );
    }

    [Fact]
    public void ParsesEmptyAttribute()
    {
        var tokens = tokenizer.Tokenize(
            "<input value=\"\">"
        );

        Assert.Equal(
            "",
            tokens[0].Attributes["value"]
        );
    }

    [Fact]
    public void ParsesMultipleBooleanAttributes()
    {
        var tokens = tokenizer.Tokenize(
            "<input checked disabled readonly>"
        );

        Assert.True(tokens[0].Attributes.ContainsKey("checked"));
        Assert.True(tokens[0].Attributes.ContainsKey("disabled"));
        Assert.True(tokens[0].Attributes.ContainsKey("readonly"));
    }

    [Fact]
    public void ParsesEmptyElement()
    {
        var tokens = tokenizer.Tokenize(
            "<div></div>"
        );

        Assert.Equal(2, tokens.Count);

        Assert.Equal(TokenType.StartTag, tokens[0].Type);
        Assert.Equal(TokenType.EndTag, tokens[1].Type);
    }

    [Fact]
    public void ParsesNestedTags()
    {
        var tokens = tokenizer.Tokenize(
            "<div><span>Hello</span></div>"
        );

        Assert.Equal(5, tokens.Count);

        Assert.Equal("div", tokens[0].Value);
        Assert.Equal(TokenType.StartTag, tokens[0].Type);
        Assert.Equal("span", tokens[1].Value);
        Assert.Equal(TokenType.StartTag, tokens[1].Type);
        Assert.Equal("Hello", tokens[2].Value);
        Assert.Equal(TokenType.Text, tokens[2].Type);
        Assert.Equal("span", tokens[3].Value);
        Assert.Equal(TokenType.EndTag, tokens[3].Type);
        Assert.Equal("div", tokens[4].Value);
        Assert.Equal(TokenType.EndTag, tokens[4].Type);
    }

    [Fact]
    public void ParsesGreaterThanInsideQuotedAttribute()
    {
        var tokens = tokenizer.Tokenize(
            """<div title="1 > 0"></div>"""
        );

        Assert.Equal(
            "1 > 0",
            tokens[0].Attributes["title"]
        );
    }

    [Fact]
    public void ParsesLessThanInsideQuotedAttribute()
    {
        var tokens = tokenizer.Tokenize(
            """<div title="1 < 2"></div>"""
        );

        Assert.Equal(
            "1 < 2",
            tokens[0].Attributes["title"]
        );
    }

    [Fact]
    public void ParsesSlashSelfClosingTag()
    {
        var tokens = tokenizer.Tokenize(
            """<img src="test.png" />"""
        );

        Assert.Single(tokens);

        Assert.Equal(
            TokenType.SelfClosingTag,
            tokens[0].Type
        );

        Assert.Equal(
            "test.png",
            tokens[0].Attributes["src"]
        );

        Assert.Single(tokens[0].Attributes);
    }

    [Fact]
    public void ParsesSelfClosingBooleanAttribute()
    {
        var tokens = tokenizer.Tokenize(
            "<input disabled />"
        );

        Assert.Single(tokens);

        Assert.Equal(
            TokenType.SelfClosingTag,
            tokens[0].Type
        );

        Assert.True(
            tokens[0].Attributes.ContainsKey("disabled")
        );

        Assert.Null(
            tokens[0].Attributes["disabled"]
        );

        Assert.Single(tokens[0].Attributes);
    }
}