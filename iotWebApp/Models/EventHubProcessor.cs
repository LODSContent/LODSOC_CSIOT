using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace iotWebApp.Models
{
    public class EventHubProcessor : IEventProcessor
    {

        private StorageContext storageContext;
        public EventHubProcessor() { }
        public EventHubProcessor(StorageContext context)
        {
            this.storageContext = context;
        }
        public int MessageCount { get; set; }


        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {

            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Task t = Task.Run(async () =>
            {
                foreach (var eventData in messages)
                {
                    var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    var entity = new DeviceReadingEntity(data);
                    if (entity.PartitionKey != null && entity.RowKey != null)
                    {
                        await storageContext.LoadEventData(new DeviceReadingEntity[] { entity });
                        MessageCount++;
                    }

                }
            });
            t.Wait();
            return context.CheckpointAsync();

        }
    }

}