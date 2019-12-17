using System.Threading.Tasks;
using AzureFunctionCore.Models;

namespace AzureFunctionCore.Interfaces
{
    public interface ISensorRepository
    {
        Task<Sensor> GetById(string id);
    }
}
