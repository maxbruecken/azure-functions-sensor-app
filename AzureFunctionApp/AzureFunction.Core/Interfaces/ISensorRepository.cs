using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces
{
    public interface ISensorRepository
    {
        Task<Sensor> GetByBoxIdAndType(string id, SensorType type);
        Task Insert(Sensor sensor);
        Task Update(Sensor sensor);
        Task<IEnumerable<Sensor>> GetAll();
    }
}
