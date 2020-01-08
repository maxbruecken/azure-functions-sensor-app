using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Models
{
    public class SensorAlarm : TableEntity
    {
        public SensorAlarm()
        {
            RowKey = Guid.NewGuid().ToString();
        }

        [IgnoreProperty]
        public string SensorId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public DateTimeOffset FiredAt { get; set; } = DateTimeOffset.UtcNow;

        [IgnoreProperty]
        public AlarmStatus Status { get; set; }

        public string StatusString
        {
            get => Status.ToString();
            set => Status = (AlarmStatus)Enum.Parse(typeof(AlarmStatus), value);
        }
    }

    public enum AlarmStatus
    {
        InvalidData, Dead, Closed
    }
}
