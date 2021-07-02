using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Models
{
    public class SensorAlarm : TableEntity
    {
        public SensorAlarm()
        {
            RowKey = Guid.NewGuid().ToString();
            FiredAt = DateTimeOffset.UtcNow;
        }

        [IgnoreProperty]
        public string SensorBoxId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        [IgnoreProperty]
        public SensorType SensorType { get; set; }
        
        public string SensorTypeString
        {
            get => $"{SensorType:G}";
            set => SensorType = (SensorType)Enum.Parse(typeof(SensorType), value);
        }

        public DateTimeOffset FiredAt
        {
            get => Timestamp;
            set => Timestamp = value;
        }

        [IgnoreProperty]
        public AlarmStatus Status { get; set; }

        public string StatusString
        {
            get => $"{Status:G}";
            set => Status = (AlarmStatus)Enum.Parse(typeof(AlarmStatus), value);
        }
    }

    public enum AlarmStatus
    {
        InvalidData, Dead, Closed
    }
}
