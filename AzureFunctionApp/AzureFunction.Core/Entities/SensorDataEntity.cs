using System;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Entities;

public class SensorDataEntity
{
    public long Id { get; set; }
    
    // ReSharper disable once InconsistentNaming
    public long SensorId { get; set; }

    public SensorEntity Sensor { get; set; } = null!;

    public AggregationType AggregationType { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public double Value { get; set; }
}