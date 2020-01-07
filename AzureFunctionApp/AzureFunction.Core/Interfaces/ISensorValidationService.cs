using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces
{
    public interface ISensorValidationService
    {
        Task<IEnumerable<SensorAlarm>> ProcessInputAsync(AggregatedSensorData input);
    }
}
