using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Processor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zhaoxi.DistributedTransaction.Utility
{
    public class CustomDispatcher : Dispatcher
    {
        public CustomDispatcher(ILogger<Dispatcher> logger,
            IMessageSender sender,
            IOptions<CapOptions> options,
            ISubscribeDispatcher executor) : base(logger, sender, options, executor)
        {
            Console.WriteLine("This is CustomDispatcher Invoke");
        }


        public new void EnqueueToPublish(MediumMessage message)
        {
            Console.WriteLine("This is CustomDispatcher Invoke EnqueueToPublish");
            base.EnqueueToPublish(message);
        }

        public new void EnqueueToExecute(MediumMessage message, ConsumerExecutorDescriptor descriptor)
        {
            Console.WriteLine("This is CustomDispatcher Invoke EnqueueToExecute");
            base.EnqueueToExecute(message,descriptor);
        }
    }
}
