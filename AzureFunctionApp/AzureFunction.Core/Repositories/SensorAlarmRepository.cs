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

        public async Task<SensorAlarm> GetBySensorBoxIdAndSensorTypeAndStatusAsync(string sensorBoxId, SensorType sensorType, AlarmStatus status)
        {
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            var tableQuery = new TableQuery<SensorAlarm>().Where(
                TableQuery.CombineFilters(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(SensorAlarm.PartitionKey), QueryComparisons.Equal, sensorBoxId),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(nameof(SensorAlarm.StatusString), QueryComparisons.Equal, $"{status:G}")
                    ),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(SensorAlarm.SensorTypeString), QueryComparisons.Equal, $"{sensorType:G}")
                ));
            return (await table.ExecuteQuerySegmentedAsync(tableQuery, null)).FirstOrDefault();
        }

        public async Task InsertAsync(SensorAlarm alarm)
        {
            var insertOperation = TableOperation.Insert(alarm);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(insertOperation);
        }

        public async Task UpdateAsync(SensorAlarm alarm)
        {
            alarm.ETag = "*";
            var mergeOperation = TableOperation.Merge(alarm);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(mergeOperation);
        }

        public async Task DeleteAsync(SensorAlarm alarm)
        {
            alarm.ETag = "*";
            var deleteOperation = TableOperation.Delete(alarm);
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(deleteOperation);
        }
    }
}
