using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Repositories
{
    public class SensorAlarmRepository : ISensorAlarmRepository
    {
        private readonly string _tableName;
        private readonly CloudTableClient _client;

        public SensorAlarmRepository(string connectionString, string tableName)
        {
            _tableName = tableName;
            _client = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        }

        public async Task<SensorAlarm> GetBySensorIdAndStatus(string sensorId, AlarmStatus status)
        {
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            var tableQuery = new TableQuery<SensorAlarm>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(SensorAlarm.PartitionKey), QueryComparisons.Equal, sensorId),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(SensorAlarm.StatusString), QueryComparisons.Equal, $"{status:G}")
                ));
            return (await table.ExecuteQuerySegmentedAsync(tableQuery, null)).FirstOrDefault();
        }

        public async Task Insert(SensorAlarm alarm)
        {
            var insertOperation = TableOperation.Insert(alarm);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(insertOperation);
        }

        public async Task Update(SensorAlarm alarm)
        {
            var mergeOperation = TableOperation.Merge(alarm);
            var table = _client.GetTableReference(_tableName);
            alarm.ETag = "*";
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(mergeOperation);
        }

        public async Task Delete(SensorAlarm sensorAlarm)
        {
            var deleteOperation = TableOperation.Delete(sensorAlarm);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(deleteOperation);
        }
    }
}
