using BlazorApp.Shared;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class WordOfTheDay
    {
        private const string RequestUri = "https://www.focloir.ie/";
        private readonly ILogger _logger;
        private IHttpClientFactory _httpClientFactory;

        public WordOfTheDay(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<WordOfTheDay>();
            _httpClientFactory = httpClientFactory;
        }

        [Function("WordOfTheDay")]
        public async Task<WordOfTheDayModel?> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            WordOfTheDayModel? result = null;

            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(RequestUri);

            if (response.IsSuccessStatusCode)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(await response.Content.ReadAsStringAsync());

                var nodeCollection = doc.DocumentNode.SelectNodes("//div[@class=\"wotdEntryHdr\"]");

                var link = nodeCollection.Descendants("a").FirstOrDefault();

                result = new WordOfTheDayModel();

                if (link is not null)
                {
                    result.Url = link.Attributes["href"].Value;

                    var nextNode = link.ChildNodes.FirstOrDefault(c => c.Attributes["class"].Value == "hwd");
                    if (nextNode is not null)
                    {
                        result.Word = nextNode.ChildNodes.FirstOrDefault(c => c.Attributes["type"].Value == "super")?.InnerText;
                    }
                }

                var translationNode = doc.DocumentNode.SelectNodes("//div[@class=\"wotdEntryBody\"]").Descendants();

                if (translationNode.Any())
                {
                    result.Type = translationNode.First().InnerText;
                    result.Translation = translationNode.Last().InnerText;
                }
            }
            else
            {
                _logger.LogError($"Can not reach {RequestUri}");
            }

            return result;
        }
    }
}
