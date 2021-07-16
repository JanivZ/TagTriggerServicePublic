using Google.Apis.TagManager.v2.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.GoogleLogic
{
    public class TagHandler : ITagHandler
    {
        private readonly ILogger<TagHandler> _logger;
        private readonly IGoogleTagManagerServiceWrapper _googleTagManagerServiceWrapper;

        public TagHandler(ILogger<TagHandler> logger, IGoogleTagManagerServiceWrapper googleTagManagerServiceWrapper)
        {
            _logger = logger;
            _googleTagManagerServiceWrapper = googleTagManagerServiceWrapper;
        }

        public async Task<Tag> CreateTag(rssChannelItem rssChannelItem, Trigger trigger, Workspace workspace)
        {
            _logger.LogInformation($"CreateTag --> Started for item: {rssChannelItem.title}");
            var tagToCreate = new Tag();
            tagToCreate.Name = "rss-" + rssChannelItem.guid.Value; // + DateTime.Now.Ticks;
            tagToCreate.Type = "html";

            var parameterToCreate = new Parameter();
            parameterToCreate.Key = "html";
            parameterToCreate.Type = "1";
            parameterToCreate.Value = GetScriptTagAsString(rssChannelItem);

            tagToCreate.Parameter = new List<Parameter> { parameterToCreate };
            tagToCreate.FiringTriggerId = new List<string> { trigger.TriggerId };
            Tag result;

            try
            {
                result = await _googleTagManagerServiceWrapper.Service.Accounts.Containers.Workspaces.Tags.Create(tagToCreate, workspace.Path).ExecuteAsync();
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }

        private string GetScriptTagAsString(rssChannelItem rssChannelItem)
        {
            var template = $"<html> </html>";
            return template;
        }
    }
}
