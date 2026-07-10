namespace Browser;

public abstract class Node
{
    public Node? Parent { get; set; }
    public List<Node> Children { get; } = [];

    public IEnumerable<Node> Descendants()
    {
        foreach (var child in Children)
        {
            yield return child;

            foreach (var descendant in child.Descendants())
                yield return descendant;
        }
    }

    public IEnumerable<Node> DescendantsAndSelf()
    {
        yield return this;

        foreach (Node descendant in Descendants())
            yield return descendant;
    }

    public ElementNode? GetElementById(string id)
    {
        return DescendantsAndSelf()
            .OfType<ElementNode>()
            .FirstOrDefault(e =>
                e.Attributes.TryGetValue("id", out string? value) &&
                value == id
            );
    }

    public IEnumerable<ElementNode> GetElementsByClassName(string className)
    {
        return DescendantsAndSelf()
            .OfType<ElementNode>()
            .Where(e =>
                e.Attributes.TryGetValue("class", out string? value) &&
                value != null &&
                value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains(className)
            );
    }

    public IEnumerable<ElementNode> GetElementsByTagName(string tagName)
    {
        return DescendantsAndSelf()
            .OfType<ElementNode>()
            .Where(e =>
                e.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase)
            );
    }
}

public class DocumentNode(string doctype) : Node
{
    public string Doctype { get; set; } = doctype;
}

public class ElementNode(string tagName, Dictionary<string, string?> attributes) : Node
{
    public string TagName { get; } = tagName;

    public Dictionary<string, string?> Attributes { get; } = attributes;
}

public class TextNode(string text) : Node
{
    public string Text { get; set; } = text;
}

public class CommentNode(string comment) : Node
{
    public string Comment { get; set; } = comment;
}

public class HtmlParser
{
    public Node Parse(List<Token> tokens)
    {
        DocumentNode document = new("html");
        Stack<Node> stack = new();

        stack.Push(document);

        foreach (Token token in tokens)
        {
            if (token.Type == TokenType.Doctype)
                continue;
            else if (token.Type == TokenType.Comment)
            {
                CommentNode commentNode = new(token.Value)
                {
                    Parent = stack.Peek()
                };
                stack.Peek().Children.Add(commentNode);
            }
            else if (token.Type == TokenType.StartTag)
            {
                ElementNode elementNode = new(token.Value, token.Attributes)
                {
                    Parent = stack.Peek()
                };
                stack.Peek().Children.Add(elementNode);
                stack.Push(elementNode);
            }
            else if (token.Type == TokenType.EndTag)
            {
                stack.Pop();
            }
            else if (token.Type == TokenType.SelfClosingTag)
            {
                ElementNode elementNode = new(token.Value, token.Attributes)
                {
                    Parent = stack.Peek()
                };
                stack.Peek().Children.Add(elementNode);
            }
            else if (token.Type == TokenType.Text)
            {
                TextNode textNode = new(token.Value)
                {
                    Parent = stack.Peek()
                };
                stack.Peek().Children.Add(textNode);
            }
        }

        return document;
    }
}