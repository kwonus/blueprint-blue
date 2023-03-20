namespace Pinshot.Blue
{
    using Pinshot.PEG;
    using System.Net.Http.Json;

    public class PinshotSvc
    {
        public string PinshotBlueURL { get; private set; }
        public PinshotSvc(string pinshotURL)
        {
            this.PinshotBlueURL = pinshotURL;
        }
        public async Task<RootParse?> Parse(string command)
        {
            var stmt = new QuelleStatement(command);
            var request = new HttpRequestMessage(HttpMethod.Post, this.PinshotBlueURL)
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