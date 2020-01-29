using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Models
{
    public class Sensor : TableEntity
    {
        public Sensor()
        {
        }
        
        public Sensor(string boxId, SensorType type)
        {
            BoxId = boxId;
            Type = type;
        }

        [IgnoreProperty]
        public string BoxId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        [IgnoreProperty]
        public SensorType Type
        {
            get => (SensorType) Enum.Parse(typeof(SensorType), RowKey, true);
            set => RowKey = $"{value:G}";
        }

        public double Min { get; set; }

        public double Max { get; set; }

        public DateTimeOffset LastSeen { get; set; } = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
