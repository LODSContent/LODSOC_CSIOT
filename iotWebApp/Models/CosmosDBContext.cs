using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using Microsoft.Azure.Documents;

namespace iotWebApp.Models
{
    public class CosmosDBContext
    {
        private DeviceWebAPIParameters parms;
        DocumentClient client;
        public CosmosDBContext(DeviceWebAPIParameters parms)
        {
            this.parms = parms;
            var uriClause = parms.CosmosDBConnection.Split(';')[0];
            var keyClause = parms.CosmosDBConnection.Split(';')[1];
            var eqPos = keyClause.IndexOf('=');
            var key  = keyClause.Substring(eqPos + 1, keyClause.Length - eqPos - 1);
            string uri = uriClause.Split('=')[1];
            client = new DocumentClient(new Uri(uri), key);
        }

        public EvaluationResult TestDocuments(string databaseName, string collectionName)
        {
            var result = new EvaluationResult { Code = 0, Message = "Received messages", Passed = true };
            try
            {
                var spec = new SqlQuerySpec("SELECT TOP 10 c.DeviceID, c.Time, c.Reading FROM c");
                IQueryable<DeviceReadingEntity> query = client.CreateDocumentQuery<DeviceReadingEntity>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),spec);
                var rows = query.ToList();
                result.Passed = rows.Count > 0;
                result.Code = result.Passed ? rows.Count : -1;
                result.Message = result.Passed ? "Retrieved readings from Cosmos DB" : "Did not retrieve any readings from Cosmos DB";
                result.Data = rows;
            }
            catch(Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

  


    }
}