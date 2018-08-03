using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iotWebApp.Models
{
    public class MyEventProcessorFactory : IEventProcessorFactory
    {
        private string storageConnection;
        public MyEventProcessorFactory(string storageConnection)
        {
            this.storageConnection = storageConnection;

        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new EventHubProcessor(storageConnection);
        }
    }
}
