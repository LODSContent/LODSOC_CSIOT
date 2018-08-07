using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace iotWebApp.Models
{
    public class DeviceReadingEntity : TableEntity
    {
        public DeviceReadingEntity() : base()
        {
            this.RowKey = Guid.NewGuid().ToString();
        }
        public DeviceReadingEntity(string data)
        {
             var reading = JsonConvert.DeserializeObject<DeviceReading>(data);
            this.RowKey = reading.ReadingID.ToString();
            this.Time = reading.Time;
            this.DeviceID = reading.DeviceID;
            this.Reading = reading.Reading;
            this.AverageReading = reading.AverageReading;
        }
        public DeviceReadingEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
        public DateTime Time { get; set; }
        public string DeviceID { get => this.PartitionKey; set => this.PartitionKey = value; }
        public double Reading { get; set; }
        public double AverageReading { get; set; }


    }
}