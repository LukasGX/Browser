using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Browser;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Browser browser = new();
        await browser.StartAsync(); 
    }
}

public class Browser
{
    readonly HttpClient client = new();
    readonly HtmlTokenizer tokenizer = new();

    public async Task StartAsync()
    {
        try
        {
            string responseBody = await client.GetStringAsync("https://example.com");

            foreach (Token token in tokenizer.Tokenize(responseBody))
            {
                Console.WriteLine(token);
            }

            Console.WriteLine(responseBody);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Fehler: {e.Message}");
        }
    }
}

public enum TokenType
{
    StartTag,
    EndTag,
    SelfClosingTag,
    Text,
    Doctype,
    Comment
}

public class Token(TokenType type, string value, Dictionary<string, string?> attributes)
{
    public TokenType Type { get; } = type;
    public string Value { get; } = value;
    public Dictionary<string, string?> Attributes { get; } = attributes;

    public override string ToString()
    {
        return $"{Type}: {Value}\nAttributes: {string.Join(", ", Attributes.Select(kv => $"{kv.Key}={kv.Value}"))}\n";
    }
}

public class TagInfo
{
    public string Name { get; set; } = "";
    public Dictionary<string, string?> Attributes { get; } = [];
}

public class HtmlTokenizer
{
    readonly List<string> selfClosingTags =
    [
        "area", "base", "br", "col", "embed", "hr", "img", "input",
        "link", "meta", "param", "source", "track", "wbr"
    ];

    public static TagInfo ParseTag(string tag)
    {
        TagInfo result = new();

        int i = 0;

        // read tagname
        while (i < tag.Length && !char.IsWhiteSpace(tag[i]))
        {
            result.Name += tag[i];
            i++;
        }

        while (i < tag.Length)
        {
            while (i < tag.Length && char.IsWhiteSpace(tag[i]))
                i++;

            if (i >= tag.Length)
                break;

            string attributeName = "";

            while (
                i < tag.Length &&
                tag[i] != '=' &&
                !char.IsWhiteSpace(tag[i])
            )
            {
                attributeName += tag[i];
                i++;
            }

            while (i < tag.Length && char.IsWhiteSpace(tag[i]))
                i++;

            // bool attributes
            if (i >= tag.Length || tag[i] != '=')
            {
                result.Attributes[attributeName] = null;
                continue;
            }

            i++; // '='

            while (i < tag.Length && char.IsWhiteSpace(tag[i]))
                i++;

            string attributeValue = "";

            if (i < tag.Length && (tag[i] == '"' || tag[i] == '\''))
            {
                char quote = tag[i];
                i++;

                while (i < tag.Length && tag[i] != quote)
                {
                    attributeValue += tag[i];
                    i++;
                }

                if (i < tag.Length)
                    i++;
            }
            else
            {
                while (
                    i < tag.Length &&
                    !char.IsWhiteSpace(tag[i])
                )
                {
                    attributeValue += tag[i];
                    i++;
                }
            }

            result.Attributes[attributeName] = attributeValue;
        }

        return result;
    }

    public List<Token> Tokenize(string html)
    {
        List<Token> tokens = new();

        int i = 0;

        while (i < html.Length)
        {
            if (html[i] == '<')
            {
                int end = i + 1;
                bool inQuotes = false;
                char quote = '\0';

                while (end < html.Length)
                {
                    char c = html[end];

                    if (!inQuotes)
                    {
                        if (c == '"' || c == '\'')
                        {
                            inQuotes = true;
                            quote = c;
                        }
                        else if (c == '>')
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (c == quote)
                        {
                            inQuotes = false;
                        }
                    }

                    end++;
                }

                string tag = html.Substring(i + 1, end - i - 1).TrimEnd();

                if (tag.EndsWith('/'))
                    tag = tag[..^1].TrimEnd();

                TagInfo info = ParseTag(tag);

                if (tag.ToLower().StartsWith("!doctype"))
                {
                    tokens.Add(new Token(
                        TokenType.Doctype,
                        tag,
                        []
                    ));
                }
                else if (tag.StartsWith("!--") && tag.EndsWith("--"))
                {
                    tokens.Add(new Token(
                        TokenType.Comment,
                        tag.Substring(3, tag.Length - 5),
                        []
                    ));
                }
                else if (tag.StartsWith('/'))
                {
                    tokens.Add(new Token(
                        TokenType.EndTag,
                        info.Name[1..],
                        info.Attributes
                    ));
                }
                else if (selfClosingTags.Contains(info.Name.ToLowerInvariant()))
                {
                    tokens.Add(new Token(
                        TokenType.SelfClosingTag,
                        info.Name,
                        info.Attributes
                    ));
                }
                else
                {
                    tokens.Add(new Token(
                        TokenType.StartTag,
                        info.Name,
                        info.Attributes
                    ));
                }

                i = end + 1;
            }
            else
            {
                int start = i;

                while (i < html.Length && html[i] != '<')
                {
                    i++;
                }

                string text = html.Substring(start, i - start);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    tokens.Add(new Token(
                        TokenType.Text,
                        text.Trim(),
                        []
                    ));
                }
            }
        }

        return tokens;
    }
}

public abstract class Node
{
    public Node? Parent { get; set; }
    public List<Node> Children { get; } = [];
}

public class ElementNode(string tagName) : Node
{
    public string TagName { get; } = tagName;

    public Dictionary<string, string> Attributes { get; } = [];
}

public class TextNode(string text) : Node
{
    public string Text { get; set; } = text;
}