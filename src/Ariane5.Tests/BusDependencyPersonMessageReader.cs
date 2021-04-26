using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Tests
{
	public class BusDependencyPersonMessageReader : MessageReaderBase<Person>
	{
		public BusDependencyPersonMessageReader(ILogger<PersonMessageReader> logger,
			MessageCollector messageCollector,
			Ariane.IServiceBus bus)
        {
			this.Logger = logger;
			this.MessageCollector = messageCollector;
			this.Bus = bus;
        }

		protected ILogger<PersonMessageReader> Logger { get; }
		protected MessageCollector MessageCollector { get; }
		protected Ariane.IServiceBus Bus { get; }

		public override Task ProcessMessageAsync(Person message)
		{
			Bus.GetRegisteredQueueList();
			message.IsProcessed = true;
			message.FromQueue = this.FromQueueName;
			message.FromSubscription = this.FromSubscriptionName;
			message.FromMessageReader = this.GetType().FullName;

			MessageCollector.AddPerson(message);
			Logger.LogInformation("{0}:{1}:{2}", FromQueueName, System.Threading.Thread.CurrentThread.Name, message.FirstName);
			return Task.CompletedTask;
		}
	}
}
