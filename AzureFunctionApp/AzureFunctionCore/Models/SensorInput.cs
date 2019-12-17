using System.Collections.Generic;

namespace AzureFunctionCore.Models
{
    public class SensorInput
    {
        public string SensorId { get; set; }

        public IEnumerable<double> Values { get; set; }
    }
}
