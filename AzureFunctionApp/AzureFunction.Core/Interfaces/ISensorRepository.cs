using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorRepository
{
    Task<Sensor?> GetByBoxIdAndTypeAsync(string id, SensorType type, bool includeAlarms = false);
    Task InsertAsync(Sensor sensor);
    Task UpdateAsync(Sensor sensor);
    Task<IReadOnlyCollection<Sensor>> GetAllAsync(bool includeAlarms = false);
}