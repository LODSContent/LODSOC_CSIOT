using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotUtilities
{
    public class IotRegistry
    {
        private RegistryManager manager;
        private string connectionString = string.Empty;
        public IotRegistry(string connectionString)
        {
            manager = RegistryManager.CreateFromConnectionString(connectionString);
            this.connectionString = connectionString;
        }
        public async Task<List<Twin>> GetDevicesAsync()
        {
            
            var query = manager.CreateQuery("SELECT * FROM devices", 100);
            var results = new List<Twin>();
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                results.AddRange(page);

            }
            return results;
        }
        public async Task<List<string>> GetDeviceNames()
        {
            List<string> results = new List<string>();
            foreach(var twin in await GetDevicesAsync())
            {
                results.Add(twin.DeviceId);
            }

            return results;
        }
        public async Task<List<String>> GetTwinsConnectionString()
        {
            var result = new List<string>();
            try
            {
                foreach (var twin in await GetDevicesAsync())
                {
                    Device myDevice = await manager.GetDeviceAsync(twin.DeviceId);
                    var key = myDevice.Authentication.SymmetricKey.PrimaryKey;
                    var iotHub = connectionString.Split(Convert.ToChar(";"))[0].Substring(9);
                    result.Add( $"HostName={iotHub};DeviceId={twin.DeviceId};SharedAccessKey={key}");
                }


            }
            catch (Exception er)
            {
                Console.WriteLine($"Error creating SAS\r{er.Message}");
            }
            return result;
        }
    }
}

