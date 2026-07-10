namespace Browser;

public enum CSSTokenType
{
    Identifier,
    OpenBrace,
    CloseBrace,
    Colon,
    Semicolon,
    Comma,
    OpenParen,
    CloseParen
}

public class CSSToken(CSSTokenType tokenType, string value)
{
    public CSSTokenType Type { get; } = tokenType;
    public string Value { get; } = value;

    public override string ToString()
    {
        return $"{Type}: {Value}";
    }
}

public class CSSTokenizer
{
    public List<CSSToken> Tokenize(string css)
    {
        List<CSSToken> tokens = [];

        int i = 0;

        while (i < css.Length)
        {
            if (char.IsWhiteSpace(css[i]))
            {
                i++;
                continue;
            }

            switch (css[i])
            {
                case '{':
                    tokens.Add(new(
                        CSSTokenType.OpenBrace,
                        "{"
                    ));
                    i++;
                    break;

                case '}':
                    tokens.Add(new(
                        CSSTokenType.CloseBrace,
                        "}"
                    ));
                    i++;
                    break;

                case ':':
                    tokens.Add(new(
                        CSSTokenType.Colon,
                        ":"
                    ));
                    i++;
                    break;

                case ';':
                    tokens.Add(new(
                        CSSTokenType.Semicolon,
                        ";"
                    ));
                    i++;
                    break;

                case ',':
                    tokens.Add(new(
                        CSSTokenType.Comma,
                        ","
                    ));
                    i++;
                    break;

                case '(':
                    tokens.Add(new(
                        CSSTokenType.OpenParen,
                        "("
                    ));
                    i++;
                    break;

                case ')':
                    tokens.Add(new(
                        CSSTokenType.CloseParen,
                        ")"
                    ));
                    i++;
                    break;

                default:
                    string identifier = "";

                    while (
                        i < css.Length &&
                        !char.IsWhiteSpace(css[i]) &&
                        css[i] != '{' &&
                        css[i] != '}' &&
                        css[i] != ':' &&
                        css[i] != ';' &&
                        css[i] != ',' &&
                        css[i] != '(' &&
                        css[i] != ')'
                    )
                    {
                        identifier += css[i];
                        i++;
                    }

                    if (identifier.Length > 0)
                    {
                        tokens.Add(new(
                            CSSTokenType.Identifier,
                            identifier
                        ));
                    }

                    break;
            }
        }

        return tokens;
    }
}