using Xunit;

namespace Browser.Tests;

public class DOMHelperTests
{
    private readonly HtmlTokenizer tokenizer = new();
    private readonly HtmlParser parser = new();

    private DocumentNode Parse(string html)
    {
        return (DocumentNode)parser.Parse(
            tokenizer.Tokenize(html)
        );
    }


    [Fact]
    public void DescendantsReturnsAllNodes()
    {
        var document = Parse(
            "<div><h1>Hello</h1><p>World</p></div>"
        );

        var descendants = document.Descendants().ToList();

        Assert.Equal(5, descendants.Count);

        Assert.IsType<ElementNode>(descendants[0]);
        Assert.IsType<ElementNode>(descendants[1]);
        Assert.IsType<TextNode>(descendants[2]);
        Assert.IsType<ElementNode>(descendants[3]);
        Assert.IsType<TextNode>(descendants[4]);
    }


    [Fact]
    public void DescendantsAndSelfContainsCurrentNode()
    {
        var document = Parse(
            "<div></div>"
        );

        var nodes = document.DescendantsAndSelf().ToList();

        Assert.Equal(document, nodes[0]);
        Assert.Contains(document.Children[0], nodes);
    }


    [Fact]
    public void FindsElementById()
    {
        var document = Parse(
            """<div><p id="test">Hello</p></div>"""
        );

        var element = document.GetElementById("test");

        Assert.NotNull(element);
        Assert.Equal("p", element.TagName);
        Assert.Equal("test", element.Attributes["id"]);
    }


    [Fact]
    public void ReturnsNullWhenIdDoesNotExist()
    {
        var document = Parse(
            "<div></div>"
        );

        var element = document.GetElementById("missing");

        Assert.Null(element);
    }


    [Fact]
    public void FindsElementsByClassName()
    {
        var document = Parse(
            """
            <div>
                <p class="text">A</p>
                <span class="text red">B</span>
                <p class="other">C</p>
            </div>
            """
        );

        var elements = document
            .GetElementsByClassName("text")
            .ToList();

        Assert.Equal(2, elements.Count);

        Assert.Equal("p", elements[0].TagName);
        Assert.Equal("span", elements[1].TagName);
    }


    [Fact]
    public void FindsElementsByTagName()
    {
        var document = Parse(
            """
            <div>
                <p>A</p>
                <p>B</p>
                <span>C</span>
            </div>
            """
        );

        var paragraphs = document
            .GetElementsByTagName("p")
            .ToList();

        Assert.Equal(2, paragraphs.Count);

        Assert.All(
            paragraphs,
            p => Assert.Equal("p", p.TagName)
        );
    }


    [Fact]
    public void TagNameSearchIsCaseInsensitive()
    {
        var document = Parse(
            "<DIV></DIV>"
        );

        var elements = document
            .GetElementsByTagName("div")
            .ToList();

        Assert.Single(elements);
        Assert.Equal("DIV", elements[0].TagName);
    }


    [Fact]
    public void ParentReferencesAreCorrect()
    {
        var document = Parse(
            "<div><p>Hello</p></div>"
        );

        var div = Assert.IsType<ElementNode>(
            document.Children[0]
        );

        var p = Assert.IsType<ElementNode>(
            div.Children[0]
        );

        var text = Assert.IsType<TextNode>(
            p.Children[0]
        );

        Assert.Equal(document, div.Parent);
        Assert.Equal(div, p.Parent);
        Assert.Equal(p, text.Parent);
    }


    [Fact]
    public void CanFindNestedElement()
    {
        var document = Parse(
            """
            <html>
                <body>
                    <div id="container">
                        <button class="primary">
                            Click
                        </button>
                    </div>
                </body>
            </html>
            """
        );

        var button = document.GetElementById("container")
            ?.GetElementsByClassName("primary")
            .FirstOrDefault();

        Assert.NotNull(button);
        Assert.Equal("button", button.TagName);
    }
}