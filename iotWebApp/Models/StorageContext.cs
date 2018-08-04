using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;


namespace iotWebApp.Models
{
    public class StorageContext
    {
        private CloudBlobContainer container;
        private CloudTable table;
        public StorageContext(string connectionString, string tableName, string containerName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            if (!string.IsNullOrEmpty(containerName))
            {
                var blobClient = account.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(containerName);
                Task.Run(async () =>  await container.CreateIfNotExistsAsync()).Wait();
            }
            if(!string.IsNullOrEmpty(tableName))
            {
                var tableClient = account.CreateCloudTableClient();
                table = tableClient.GetTableReference(tableName);
                Task.Run(async () => await table.CreateIfNotExistsAsync()).Wait();
            }
        }

        public async Task<EvaluationResult> LoadTableData(DeviceReadingEntity[] data)
        {
            if (data.Length == 0) {
                return new EvaluationResult { Code = -1, Message = $"No data to load to {table.Name}", Passed = false };
            }
            var result = new EvaluationResult { Code = 0, Message = "Loaded table data", Passed = true };
            var uploaded = 0;
            try
            {
                List<DeviceReadingEntity> rows = data.OrderBy(d => d.PartitionKey).ToList();
                var batchCount = 0;
                var currentPartition = rows[0].PartitionKey;
                var batch = new TableBatchOperation();
                foreach (var row in rows)
                {
                    if ((row.PartitionKey != currentPartition) || (batchCount > 49))
                    {
                        await table.ExecuteBatchAsync(batch);
                        uploaded += batchCount;
                        batch = new TableBatchOperation();
                        batchCount = 0;
                        currentPartition = row.PartitionKey;
                    }
                    batch.Add(TableOperation.Insert(row));
                    batchCount++;
                }
                if (batchCount > 0)
                {
                    uploaded += batchCount;
                    await table.ExecuteBatchAsync(batch);
                }
                result.Message = $"Successfully uploaded {uploaded} rows to {table.Name}.";
                result.Code = uploaded;
            }
            catch (Exception ex)
            {
                result.Code = ex.HResult;
                result.Passed = false;
                result.Message = $"Encountered an error after uploading {uploaded} rows: {ex.Message}";
            }
            return result;

        }

        public async Task<EvaluationResult> RetrieveTableData(int rowCount, List<string> partitionKeys)
        {
            var result = new EvaluationResult { Code = 0, Message = "Returned table data successfully", Passed = true };
            int totalRows = 0;
            var output = new List<DeviceReadingEntity>();
            if (partitionKeys == null)
            {
                var query = new TableQuery<DeviceReadingEntity>();
                var queryResult = table.ExecuteQuerySegmentedAsync(query, null);
                var seg = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
                foreach (var row in seg.Take(rowCount))
                {
                    output.Add(row);
                    totalRows++;
                }


            } else
            {
                foreach(var key in partitionKeys)
                {
                    var query = new TableQuery<DeviceReadingEntity>();
                    query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key));
                    var queryResult = table.ExecuteQuerySegmentedAsync(query, null);
                    var seg = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
                    foreach (var row in seg.Take(rowCount))
                    {
                        output.Add(row);
                        totalRows++;
                    }

                }
            }
            result.Data =  output;
            result.Message = $"Returned {totalRows} rows from {table.Name}";
            return result;
        }
        public async Task<List<long>> RetrieveLastKeys(List<string> partitionKeys)
        {
            var result = new List<long>();
            foreach (var key in partitionKeys)
            {
                var query = new TableQuery<TableEntity>();
                query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key));
                var queryResult = table.ExecuteQuerySegmentedAsync(query, null);
                var seg = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
                var firstRow = seg.FirstOrDefault();
                result.Add(firstRow != null ? long.Parse(firstRow.RowKey) : long.MaxValue);
            }

            return result;
        }
    }
}
