using System;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Entities;

public class SensorAlarmEntity
{
    public long Id { get; set; }
    
    // ReSharper disable once InconsistentNaming
    public long SensorId { get; set; }

    public SensorEntity Sensor { get; set; } = null!;

    public string Identifier { get; set; } = null!;

    public DateTimeOffset FiredAt { get; set; }

    public SensorAlarmStatus Status { get; set; }
}