using System.Net;
using System.Text.Json;
using System.Text;
using BlueprintBlue.PEG;
using System.Text.Json.Nodes;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;

namespace BlueprintBlue
{
    public class Blueprint
    {
        public string PinshotBlue { get; private set; }
        public Blueprint(string pinshotURL)
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