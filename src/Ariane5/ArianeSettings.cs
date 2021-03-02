using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    public class ArianeSettings
    {
        public ArianeSettings()
        {
            WorkSynchronized = false;
        }
        public bool WorkSynchronized { get; set; }
        public string UniqueTopicNameForTest { get; set; }
        public string DefaultAzureConnectionString { get; set; }
    }
}
