using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HeimdallUpdate.Models
{
    internal class UpdateConfig
    {
        [JsonConstructor]
        public UpdateConfig() { }

        public DateTime lastUpdated { get; set; }
        public bool ignoreMinorUpdates { get; set; }
        public bool ignoreMajorUpdates { get; set; }

        public bool ignorePreRequestWarnings { get; set; }

    }
}
