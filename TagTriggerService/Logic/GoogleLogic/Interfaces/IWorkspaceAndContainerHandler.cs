using Google.Apis.TagManager.v2.Data;
using System.Threading.Tasks;

namespace TagTriggerService.Logic.GoogleLogic
{
    public interface IWorkspaceAndContainerHandler
    {
        Workspace NewWorkspace { get; }

        Task<PublishContainerVersionResponse> PublishContainerVersion();
        bool TagExistsInLiveVersion(string tagName);
    }
}