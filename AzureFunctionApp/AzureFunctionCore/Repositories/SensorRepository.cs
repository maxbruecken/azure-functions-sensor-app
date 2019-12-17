using System.Threading.Tasks;
using AzureFunctionCore.Interfaces;
using AzureFunctionCore.Models;

namespace AzureFunctionCore.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        public Task<Sensor> GetById(string id)
        {
            return Task.FromResult(new Sensor {Id = id, Type = SensorType.Temperature});
        }
    }
}
