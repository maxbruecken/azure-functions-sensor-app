using System;

namespace AzureFunction.Core.Models
{
    public class Sensor
    {
        public string Id { get; set; }

        public SensorType Type { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public DateTimeOffset LastSeen { get; set; } = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
