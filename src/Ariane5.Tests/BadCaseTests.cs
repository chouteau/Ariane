using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
    [TestClass]
    public class BadCaseTests
    {
        [TestMethod]
        public async Task Start_Stop_Bus_Manager_With_Unknown_Queue_Name()
        {
            var sp = RootTests.Initialize(services =>
            {
                services.ConfigureAriane();
            });

            var bus = sp.GetRequiredService<IServiceBus>();
            await bus.StartReadingAsync("unknown");
            await bus.StopReadingAsync("unknown");
        }

        [TestMethod]
        public void Send_With_Unknown_Queue()
        {
            var sp = RootTests.Initialize(services =>
            {
                services.ConfigureAriane();
            });

            var bus = sp.GetRequiredService<IServiceBus>();
            var person = Person.CreateTestPerson();
            bus.Send("unknown", person);
        }

        [TestMethod]
        public void Register_Same_Queues()
        {
            var sp = RootTests.Initialize(services =>
            {
                services.ConfigureAriane(register =>
                {
                    register.AddMemoryWriter("same");
                    register.AddMemoryWriter("same");
                });
            });
            var bus = sp.GetRequiredService<IServiceBus>();
            var queueList = bus.GetRegisteredQueueList();

            Check.That(queueList.Count()).IsEqualTo(1);
        }

        [TestMethod]
        public async Task Register_Same_Queues_With_2_Readers_With_Different_MessageType()
        {
            var sp = RootTests.Initialize(services =>
            {
                services.ConfigureAriane(register =>
                {
                    register.AddMemoryReader<PersonMessageReader>("person.queue");
                    register.AddMemoryReader<UserMessageReader>("person.queue");
                });
            });

            Exception badRegister = null;
            try
            {
                var bus = sp.GetRequiredService<IServiceBus>();
                await bus.StartReadingAsync();
            }
            catch(Exception ex)
            {
                badRegister = ex;
            }

            Check.That(badRegister).IsNotNull();
        }

        [TestMethod]
        public async Task Receive_With_Unkwnow_Queue()
        {
            var sp = RootTests.Initialize(services =>
            {
                services.ConfigureAriane();
            });

            var bus = sp.GetRequiredService<IServiceBus>();
            var result = await bus.ReceiveAsync<Person>("unknown", 10, 10);

            Check.That(result).IsNull();
        }


    }
}
