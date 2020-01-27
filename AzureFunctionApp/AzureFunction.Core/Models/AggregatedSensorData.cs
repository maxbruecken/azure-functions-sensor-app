using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunction.Core.Models
{
    public class AggregatedSensorData : TableEntity
    {
        public AggregatedSensorData()
        {
            RowKey = Guid.NewGuid().ToString();
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
            set => SensorType = (SensorType) Enum.Parse(typeof(SensorType), value, true);
        }

        [IgnoreProperty]
        public AggregationType AggregationType { get; set; }

        public string AggregationTypeString
        {
            get => $"{AggregationType:G}";
            set => AggregationType = (AggregationType) Enum.Parse(typeof(AggregationType), value, true);
        }

        [IgnoreProperty]
        public DateTimeOffset CreatedAt
        {
            get => Timestamp;
            set => Timestamp = value;
        }

        public double Value { get; set; }
    }

    public enum AggregationType
    {
        Min, Max, Mean, StandardDeviation
    }
}
