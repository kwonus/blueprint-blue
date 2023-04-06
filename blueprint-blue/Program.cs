
namespace Blueprint.Blue
{
    public class QuelleSvc
    {
        public static void Main(string[] args)
        {
            string schema = @"C:\src\blueprint-blue\blueprint-blue\blueprint-blue.graphql";

            // Open the file to read from.
            using (StreamReader sr = File.OpenText(schema))
            {
                schema = sr.ReadToEnd();
            }
            SchemaBuilder.New().AddDocumentFromString(schema)
              .AddResolver(context => "Hello!")
              .Create();

            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
        public string PinshotBlue { get; private set; }
        public QuelleSvc(string pinshotURL)
        {
            this.PinshotBlue = pinshotURL;
        }
        public async Task<RootParse?> Parse(string command)
        {
            var stmt = new QuelleStatement(command);
            var request = new HttpRequestMessage(HttpMethod.Post, this.PinshotBlue)
            {
                Content = JsonContent.Create(stmt)
            };

            HttpClient http = new HttpClient();
            var response = http.Send(request);

            var result = RootParse.Create(await response.Content.ReadAsStreamAsync());

            return result.ok ? result.root : null;
        }
    }
}