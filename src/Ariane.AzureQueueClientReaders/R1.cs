using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane5.AzureQueueClientReaders
{
    public class R1 : Ariane.MessageReaderBase<User>
    {
        int _count = 0;
        int messageId = 0;

        public R1(ILogger<R1> logger)
        {
            this.Logger = logger;
        }

        protected ILogger Logger { get; }

        public override async Task ProcessMessageAsync(User message)
        {
            if (message.MessageId <= messageId)
			{
                Logger.LogWarning("Message not ordered");
			}
            messageId = message.MessageId;
            await Task.Delay(1 * 1000);
            _count++;
            Logger.LogInformation($"{message} {message.MessageId} {_count}");
        }

		public override void Elapsed()
		{
            Logger.LogInformation("Elaped");
		}
	}
}
