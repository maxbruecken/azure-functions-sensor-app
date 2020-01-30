using System.Collections.Generic;
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

        public async Task<Sensor> GetByBoxIdAndType(string boxId, SensorType type)
        {
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            var tableQuery = new TableQuery<Sensor>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(Sensor.PartitionKey), QueryComparisons.Equal, boxId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(Sensor.RowKey), QueryComparisons.Equal, $"{type:G}")
                ));
            return (await table.ExecuteQuerySegmentedAsync(tableQuery, null)).FirstOrDefault();
        }

        public async Task<IEnumerable<Sensor>> GetAll()
        {
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            var tableQuery = new TableQuery<Sensor>();
            var sensors = new List<Sensor>();
            var segment = await table.ExecuteQuerySegmentedAsync(tableQuery, null);
            var continuationToken = segment.ContinuationToken;
            do
            {
                sensors.AddRange(segment);
                if (continuationToken == null) break;
                segment = await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                continuationToken = segment.ContinuationToken;
            } while (true);
            return sensors;
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
