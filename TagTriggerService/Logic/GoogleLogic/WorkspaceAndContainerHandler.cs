using Google.Apis.TagManager.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.GoogleLogic
{
    public class WorkspaceAndContainerHandler : IWorkspaceAndContainerHandler
    {
        private readonly IGoogleTagManagerServiceWrapper _googleTagManagerServiceWrapper;
        List<string> _exsistingNames;
        private Workspace _newWorkspace;

        public WorkspaceAndContainerHandler(IGoogleTagManagerServiceWrapper googleTagManagerServiceWrapper)
        {
            _googleTagManagerServiceWrapper = googleTagManagerServiceWrapper;
        }

        public bool TagExistsInLiveVersion(string tagGuid)
        {
            if (_exsistingNames == null)
            {
                GetExistingTagNamesInLiveVersion();
            }
            return _exsistingNames.Any(x => x.Contains("rss-" + tagGuid));
        }

        public async Task<PublishContainerVersionResponse> PublishContainerVersion()
        {
            // Create a container version from the changes ( tag and trigger creation)
            var containerVersionResponse = await _googleTagManagerServiceWrapper.Service.Accounts.Containers.Workspaces.CreateVersion(
                new CreateContainerVersionRequestVersionOptions()
                { Name = $"version for {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}" }, this.NewWorkspace.Path).ExecuteAsync();

            // publish the version
            var publishResult = await _googleTagManagerServiceWrapper.Service.Accounts.Containers.Versions.Publish(containerVersionResponse.ContainerVersion.Path).ExecuteAsync();
            return publishResult;
        }

        private void GetExistingTagNamesInLiveVersion()
        {
            _exsistingNames = new List<string>();
            var liveContainerVersion = _googleTagManagerServiceWrapper.Service.Accounts.Containers.Versions.Live(_googleTagManagerServiceWrapper.AccountAndContainerPath).Execute();

            var tags = liveContainerVersion.Tag;

            if (tags == null) // no tags in current live version
            {
                return;
            }

            foreach (var tag in tags)
            {
                _exsistingNames.Add(tag.Name);
            }
        }

        public Workspace NewWorkspace
        {
            get
            {
                if (_newWorkspace == null)
                {
                    _newWorkspace = CreateNewWorkspace();
                }
                return _newWorkspace;
            }
        }

        private Workspace CreateNewWorkspace()
        {
            return _googleTagManagerServiceWrapper.Service.Accounts.Containers.Workspaces.Create(
                new Workspace()
                { Name = $"Workspace for {DateTime.Now:dddd, dd MMMM yyyy HH- mm-ss}" }
                , _googleTagManagerServiceWrapper.AccountAndContainerPath).Execute();
        }
    }
}
