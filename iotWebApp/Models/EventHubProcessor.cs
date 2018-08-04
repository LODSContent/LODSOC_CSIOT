using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

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
        private CloudTable _table;
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
                        await storageContext.LoadTableData(new DeviceReadingEntity[] { entity });
                        MessageCount++;
                    }

                }
            });
            t.Wait();
            return context.CheckpointAsync();

        }
    }
    public class DeviceReadingEntity : TableEntity
    {
        public DeviceReadingEntity() : base()
        {
            this.RowKey = Guid.NewGuid().ToString();
        }
        public DeviceReadingEntity(string data)
        {
            var reading = JsonConvert.DeserializeObject<DeviceReading>(data);
            this.RowKey = (DateTime.MaxValue - DateTime.Now).Ticks.ToString();
            this.Time = reading.Time;
            this.DeviceID = reading.DeviceID;
            this.Reading = reading.Reading;
        }
        public DeviceReadingEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
        public DateTime Time { get; set; }
        public string DeviceID { get => this.PartitionKey; set => this.PartitionKey = value; }
        public double Reading { get; set; }

    }
}