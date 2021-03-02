using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Tests
{
	public class MyDynamicMessageReader : Ariane.DynamicMessageReaderBase
	{
        public MyDynamicMessageReader(ILogger<MyDynamicMessageReader> logger,
			MessageCollector messageCollector)
        {
			this.Logger = logger;
			this.MessageCollector = messageCollector;
        }

		protected ILogger<MyDynamicMessageReader> Logger { get; }
		protected MessageCollector MessageCollector { get; }

		protected override async Task ProcessAsync(string messageName, dynamic message)
		{
			if (messageName != "test")
			{
				return;
			}

			message.IsProcessed = true;
			MessageCollector.AddPerson(message.Person);
			Logger.LogInformation("dynamic process from queue {0}", FromQueueName);
			await Task.Delay(0);
		}
	}
}
