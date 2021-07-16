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
    public class TriggerHandler : ITriggerHandler
    {
        private readonly ILogger<TriggerHandler> _logger;
        private readonly IGoogleTagManagerServiceWrapper _googleTagManagerServiceWrapper;

        public TriggerHandler(ILogger<TriggerHandler> logger, IGoogleTagManagerServiceWrapper googleTagManagerServiceWrapper)
        {
            _logger = logger;
            _googleTagManagerServiceWrapper = googleTagManagerServiceWrapper;
        }

        public async Task<Trigger> CreateTrigger(rssChannelItem item, Workspace workspace)
        {
            _logger.LogInformation("CreateTrigger --> Started");
            var triggerToCreate = new Trigger();
            triggerToCreate.Name = "rss-" + item.guid.Value; //+ DateTime.Now.Ticks; // TODO: ticks only for debugging for trigger name uniqueness
            triggerToCreate.Type = "1";
            triggerToCreate.Filter = new List<Condition>();

            triggerToCreate.Parameter = new List<Parameter>();

            var filterCondition = new Condition();
            filterCondition.Type = "4";
            filterCondition.Parameter = new List<Parameter>();

            var firstParam = new Parameter() { Type = "4", Value = "Page URL", Key = "arg0" };
            var secondParam = new Parameter() { Type = "1", Value = "uberfacts.com", Key = "arg1" };
            //filterCondition.Parameter.Add(firstParam);
            //filterCondition.Parameter.Add(secondParam);
            //triggerToCreate.Filter.Add(filterCondition);

            triggerToCreate.Parameter.Add(firstParam);
            triggerToCreate.Parameter.Add(secondParam);

            Trigger result;
            try
            {
                result = await _googleTagManagerServiceWrapper.Service.Accounts.Containers.Workspaces.Triggers.Create(triggerToCreate, workspace.Path).ExecuteAsync();
            }
            catch (Exception e)
            {

                throw e;
            }
            return result;
        }
    }
}
