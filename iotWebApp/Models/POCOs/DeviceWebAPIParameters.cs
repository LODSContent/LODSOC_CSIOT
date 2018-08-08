using Microsoft.Extensions.Configuration;

namespace iotWebApp.Models
{
    public class DeviceWebAPIParameters
    {
        public string IotConnection { get; set; }
        public string EhubConnection { get; set; }
        public string EhubStorage { get; set; }
        public string CosmosDBConnection { get; set; }
        public int Interval { get; set; }
        public int Iterations { get; set; }
        public string HubName { get; set; }
        public int EventReceiveDelay { get; set; }
        public void Fix(IConfiguration Config)
        {
            IotConnection = fixStringProperty(Config["ConnectionStrings:iotHubConnectionString"], IotConnection);
            CosmosDBConnection = fixStringProperty(Config["ConnectionStrings:CosmosDBConnection"], CosmosDBConnection);
            EhubConnection = fixStringProperty(Config["ConnectionStrings:eventHubConnectionString"], EhubConnection);
            EhubStorage = fixStringProperty(Config["ConnectionStrings:eventHubStorage"], EhubStorage);
            Interval = fixIntProperty(Interval, Config["interval"]);
            Iterations = fixIntProperty(Iterations, Config["interval"]);
            HubName = IotConnection.Split(';')[0].Split('=')[1].Split('.')[0];
            EventReceiveDelay = fixIntProperty(EventReceiveDelay, Config["eventReceiveDelay"]);
        }
        private string fixStringProperty(string replace, string prop)
        {
            return (string.IsNullOrEmpty(prop) || (prop == "-1")) ? replace : prop;
        }
        private int fixIntProperty(int prop, string replace)
        {
            return ((prop == 0) || (prop == -1)) ? int.Parse(replace) : prop;
        }

    }
}
