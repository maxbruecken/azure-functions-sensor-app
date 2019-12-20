using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        public Task<Sensor> GetById(string id)
        {
            return Task.FromResult(new Sensor {Id = id, Type = SensorType.Temperature});
        }
    }
}
