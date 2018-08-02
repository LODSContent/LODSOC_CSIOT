using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Security;
using IotUtilities;

namespace Playground
{
    class Program
    {
        private static string connectionString = "HostName=tswiot.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=tQVRjH9LyHMX9IpeTGCHK0Fe3jVt5rcdVs8RG+uKFhU=";
        static void Main(string[] args)
        {
            GetDevicesAsync().Wait();
            Console.Write("Done. Click enter to close");
            Console.ReadLine();
        }

        private static async Task GetDevicesAsync()
        {
            var regUtil = new IotRegistry(connectionString);
            foreach(string conn in await regUtil.GetTwinsConnectionString())
            {
                Console.WriteLine(conn);
            }
            //var manager = RegistryManager.CreateFromConnectionString(connectionString);
            //var query = manager.CreateQuery("SELECT * FROM devices", 100);
            //while (query.HasMoreResults)
            //{
            //    var page = await query.GetNextAsTwinAsync();
            //    foreach (var twin in page)
            //    {
            //        Console.WriteLine(await getSASToken(manager, twin.DeviceId, 1));
            //    }
            //}
        }

        private static async Task<String> getSASToken(RegistryManager manager, string deviceId,  int ttl = 1)
        {
            string result = null;
            try
            {
                Device myDevice = await manager.GetDeviceAsync(deviceId);
                var key = myDevice.Authentication.SymmetricKey.PrimaryKey;
                var iotHub = connectionString.Split(Convert.ToChar(";"))[0].Substring(9);
                result = $"HostName={iotHub};DeviceId={deviceId};SharedAccessKey={key}";

              
            }
            catch (Exception er)
            {
                Console.WriteLine($"Error creating SAS\r{er.Message}");
            }
            return result;
        }
    }
}
