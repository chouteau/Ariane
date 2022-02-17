using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class AutoStartTests
	{
		[TestMethod]
		public async Task Test_AutoStart()
		{
			var host = Host.CreateDefaultBuilder()
						.ConfigureServices(configure =>
						{
							configure.AddSingleton<MessageCollector>();
							configure.ConfigureAriane(register =>
							{
								register.AddQueue(new QueueSetting()
								{
									Name = "sendreceivetest",
									TypeMedium = typeof(Ariane.InMemoryMedium),
									TypeReader = typeof(PersonMessageReader)
								});
							}, settings =>
							{
								settings.AutoStart = true;
							});
						})
						.Build();

			var sp = host.Services;
			await host.StartAsync();

			var bus = sp.GetRequiredService<Ariane.IServiceBus>();

			var person = new Person();
			var firstName = person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();
			await bus.SendAsync("sendreceivetest", person);

			await Task.Delay(5 * 1000);

			var messageCollector = sp.GetRequiredService<MessageCollector>();
			var syncReceivePeson = messageCollector.GetList().Single();

			Check.That(syncReceivePeson.FirstName).IsEqualTo(firstName);

			await host.StopAsync();
		}
	}
}
