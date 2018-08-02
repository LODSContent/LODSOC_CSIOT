using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading;

namespace DeviceEmulator.Models
{
    public class EventHubContext
    {
        public async Task<EvaluationResult> ReceiveEvents(DeviceWebAPIParameters parms)
        {
            var result = new EvaluationResult { Code = 0, Message = "Received messages", Passed = true };
            try
            {
                var regUtil = new IotUtilities.IotRegistry(parms.IotConnection);
                var deviceNames = await regUtil.GetDeviceNames();
                var storageContainerName = "eventhub2";
                var storageAccount = CloudStorageAccount.Parse(parms.EhubStorage);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(storageContainerName);
                container.CreateIfNotExists();
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference("messages");
                table.CreateIfNotExists();
                var priorRowKeys = getLatestTableRowKeys(table, deviceNames);
                var priorRowKey = priorRowKeys[0];
                var currentRowKey = priorRowKey;
                var eventProcessorHost = new EventProcessorHost(
                    parms.HubName,
                    PartitionReceiver.DefaultConsumerGroupName,
                    parms.EhubConnection,
                    parms.EhubStorage,
                    storageContainerName);
                try
                {
                    await eventProcessorHost.RegisterEventProcessorAsync<EventHubProcessor>();

                    var start = DateTime.Now;
                    var currentTime = DateTime.Now;
                    //Wait for the first table entry for 30 seconds.
                    while ((priorRowKey == currentRowKey) && ((currentTime - start).TotalSeconds < 30))
                    {
                        Thread.Sleep(1000);
                        currentTime = DateTime.Now;
                        currentRowKey = getLatestTableRowKeys(table, deviceNames)[0];
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
                        if (parms.EventReceiveDelay>10) { parms.EventReceiveDelay = 10; }
                        if (parms.EventReceiveDelay > 0) { Thread.Sleep(parms.EventReceiveDelay*1000); }
                        List<DeviceReadingEntity> data = new List<DeviceReadingEntity>();
                        for (int i = 0; i < deviceNames.Count; i++)
                        {
                            string deviceName = deviceNames[i];
                            long prk = priorRowKeys[i];
                            var query = new TableQuery<DeviceReadingEntity>();
                            query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, deviceName));
                            query.TakeCount = 5;
                            data.AddRange(table.ExecuteQuery(query));
                        }
                        result.Data = new List<object>(data);


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

        private List<long> getLatestTableRowKeys(CloudTable table, List<string> partitionKeys)
        {
            List<long> results = new List<long>();
            foreach (string partitionKey in partitionKeys)
            {
                var query = new TableQuery<TableEntity>();

                var firstRow = table.ExecuteQuery(query).FirstOrDefault();
                query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                results.Add(firstRow != null ? long.Parse(firstRow.RowKey) : long.MaxValue);

            }

            return results;

        }


    }
}