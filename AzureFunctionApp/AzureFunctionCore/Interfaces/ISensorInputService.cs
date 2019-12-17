using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunctionCore.Models;
using COP.Cloud.Azure.Core.Models;

namespace AzureFunctionCore.Interfaces
{
    public interface ISensorInputService
    {
        Task<IEnumerable<AggregatedSensorData>> ProcessInputAsync(SensorInput input);
    }
}
