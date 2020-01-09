using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Models
{
    public class Sensor : TableEntity
    {
        public Sensor()
        {
            RowKey = string.Empty;
        }

        [IgnoreProperty]
        public string Id
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        [IgnoreProperty]
        public SensorType Type { get; set; }

        public string SensorTypeString
        {
            get => $"{Type:G}";
            set => Type = (SensorType) Enum.Parse(typeof(SensorType), value, true);
        }

        public double Min { get; set; }

        public double Max { get; set; }

        public DateTimeOffset LastSeen { get; set; } = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
