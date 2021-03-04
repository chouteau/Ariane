using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane5.AzureTopicClientWriters
{
    public class User
    {
        public User()
        {
            Name = Guid.NewGuid().ToString();
        }
        public string Name { get; set; }
    }
}
