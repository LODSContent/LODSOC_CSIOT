using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace iotWebApp.Models
{
    public class EventHubContext
    {
        private StorageContext storage;
        private string tableName = "defaultreadings";
        private string containerName = "eventhub2";
        private DeviceWebAPIParameters parms;
        public EventHubContext(DeviceWebAPIParameters parms)
        {
            this.parms = parms;
            storage = new StorageContext(parms.EhubStorage, tableName, containerName);
        }
        public EventHubContext(DeviceWebAPIParameters parms, string tableName, string containerName)
        {
            this.tableName = tableName;
            this.containerName = containerName;
            this.parms = parms;
            storage = new StorageContext(parms.EhubStorage, tableName, containerName);
        }
        public async Task<EvaluationResult> ReceiveEvents(string consumerGroup = null)
        {
            var result = new EvaluationResult { Code = 0, Message = "Received messages", Passed = true };
            try
            {
                consumerGroup = consumerGroup ?? PartitionReceiver.DefaultConsumerGroupName;
                var regUtil = new IotUtilities.IotRegistry(parms.IotConnection);
                var deviceNames = await regUtil.GetDeviceNames();
                var priorRowKeys = await storage.RetrieveLastKeys(deviceNames);
                var priorRowKey = priorRowKeys[0];
                var currentRowKey = priorRowKey;
                var eventProcessorHost = new EventProcessorHost(
                    parms.HubName,
                   consumerGroup,
                    parms.EhubConnection,
                    parms.EhubStorage,
                    this.containerName);
                try
                {
                    //await eventProcessorHost.RegisterEventProcessorAsync<EventHubProcessor>();
                    await eventProcessorHost.RegisterEventProcessorFactoryAsync(new MyEventProcessorFactory(parms.EhubStorage, tableName, containerName));
                    var start = DateTime.Now;
                    var currentTime = DateTime.Now;
                    var seconds = (currentTime - start).TotalSeconds;
                    //Wait for the first table entry for 30 seconds.
                    while ((priorRowKey == currentRowKey) && (seconds < 30))
                    {
                        Thread.Sleep(100);
                        currentTime = DateTime.Now;
                        currentRowKey = (await storage.RetrieveLastKeys(deviceNames))[0];
                        seconds = (currentTime - start).TotalSeconds;
                        System.Diagnostics.Trace.WriteLine($"Seconds: {seconds}\tPrior: {priorRowKey}\tCurrent: {currentRowKey}");
                    }
                    if (currentRowKey == priorRowKey)
                    {
                        //No rows found
                        result.Code = -1;
                        result.Passed = false;
                        result.Message = "No errors occurred, but no events were processed.";

                    }
                    else
                    {
                        //Wait for events to come in.  If synchronous, this will be 0 seconds.  This can be no more than 10 seconds.
                        if (parms.EventReceiveDelay > 10) { parms.EventReceiveDelay = 10; }
                        if (parms.EventReceiveDelay > 0) { Thread.Sleep(parms.EventReceiveDelay * 1000); }
                        List<DeviceReadingEntity> data = new List<DeviceReadingEntity>();

                        data.AddRange((await storage.RetrieveTableData(5, deviceNames)).Data);
                        result.Data = data;
                        result.Message = $"IoT device messages were received and processed into the {tableName} storage table";

                    }
                }
                finally
                {
                    await eventProcessorHost.UnregisterEventProcessorAsync();
                }
            }
            catch (Exception outer)
            {
                result.Passed = false;
                result.Code = outer.HResult;
                result.Message = $"Error: {outer.Message}";
            }
            return result;
        }

        public async Task<EvaluationResult> TestConsumerGroups()
        {
            var result = new EvaluationResult { Code = 0, Passed = true, Message = "Passed" };
            try
            {
                List<Task<EvaluationResult>> tasks = new List<Task<EvaluationResult>>();
                tasks.Add(Task.Run<EvaluationResult>(async () => await (new EventHubContext(parms, "primary", "primary")).ReceiveEvents("primary")));
                tasks.Add(Task.Run<EvaluationResult>(async () => await (new EventHubContext(parms, "secondary", "secondary")).ReceiveEvents("primary")));

                EvaluationResult[] cgResults = { tasks[0].Result, tasks[1].Result };

                try
                {
                    if (cgResults[0].Passed && cgResults[1].Passed)
                    {
                        var testStorage = new StorageContext(parms.EhubStorage, "secondary", null);
                        var testResults = await testStorage.CompareContent("primary");

                        if (testResults.Unmatched>1)
                        {
                             result.Passed = false;
                            result.Code = -1;
                            result.Message = "The consumer groups returned different data.";

                        }
                        if (result.Passed) {
                            result.Message = "Both consumer groups retrieved the same data.";
                            result.Data = new List<DeviceReadingEntity>(cgResults[0].Data);
                        }
                    }
                    else
                    {
                        result.Passed = false;
                        result.Code = -1;
                        result.Message = (cgResults[0].Passed ? "" : $"Primary consumer group failed: {cgResults[0].Message}. ") + (cgResults[1].Passed ? "" : $"Secondary consumer group failed: {cgResults[1].Message}");
                    }
                }
                catch (Exception ex)
                {
                    result.Code = ex.HResult;
                    result.Passed = false;
                    result.Message = $"Data capture on both consumer groups was successful, however an error occurred when comparing the results: {ex.Message}";
                }
            }
            catch (Exception outerEx)
            {
                result.Code = outerEx.HResult;
                result.Passed = false;
                result.Message = $"An error occurred while retrieving data from multiple consumer groups: {outerEx.Message}";

            }

            return result;
        }


    }
}