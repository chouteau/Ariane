﻿using Microsoft.Extensions.Logging;
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
        public R1(ILogger<R1> logger)
        {
            this.Logger = logger;
        }

        protected ILogger Logger { get; }

        public override Task ProcessMessageAsync(User message)
        {
            _count++;
            if (_count % 100 == 0)
            {
                Logger.LogInformation($"{message} {_count}");
            }
            return Task.CompletedTask;
        }
    }
}
