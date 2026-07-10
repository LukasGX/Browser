namespace Browser.Tests;

public class HtmlParserTests
{
    private readonly HtmlTokenizer tokenizer = new();
    private readonly HtmlParser parser = new();

    [Fact]
    public void ParsesSingleElement()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<div></div>")
        );

        Assert.Single(document.Children);

        var div = Assert.IsType<ElementNode>(document.Children[0]);
        Assert.Equal("div", div.TagName);
    }

    [Fact]
    public void ParsesNestedElements()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<div><h1>Hello</h1></div>")
        );

        var div = Assert.IsType<ElementNode>(document.Children[0]);
        var h1 = Assert.IsType<ElementNode>(div.Children[0]);
        var text = Assert.IsType<TextNode>(h1.Children[0]);

        Assert.Equal("div", div.TagName);
        Assert.Equal("h1", h1.TagName);
        Assert.Equal("Hello", text.Text);
    }

    [Fact]
    public void ParsesSiblingElements()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<div></div><span></span>")
        );

        Assert.Equal(2, document.Children.Count);

        Assert.Equal(
            "div",
            Assert.IsType<ElementNode>(document.Children[0]).TagName
        );

        Assert.Equal(
            "span",
            Assert.IsType<ElementNode>(document.Children[1]).TagName
        );
    }

    [Fact]
    public void ParsesAttributes()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize(
                """<a href="https://example.com">Link</a>"""
            )
        );

        var a = Assert.IsType<ElementNode>(document.Children[0]);

        Assert.Equal(
            "https://example.com",
            a.Attributes["href"]
        );
    }

    [Fact]
    public void ParsesSelfClosingElement()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<img src=\"test.png\">")
        );

        var img = Assert.IsType<ElementNode>(document.Children[0]);

        Assert.Equal("img", img.TagName);
        Assert.Empty(img.Children);
    }

    [Fact]
    public void ParsesTextNode()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<p>Hello World</p>")
        );

        var p = Assert.IsType<ElementNode>(document.Children[0]);
        var text = Assert.IsType<TextNode>(p.Children[0]);

        Assert.Equal("Hello World", text.Text);
    }

    [Fact]
    public void ParsesComment()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<!-- hello -->")
        );

        var comment = Assert.IsType<CommentNode>(
            document.Children[0]
        );

        Assert.Equal(" hello ", comment.Comment);
    }

    [Fact]
    public void IgnoresDoctype()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<!doctype html><html></html>")
        );

        Assert.Single(document.Children);

        Assert.Equal(
            "html",
            Assert.IsType<ElementNode>(document.Children[0]).TagName
        );
    }

    [Fact]
    public void ParsesDeepHierarchy()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize(
                "<html><body><div><p>Test</p></div></body></html>"
            )
        );

        var html = Assert.IsType<ElementNode>(document.Children[0]);
        var body = Assert.IsType<ElementNode>(html.Children[0]);
        var div = Assert.IsType<ElementNode>(body.Children[0]);
        var p = Assert.IsType<ElementNode>(div.Children[0]);
        var text = Assert.IsType<TextNode>(p.Children[0]);

        Assert.Equal("Test", text.Text);
    }

    [Fact]
    public void SetsParentOfElement()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<div></div>")
        );

        var div = Assert.IsType<ElementNode>(document.Children[0]);

        Assert.Same(document, div.Parent);
    }

    [Fact]
    public void SetsParentOfNestedElement()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<div><span></span></div>")
        );

        var div = Assert.IsType<ElementNode>(document.Children[0]);
        var span = Assert.IsType<ElementNode>(div.Children[0]);

        Assert.Same(div, span.Parent);
        Assert.Same(document, div.Parent);
    }

    [Fact]
    public void SetsParentOfTextNode()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<p>Hello</p>")
        );

        var p = Assert.IsType<ElementNode>(document.Children[0]);
        var text = Assert.IsType<TextNode>(p.Children[0]);

        Assert.Same(p, text.Parent);
    }

    [Fact]
    public void SetsParentOfCommentNode()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize("<div><!-- hello --></div>")
        );

        var div = Assert.IsType<ElementNode>(document.Children[0]);
        var comment = Assert.IsType<CommentNode>(div.Children[0]);

        Assert.Same(div, comment.Parent);
    }

    [Fact]
    public void AllNodesHaveCorrectParents()
    {
        var document = (DocumentNode)parser.Parse(
            tokenizer.Tokenize(
                "<div><p>Hello</p><img></div>"
            )
        );

        var div = Assert.IsType<ElementNode>(document.Children[0]);
        var p = Assert.IsType<ElementNode>(div.Children[0]);
        var text = Assert.IsType<TextNode>(p.Children[0]);
        var img = Assert.IsType<ElementNode>(div.Children[1]);

        Assert.Same(document, div.Parent);
        Assert.Same(div, p.Parent);
        Assert.Same(p, text.Parent);
        Assert.Same(div, img.Parent);
    }
}