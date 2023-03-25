using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorInputService
{
    IAsyncEnumerable<AggregatedSensorData> ProcessInputAsync(SensorInput input);
}