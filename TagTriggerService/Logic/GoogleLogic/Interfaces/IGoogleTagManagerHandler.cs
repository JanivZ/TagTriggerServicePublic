using System.Collections.Generic;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.GoogleLogic
{
    public interface IGoogleTagManagerOrchestrator
    {
        Task<string> CreateTagsAndTriggersForItems(IEnumerable<rssChannelItem> rssChannelItems);
    }
}