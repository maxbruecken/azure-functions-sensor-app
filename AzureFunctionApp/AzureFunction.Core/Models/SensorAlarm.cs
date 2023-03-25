using System;

namespace AzureFunction.Core.Models;

public class SensorAlarm
{
    public SensorAlarm(Sensor sensor) : this(sensor, Guid.NewGuid().ToString())
    {
    }
    
    internal SensorAlarm(Sensor sensor, string identifier)
    {
        Sensor = sensor;
        Identifier = identifier;
        FiredAt = DateTimeOffset.UtcNow;;
    }
    
    public Sensor Sensor { get; }

    public string Identifier { get; }

    public DateTimeOffset FiredAt { get; set; }

    public SensorAlarmStatus Status { get; set; }
}

public enum SensorAlarmStatus
{
    InvalidData, Dead, Closed
}