using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.RssLogic
{
    public class RssService : IRssService
    {
        private readonly ILogger<RssService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public RssService(ILogger<RssService> logger, HttpClient httpClient, IConfiguration config)
        {
            _logger = logger;
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<rss> GetRssItems()
        {
            _logger.LogInformation("GetRssItems -> execution started !");

            var uri = _config["RssService:uri"];
            var responseString = await _httpClient.GetStringAsync(uri);

            _logger.LogInformation("Rss response: " + responseString);
            var serializer = new XmlSerializer(typeof(rss));
            rss result;
            using (TextReader reader = new StringReader(responseString))
            {
                result = (rss)serializer.Deserialize(reader);
            }

            return result;
        }
    }
}
