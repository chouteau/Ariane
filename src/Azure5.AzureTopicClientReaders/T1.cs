using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane5.AzureTopicClientReaders
{
    public class T1 : Ariane.MessageReaderBase<User>
    {
        int _count = 0;
        public T1(ILogger<T1> logger)
        {
            this.Logger = logger;
        }

        protected ILogger Logger { get; }

        public override Task ProcessMessageAsync(User message)
        {
            _count++;
            if (_count % 10 == 0)
            {
                Logger.LogInformation($"{message} {_count}");
            }
            return Task.CompletedTask;
        }
    }
}
