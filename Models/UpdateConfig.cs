using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeimdallUpdate.Models
{
    internal class UpdateConfig
    {
        public string lastUpdated { get; set; }
        public bool ignoreMinorUpdates { get; set; }
        public bool ignoreMajorUpdates { get; set; }

    }
}
