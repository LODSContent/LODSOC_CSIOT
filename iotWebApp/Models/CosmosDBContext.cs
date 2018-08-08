using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Documents.Client;
using System.Linq;

namespace iotWebApp.Models
{
    public class CosmosDBContext
    {
        private DeviceWebAPIParameters parms;
        DocumentClient client;
        public CosmosDBContext(DeviceWebAPIParameters parms)
        {
            this.parms = parms;
            string uri = parms.CosmosDBConnection.Split(';')[0].Split('=')[1];
            string key = parms.CosmosDBConnection.Split(';')[1].Split('=')[1];
            client = new DocumentClient(new Uri(uri), key);
        }

        public EvaluationResult TestDocuments(string databaseName, string collectionName)
        {
            var result = new EvaluationResult { Code = 0, Message = "Received messages", Passed = true };
            try
            {
                IQueryable<DeviceReading> query = client.CreateDocumentQuery<DeviceReading>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
                var rows = query.ToList();
                result.Passed = rows.Count > 0;
                result.Code = result.Passed ? rows.Count : -1;
                result.Message = result.Passed ? "Retrieved readings from Cosmos DB" : "Did not retrieve any readings from Cosmos DB";
            }
            catch(Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<EvaluationResult> TestConsumerGroups()
        {
            var result = new EvaluationResult { Code = 0, Passed = true, Message = "Passed" };

            return result;
        }



    }
}