using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Ariane
{
	public class AutoStartBackgroundService : BackgroundService
	{
		public AutoStartBackgroundService(IServiceBus bus)
		{
			this.Bus = bus;
		}

		protected IServiceBus Bus { get; }

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			await Bus.StartReadingAsync();
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await Bus.StopReadingAsync();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.CompletedTask;
		}
	}
}
