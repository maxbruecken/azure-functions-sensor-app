using System.Threading.Tasks;
using AzureFunctionCore.Models;

namespace AzureFunctionCore.Interfaces
{
    public interface ISensorInputService
    {
        Task ProcessInputAsync(SensorInput input);
    }
}
