using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading;

namespace iotWebApp.Models
{
    public class StorageContext
    {
        private CloudBlobContainer container;
        private CloudTable table;
        private CloudStorageAccount account;
        public StorageContext(string connectionString, string tableName, string containerName)
        {
            account = CloudStorageAccount.Parse(connectionString);
            if (!string.IsNullOrEmpty(containerName))
            {
                var blobClient = account.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(containerName);
                Task.Run(async () =>
                {
                    try
                    {
                        await container.CreateIfNotExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError($"Error creating container:\r {ex.ToString()}");
                    }
                }).Wait();
            }
            if (!string.IsNullOrEmpty(tableName))
            {
                var tableClient = account.CreateCloudTableClient();
                table = tableClient.GetTableReference(tableName);
                Task.Run(async () => await table.CreateIfNotExistsAsync()).Wait();
            }
        }

        public async Task<EvaluationResult> LoadEventData(DeviceReadingEntity[] data)
        {
            if (data.Length == 0)
            {
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

        public async Task<EvaluationResult> RetrieveProcessedData(int rowCount, List<string> partitionKeys)
        {
            var result = new EvaluationResult { Code = 0, Message = "Returned table data successfully", Passed = true };
            int totalRows = 0;
            var output = new List<DeviceReadingEntity>();
            if (partitionKeys == null)
            {
                var query = new TableQuery<DeviceReadingEntity>();
                query.TakeCount = rowCount;
                var seg = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
                foreach (var row in seg)
                {
                    output.Add(row);
                    totalRows++;
                }


            }
            else
            {
                foreach (var key in partitionKeys)
                {
                    var query = new TableQuery<DeviceReadingEntity>();
                    query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key));
                    query.TakeCount = rowCount;
                    var seg = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
                    foreach (var row in seg)
                    {
                        output.Add(row);
                        totalRows++;
                    }

                }
            }
            result.Data = output;
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

        public async Task<(int Matched, int Unmatched)> CompareContent(string primaryTableName)
        {
            int matched = 0; int unmatched = 0;

            foreach (var row in await RetrieveTableData(primaryTableName, 0))
            {
                var query = new TableQuery<TableEntity>();
                var partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, row.PartitionKey);
                var timeFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row.RowKey);
                query.Where(TableQuery.CombineFilters(partitionFilter, TableOperators.And, timeFilter));
                var queryResult = table.ExecuteQuerySegmentedAsync(query, null);
                var seg = await table.ExecuteQuerySegmentedAsync(query, default(TableContinuationToken));
                if (seg.Count() > 0) { matched++; } else { unmatched++; }

            }

            return (matched, unmatched);
        }

        public async Task<List<DeviceReadingEntity>> RetrieveTableData(string TableName, int rowCount)
        {
            var pClient = account.CreateCloudTableClient();
            var pTable = pClient.GetTableReference(TableName);
            var pQuery = new TableQuery<DeviceReadingEntity>();
            if (rowCount > 0) pQuery.TakeCount = rowCount;
            var rows = await table.ExecuteQuerySegmentedAsync(pQuery, default(TableContinuationToken));
            return rows.Results;


        }

        public async Task<EvaluationResult> GetFirstBlob(string containerName, int waitTime)
        {
            var result = new EvaluationResult { Passed = true, Message = "Passed", Code = 0 };
            try
            {
                var startTime = DateTime.Now;
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference(containerName);
                if (!await container.ExistsAsync())
                {
                    result.Code = -1;
                    result.Passed = false;
                    result.Message = $"Blob container {container} does not exist.";

                }
                else
                {
                    var files = await container.ListBlobsSegmentedAsync(default(BlobContinuationToken));
                    var count = files.Results.Count();
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    while ((count == 0) && (elapsed < waitTime))
                    {
                        Thread.Sleep(1000);
                        files = container.ListBlobsSegmentedAsync(default(BlobContinuationToken)).GetAwaiter().GetResult();
                        count = files.Results.Count();
                        elapsed = (DateTime.Now - startTime).TotalSeconds;
                    }
                    result.Code = count > 0 ? count : -1;
                    result.Passed = count > 0;
                    result.Message = count > 0 ? "Located the expected blob file" : "Did not locate any blob files";
                }
            }
            catch (Exception ex)
            {
                result.Code = ex.HResult;
                result.Passed = false;
                result.Message = $"Error: {ex.Message}";
            }

            return result;
        }

    }
}
