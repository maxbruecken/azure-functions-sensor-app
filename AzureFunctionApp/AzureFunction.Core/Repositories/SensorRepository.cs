using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly string _tableName;
        private readonly CloudTableClient _client;

        public SensorRepository(string connectionString, string tableName)
        {
            _tableName = tableName;
            _client = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        }

        public async Task<Sensor> GetById(string id)
        {
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            var tableQuery = new TableQuery<Sensor>().Where(TableQuery.GenerateFilterCondition(nameof(Sensor.PartitionKey), QueryComparisons.Equal, id));
            return (await table.ExecuteQuerySegmentedAsync(tableQuery, null)).FirstOrDefault();
        }

        public async Task Insert(Sensor sensor)
        {
            var insertOperation = TableOperation.Insert(sensor);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(insertOperation);
        }

        public async Task Update(Sensor sensor)
        {
            var mergeOperation = TableOperation.Merge(sensor);
            var table = _client.GetTableReference(_tableName);
            sensor.ETag = "*";
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(mergeOperation);
        }
    }
}
