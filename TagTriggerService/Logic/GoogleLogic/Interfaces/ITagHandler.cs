using Google.Apis.TagManager.v2.Data;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.GoogleLogic
{
    public interface ITagHandler
    {
        Task<Tag> CreateTag(rssChannelItem rssChannelItem, Trigger trigger, Workspace workspace);
    }
}