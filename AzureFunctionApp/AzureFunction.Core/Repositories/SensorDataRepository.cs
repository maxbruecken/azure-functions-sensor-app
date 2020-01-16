using System.Threading.Tasks;
using AzureFunction.Core.Models;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Interfaces
{
    public class SensorDataRepository : ISensorDataRepository
    {
        private readonly string _tableName;
        private readonly CloudTableClient _client;
        
        public SensorDataRepository(string connectionString, string tableName)
        {
            _tableName = tableName;
            _client = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        }

        public async Task InsertAsync(AggregatedSensorData aggregatedSensorData)
        {
            var insertOperation = TableOperation.Insert(aggregatedSensorData);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(insertOperation);
        }
    }
}