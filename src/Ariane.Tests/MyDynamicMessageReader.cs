using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public class MyDynamicMessageReader : Ariane.DynamicMessageReaderBase
	{
		protected override void Process(string messageName, dynamic message)
		{
			if (messageName != "test")
			{
				return;
			}

			message.IsProcessed = true;
			StaticContainer.Model = message;
			GlobalConfiguration.Configuration.Logger.Info("dynamic process from queue {0}", FromQueueName);
		}
	}
}
