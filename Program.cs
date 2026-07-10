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
    readonly HtmlParser parser = new();

    public async Task StartAsync()
    {
        try
        {
            string responseBody = await client.GetStringAsync("https://example.com");

            List<Token> tokens = tokenizer.Tokenize(responseBody);
            Node documentNode = parser.Parse(tokens);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}