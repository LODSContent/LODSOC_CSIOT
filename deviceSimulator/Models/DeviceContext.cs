using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace deviceSimulator.Models
{
    public class DeviceContext
    {
        private static TransportType s_transportType = TransportType.Amqp;
        private static string[] ids = { "{95309F0D-8206-4332-8484-A9B841849B4F}", "{C000E49A-C4C4-4E68-AB33-38D69A37F71A}", "{78A1432D-E32F-46E8-960A-F91AED72EDB7}" };
        private static string[] names = { "Building001:RoomA", "Building105:RoomD", "Vehicle321:RoomA" };
        private static double[] initialReadings = { 75.6, 68.3, 82.2 };

        public void GetReadings(string[] connectionStrings, int interval, int iterations)
        {
            if (connectionStrings.Length != 3)
            {
                throw new IndexOutOfRangeException("Incorrect number of connection strings.  There should be three.");
            }
            List<Task> tasks = new List<Task>();
            var devices = generateDevices(connectionStrings, interval);
            for (int i = 0; i < 3; i++)
            {
                var client = DeviceClient.CreateFromConnectionString(devices[i].Connection);
                if (client == null)
                {
                    throw new ArgumentException("Failed to create a device client");
                }
                tasks.Add(new Task(async () =>
                {
                    for (int r = 0; r < iterations; r++)
                    {
                        Thread.Sleep(50);
                        var readings = JsonConvert.SerializeObject(devices[i].GetReading());
                        Message eventMessage = new Message(Encoding.UTF8.GetBytes(readings));
                        await client.SendEventAsync(eventMessage).ConfigureAwait(false);
                    }
                }));
            }
            foreach(var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());
        }

        private List<Device> generateDevices(string[] connectionStrings, int interval)
        {
            var result = new List<Device>();
            var rand = new Random();
            for (int i = 0; i < 3; i++)
            {
                result.Add(new Device
                {
                    Connection = connectionStrings[i],
                    CurrentTime = DateTime.Now,
                    DeviceID = ids[i],
                    DeviceName = names[i],
                    Firmware = "1.1.1.1",
                    Interval = interval,
                    Range = rand.Next(10) + 3,
                    Reading = initialReadings[i],
                    StartUp = DateTime.Now.AddSeconds(-rand.Next(600))
                });
            }
            return result;
        }
    }
}
