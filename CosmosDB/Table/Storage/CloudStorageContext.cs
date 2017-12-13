using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.CosmosDB;
using Microsoft.Azure.CosmosDB.Table;
using Microsoft.Azure.Storage.RetryPolicies;

namespace Table.Data
{
    public class CloudStorageContext : ICloudStorageContext
    {
        private readonly string _connectionString;
        private CloudStorageAccount _cloudStorageAccount;

        public CloudStorageContext(string connectionString)
        {
            _connectionString = connectionString;
            Connect();
        }

        private void Connect()
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(_connectionString);
        }

        public async Task<CloudTable> CreateTable(string tableName)
        {
            var policy = new TableConnectionPolicy()
            {
                MaxRetryWaitTimeInSeconds = 5,
                MaxRetryAttemptsOnThrottledRequests = 100,
                EnableEndpointDiscovery = true
            };
            var tableClient = _cloudStorageAccount.CreateCloudTableClient(policy, ConsistencyLevel.Eventual);
            var table = tableClient.GetTableReference(tableName);
            var requestOption = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(10),
                RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 10)
            };
            
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public TableRequestOptions GetDefaultRequestOption()
        {
            var requestOption = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(10),
                RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 10)
            };
            return requestOption;
        }
    }
}