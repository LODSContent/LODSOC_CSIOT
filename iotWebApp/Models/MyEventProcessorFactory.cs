using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iotWebApp.Models
{
    public class MyEventProcessorFactory : IEventProcessorFactory
    {
        private StorageContext storageContext;
        
        public MyEventProcessorFactory(string storageConnection, string tableName, string containerName)
        {
            storageContext = new StorageContext(storageConnection, tableName, containerName);

        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new EventHubProcessor(storageContext);
        }
    }
}
