using Google.Apis.TagManager.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTriggerService.Model
{
    public class TagAndTriggerResult
    {
        public Tag TagResult { get; set; }
        public Trigger TriggerResult { get; set; }
    }
}
