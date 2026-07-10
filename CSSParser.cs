namespace Browser;

public class Stylesheet
{
    public List<CSSRule> Rules { get; } = [];
}

public class CSSRule(List<string> selectors)
{
    public List<string> Selectors { get; } = selectors;
    public List<CSSDeclaration> Declarations { get; } = [];
}

public class CSSDeclaration(string property, string value)
{
    public string Property { get; set; } = property;
    public string Value { get; set; } = value;
}

public class CSSParser
{
    public Stylesheet Parse(List<CSSToken> tokens)
    {
        Stylesheet stylesheet = new();

        List<string> selectors = [];

        bool inRule = false;
        bool readingValue = false;

        string propertyName = "";
        List<string> propertyValueParts = [];

        foreach (CSSToken token in tokens)
        {
            if (!inRule)
            {
                switch (token.Type)
                {
                    case CSSTokenType.Identifier:
                        selectors.Add(token.Value);
                        break;

                    case CSSTokenType.Comma:
                        break;

                    case CSSTokenType.OpenBrace:
                        stylesheet.Rules.Add(new CSSRule(selectors));
                        selectors = [];
                        inRule = true;
                        break;
                }

                continue;
            }

            if (!readingValue)
            {
                switch (token.Type)
                {
                    case CSSTokenType.Identifier:
                        propertyName = token.Value;
                        break;

                    case CSSTokenType.Colon:
                        readingValue = true;
                        break;

                    case CSSTokenType.CloseBrace:
                        inRule = false;
                        break;
                }

                continue;
            }

            switch (token.Type)
            {
                case CSSTokenType.Semicolon:

                    stylesheet.Rules.Last().Declarations.Add(
                        new CSSDeclaration(
                            propertyName,
                            string.Concat(propertyValueParts)
                        )
                    );

                    propertyName = "";
                    propertyValueParts.Clear();
                    readingValue = false;

                    break;

                case CSSTokenType.CloseBrace:

                    stylesheet.Rules.Last().Declarations.Add(
                        new CSSDeclaration(
                            propertyName,
                            string.Concat(propertyValueParts)
                        )
                    );

                    propertyName = "";
                    propertyValueParts.Clear();
                    readingValue = false;
                    inRule = false;

                    break;

                default:

                    propertyValueParts.Add(token.Value);

                    break;
            }
        }

        return stylesheet;
    }
}