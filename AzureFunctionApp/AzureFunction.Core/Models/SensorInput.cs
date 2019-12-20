using System.Collections.Generic;

namespace AzureFunction.Core.Models
{
    public class SensorInput
    {
        public string SensorId { get; set; }

        public IEnumerable<double> Values { get; set; }
    }
}
