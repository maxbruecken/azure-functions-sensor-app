using System;
using System.Collections.Generic;

namespace AzureFunction.Core.Models;

public class Sensor
{
    public Sensor(string boxId, SensorType type)
    {
        BoxId = boxId;
        Type = type;
    }

    public string BoxId { get; }

    public SensorType Type { get; }

    public SensorInformation Information { get; set; } = new(string.Empty, string.Empty, string.Empty);

    public double Min { get; init; }

    public double Max { get; init; }

    public DateTimeOffset LastSeen { get; set; }

    public IReadOnlyCollection<SensorAlarm> Alarms { get; set; } = Array.Empty<SensorAlarm>();
}