using Google.Apis.TagManager.v2.Data;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.GoogleLogic
{
    public interface ITriggerHandler
    {
        Task<Trigger> CreateTrigger(rssChannelItem item, Workspace workspace);
    }
}