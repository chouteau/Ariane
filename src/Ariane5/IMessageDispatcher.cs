using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ariane
{
	internal interface IMessageDispatcher : IAsyncDisposable, IDisposable
	{
		string QueueName { get; }
        bool AutoStart { get; set; }
        void AddMessageSubscriberType(Type messageSubscriber);
		void InitializeSubscribers(IServiceProvider serviceProvider);
		void InitializeMedium(IServiceProvider serviceProvider, QueueSetting queueSetting);
		Task StartAsync(bool force = false);
		Task StopAsync(CancellationToken cancellationToken);
	}
}
