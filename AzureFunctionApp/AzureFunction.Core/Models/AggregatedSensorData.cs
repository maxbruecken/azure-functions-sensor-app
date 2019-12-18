using System;

namespace AzureFunction.Core.Models
{
    public class AggregatedSensorData
    {
        public string SensorId { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public SensorType SensorType { get; set; }

        public AggregationType AggregationType { get; set; }

        public double Value { get; set; }
    }

    public enum AggregationType
    {
        Min, Max, Mean, StandardDeviation
    }
}
