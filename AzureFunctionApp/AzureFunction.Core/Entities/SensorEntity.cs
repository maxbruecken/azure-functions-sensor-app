using System;
using System.Collections.Generic;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Entities;

public class SensorEntity
{
    public long Id { get; set; }

    public string BoxId { get; init; } = null!;

    public SensorType Type { get; init; }

    public SensorInformationEntity Information { get; set; } = new();

    public double Min { get; set; }

    public double Max { get; set; }

    public DateTimeOffset? LastSeen { get; set; }

    public IList<SensorAlarmEntity> Alarms { get; set; } = new List<SensorAlarmEntity>();
}