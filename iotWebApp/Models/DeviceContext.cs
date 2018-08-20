using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IotD = Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;

namespace iotWebApp.Models
{
    public class DeviceContext
    {
        private static TransportType s_transportType = TransportType.Amqp;
        private string[] ids = { "{95309F0D-8206-4332-8484-A9B841849B4F}", "{C000E49A-C4C4-4E68-AB33-38D69A37F71A}", "{78A1432D-E32F-46E8-960A-F91AED72EDB7}" };
        private double[] initialReadings = { 23.5, 50.5, 75.5 };

        public async Task<EvaluationResult> GetReadings(DeviceWebAPIParameters parms)
        {
            var result = new EvaluationResult { Code = 0, Message = "Processed telemetry", Passed = true };
            try
            {

                var iotReg = new IotUtilities.IotRegistry(parms.IotConnection);
                var connectionStrings = await iotReg.GetTwinsConnectionString();
                if (connectionStrings.Count != 3)
                {
                    throw new IndexOutOfRangeException("Incorrect number of connection strings.  There should be three.");
                }
                List<Task> tasks = new List<Task>();
                var devices = generateDevices(connectionStrings, parms.Interval);
                for (int i = 0; i < 3; i++)
                {
                    tasks.Add(deviceReadings(devices[i], parms.Iterations));

                }

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<EvaluationResult> ReceiveCommand(DeviceWebAPIParameters parms)
        {
            var result = new EvaluationResult { Code = 0, Message = "Received a command", Passed = true };
            var iotReg = new IotUtilities.IotRegistry(parms.IotConnection);
            var connectionStrings = await iotReg.GetTwinsConnectionString();

            var client = DeviceClient.CreateFromConnectionString(connectionStrings[0], s_transportType);
            if (client == null)
            {
                throw new ArgumentException("Failed to create a device client");
            }

            Message receivedMessage;


            receivedMessage = await client.ReceiveAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            if (receivedMessage != null)
            {
                result.Message = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await client.CompleteAsync(receivedMessage).ConfigureAwait(false);
            }
            else
            {
                result.Message = "Device message receive timed out.  You must add a message within 30 seconds.";
                result.Passed = false;
                result.Code = -1;
            }
            return result;
        }

        public async Task<EvaluationResult> DeviceTwin(DeviceWebAPIParameters parms)
        {
            var result = new EvaluationResult { Code = 0, Message = "Received a command", Passed = true };
            try
            {
                var iotReg = new IotUtilities.IotRegistry(parms.IotConnection);
                var connectionStrings = await iotReg.GetTwinsConnectionString();

                var client = DeviceClient.CreateFromConnectionString(connectionStrings[0], s_transportType);
                if (client == null)
                {
                    throw new ArgumentException("Failed to create a device client");
                }
                Twin twin = await client.GetTwinAsync().ConfigureAwait(false);
                if (twin.Properties.Desired.Contains("sample"))
                {
                    dynamic prop = twin.Properties.Desired["sample"];
                    result.Message = prop;
                    TwinCollection reportedProperties = new TwinCollection();
                    reportedProperties["sample"] = result.Message;
                    try
                    {
                        await client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
                    }
                    catch
                    {
                        //Ignore error here.  This is a write operation and it fails if the value already exists.
                    }
                }
                else
                {
                    result.Message = $"The \"sample\" property does not exist for the {twin.DeviceId} device.";
                    result.Passed = false;
                    result.Code = -1;
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<EvaluationResult> GetDeviceCount(DeviceWebAPIParameters parms)
        {
            var result = new EvaluationResult { Code = 0, Message = "Processed telemetry", Passed = true };
            try
            {

                var iotReg = new IotUtilities.IotRegistry(parms.IotConnection);
                var deviceCount = (await iotReg.GetTwinsConnectionString()).Count;
                result.Passed = deviceCount == 3;
                result.Message = $"There are {deviceCount} devices registered in IoY Hub";
                result.Code = deviceCount == 3 ? 0 : -1;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        #region Helper Methods
        private async Task deviceReadings(IoTDevice device, int iterations)
        {
            var client = DeviceClient.CreateFromConnectionString(device.Connection, s_transportType);
            if (client == null)
            {
                throw new ArgumentException("Failed to create a device client");
            }
            for (int j = 0; j < iterations; j++)
            {
                Thread.Sleep(50);
                var readings = JsonConvert.SerializeObject(device.GetReading());
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(readings));
                eventMessage.ContentType = "application/json";
                await client.SendEventAsync(eventMessage).ConfigureAwait(false);
            }

        }

        private List<IoTDevice> generateDevices(List<string> connectionStrings, int interval)
        {
            var result = new List<IoTDevice>();
            var rand = new Random();
            for (int i = 0; i < 3; i++)
            {
                var name = connectionStrings[i].Split(';')[1].Split('=')[1];
                result.Add(new IoTDevice
                {
                    Connection = connectionStrings[i],
                    CurrentTime = DateTime.Now,
                    DeviceID = ids[i],
                    DeviceName = name,
                    Firmware = "1.1.1.1",
                    Interval = interval,
                    Range = rand.Next(10) + 3,
                    Reading = initialReadings[i],
                    StartUp = DateTime.Now.AddSeconds(-rand.Next(600))
                });
            }
            return result;
        }


        #endregion
    }


}
