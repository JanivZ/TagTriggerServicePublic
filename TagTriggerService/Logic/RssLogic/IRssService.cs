using System.Collections.Generic;
using System.Threading.Tasks;
using TagTriggerService.Model;

namespace TagTriggerService.Logic.RssLogic
{
    public interface IRssService
    {
        Task<rss> GetRssItems();
    }
}