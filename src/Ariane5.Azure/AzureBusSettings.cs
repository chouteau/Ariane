using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    public class AzureBusSettings
    {
        public AzureBusSettings()
        {
            ReceiverPrefetchCount = 0;
            ProcessorPrefetchCount = 0;
            DefaultMessageTimeToLiveInDays = 1;
            AutoDeleteOnIdleInDays = 7;
            TransportType = ServiceBusTransportType.AmqpTcp;
        }
        public int ReceiverPrefetchCount { get; set; }
        public int ProcessorPrefetchCount { get; set; }
        public int DefaultMessageTimeToLiveInDays { get; set; }
        public int AutoDeleteOnIdleInDays { get; set; }
        public ServiceBusTransportType TransportType { get; set; }
    }
}
