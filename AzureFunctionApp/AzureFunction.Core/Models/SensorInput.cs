using System;
using System.Collections.Generic;

namespace AzureFunction.Core.Models
{
    public class SensorInput
    {
        public string SensorBoxId { get; set; }
        
        public DateTimeOffset Timestamp { get; set; }

        public IEnumerable<SensorData> Data { get; set; }
    }

    public class SensorData
    {
        public SensorType Type { get; set; }
        
        public IEnumerable<double> Values { get; set; }
    }
}
