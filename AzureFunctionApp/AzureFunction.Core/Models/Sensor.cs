using System;

namespace AzureFunction.Core.Models
{
    public class Sensor
    {
        public Sensor()
        {
        }
        
        public Sensor(string boxId, SensorType type)
        {
            BoxId = boxId;
            Type = type;
        }

        public string BoxId { get; set; }

        public SensorType Type { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public DateTimeOffset LastSeen { get; set; } = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
