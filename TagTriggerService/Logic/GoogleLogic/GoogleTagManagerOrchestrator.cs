using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.TagManager.v2.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.GoogleLogic
{
    public class GoogleTagManagerOrchestrator : IGoogleTagManagerOrchestrator
    {
        private readonly ILogger<GoogleTagManagerOrchestrator> _logger;
        private readonly IGoogleTagManagerServiceWrapper _googleTagManagerServiceWrapper;
        private readonly IWorkspaceAndContainerHandler _workspaceAndContainerHandler;
        private readonly ITriggerHandler _triggerHandler;
        private readonly ITagHandler _tagHandler;

        public GoogleTagManagerOrchestrator(ILogger<GoogleTagManagerOrchestrator> logger,
                                       IGoogleTagManagerServiceWrapper googleTagManagerServiceWrapper,
                                       IWorkspaceAndContainerHandler workspaceAndContainerHandler,
                                       ITriggerHandler triggerHandler, ITagHandler tagHandler)
        {
            _logger = logger;
            _googleTagManagerServiceWrapper = googleTagManagerServiceWrapper;
            _workspaceAndContainerHandler = workspaceAndContainerHandler;
            _triggerHandler = triggerHandler;
            _tagHandler = tagHandler;
        }

        public async Task<string> CreateTagsAndTriggersForItems(IEnumerable<rssChannelItem> rssChannelItems)
        {
            _logger.LogInformation("CreateTagsAndTriggersForItems -> execution started !");

            try
            {
                // Create Triggers and Tags as pairs because they need to be linked 
                var tasks = new List<Task<TagAndTriggerResult>>();
                var i = 0;
                foreach (var rssItem in rssChannelItems)
                {
                    if (_workspaceAndContainerHandler.TagExistsInLiveVersion(rssItem.guid.Value)) // handle duplicates
                    {
                        continue;
                    }
                    // TODO: fix this ! implement exponential backoff with Polly.net
                    // our limit is 15 requests a minute ( or 100 sec ? ) 
                    if (i > 3)
                    {
                        _logger.LogInformation("Starting delay 65 s");
                        await Task.Delay(65000);
                        _logger.LogInformation("Finished delay 65 s");

                        i = 0;
                    }
                    async Task<TagAndTriggerResult> func()
                    {
                        return await CreateTagAndTrigger(rssItem, _workspaceAndContainerHandler.NewWorkspace);
                    }
                    tasks.Add(func());
                    i++;
                }

                var x = await Task.WhenAll(tasks);

                PublishContainerVersionResponse publishResult = await _workspaceAndContainerHandler.PublishContainerVersion();
                return publishResult.ContainerVersion.ContainerVersionId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<TagAndTriggerResult> CreateTagAndTrigger(rssChannelItem rssChannelItem, Workspace workspace)
        {
            var trigger = await _triggerHandler.CreateTrigger(rssChannelItem, workspace);
            var newTag = await _tagHandler.CreateTag(rssChannelItem, trigger, workspace);

            var result = new TagAndTriggerResult { TagResult = newTag, TriggerResult = trigger };
            return result;
        }

        //,   n {
        //     "default":{"key":{"accountId":"**","containerId":"**","triggerId":38,"containerDraftId":38},"data":{"name":"programatically trigger","type":4,"customEventFilter":[{"type":1,"negate":false,"arg":[{"type":4,"listItem":[],"mapKey":[],"mapValue":[],"macroReference":"_event","templateToken":[],"escaping":[]
        //    },{"type":1,"string":"promotion data ready","listItem":[],"mapKey":[],"mapValue":[],"templateToken":[],"escaping":[]
        //      }]}],"filter":[{"type":4,"negate":false,"arg":[{"type":4,"listItem":[],"mapKey":[],"mapValue":[],"macroReference":"Page URL","templateToken":[],"escaping":[]},{ "type":1,"string":"uberfacts.com","listItem":[],"mapKey":[],"mapValue":[],"templateToken":[],"escaping":[]}]}],"autoEventFilter":[]},"statMetadata":{ "createdTime":"1626021614476","modifiedTime":"1626021842743","entityVersion":"1626021842743"},"isBuiltIn":false,"containerDraftMetadata":{ "changeStatus":1},"references":[]}}"
        private async Task<Trigger> CreateCustomTrigger(rssChannelItem rssChannelItem, Workspace workspace)
        {
            var triggerToCreate = new Trigger();
            triggerToCreate.Name = "programatically trigger";
            triggerToCreate.Type = "4";

            // arrange Custom Event Filter
            triggerToCreate.CustomEventFilter = new List<Condition>();
            var eventCondition = new Condition();
            eventCondition.Type = "1";
            eventCondition.Parameter = new List<Parameter>();

            var arg0 = new Parameter();
            arg0.Key = "arg0";
            arg0.Type = "4";
            arg0.List = new List<Parameter>() { new Parameter() { Key = "macroReference", Value = "_event", Type = "1" } };

            //var arg1 = new Parameter();
            //arg1.Key = "arg1";
            //arg1.Type = "1";
            //arg1.List = new List<Parameter>() { new Parameter() { Key = "string", Value = "promotion data ready", Type = "1" } };

            eventCondition.Parameter.Add(arg0);
            //eventCondition.Parameter.Add(arg1);
            triggerToCreate.CustomEventFilter.Add(eventCondition);

            //triggerToCreate.Filter = new List<Condition>();
            var filterCondition = new Condition();
            filterCondition.Type = "4";
            filterCondition.Parameter = new List<Parameter>();

            var filterArg0 = new Parameter();
            filterArg0.Key = "filterArg0";
            filterArg0.Type = "4";
            filterArg0.List = new List<Parameter>() { new Parameter() { Key = "macroReference", Value = "Page URL" } };

            var filterArg1 = new Parameter();
            filterArg1.Key = "filterArg1";
            filterArg1.Type = "1";
            filterArg1.List = new List<Parameter>() { new Parameter() { Key = "string", Value = "uberfacts.com" } };
            filterCondition.Parameter.Add(filterArg0);
            filterCondition.Parameter.Add(filterArg1);
            //triggerToCreate.Filter.Add(filterCondition);

            var newTrigger = await _googleTagManagerServiceWrapper.Service.Accounts.Containers.Workspaces.Triggers.Create(triggerToCreate,
                //new Trigger() 
                //{ 
                //    Name = "test custom event from code", 
                //    Type = "4",
                //    CustomEventFilter = new List<Condition>() 
                //    { 
                //        new Condition() 
                //            { 
                //                Type = "1",  
                //                Parameter = new List<Parameter>() 
                //                    { 
                //                        new Parameter() 
                //                            { Type ="4" }, 
                //                        new Parameter() 
                //                            { Type = "1" } 
                //                } }
                //    },
                //    Filter = new List<Condition>() 
                //    { 
                //        new Condition() 
                //        {
                //            Type = "4",
                //            Parameter = 
                //                new List<Parameter>() 
                //                    { 
                //                        new Parameter() 
                //                            { Type = "4",  Key = "macroReference", Value = "Page URL" } ,
                //                        new Parameter()
                //                            { Type = "1",  Key = "string", Value = "11111uberfacts.com" }
                //                    } 
                //        } 
                //    }    
                //}, 
                workspace.Path).ExecuteAsync();
            return newTrigger;
        }

    }


}
