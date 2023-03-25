using System;
// ReSharper disable InconsistentNaming

namespace AzureFunction.Core.Models;

public class AggregatedSensorData
{
    public AggregatedSensorData(Sensor sensor, AggregationType aggregationType)
    {
        Sensor = sensor;
        AggregationType = aggregationType;
    }

    public Sensor Sensor { get; }
    
    public AggregationType AggregationType { get; }

    public DateTimeOffset CreatedAt { get; set; }

    public double Value { get; set; }
}

public enum AggregationType
{
    Min, Max, Mean, StandardDeviation
}