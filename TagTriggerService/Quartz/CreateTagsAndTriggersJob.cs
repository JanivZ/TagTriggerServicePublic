using Microsoft.Extensions.Logging;
using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;
using TagTriggerService.Logic.GoogleLogic;
using TagTriggerService.Logic.RssLogic;
using TagTriggerService.Model;

namespace TagTriggerService.Quartz
{
    [DisallowConcurrentExecution]
    public class CreateTagsAndTriggersJob : IJob
    {
        private readonly ILogger<CreateTagsAndTriggersJob> _logger;
        private readonly IGoogleTagManagerOrchestrator _googleTagManagerHandler;
        private readonly IRssService _rssService;

        public CreateTagsAndTriggersJob(ILogger<CreateTagsAndTriggersJob> logger, IGoogleTagManagerOrchestrator googleTagManagerHandler, IRssService rssService)
        {
            _logger = logger;
            _googleTagManagerHandler = googleTagManagerHandler;
            _rssService = rssService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("CreateTagsAndTriggersJob -> execution started !");
            var rssResponse = _rssService.GetRssItems().Result;

            _ = _googleTagManagerHandler.CreateTagsAndTriggersForItems(rssResponse.channel.item).Result;
            
            return Task.CompletedTask;
        }
    }
}