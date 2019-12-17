using System.Threading.Tasks;
using AzureFunctionCore.Interfaces;
using AzureFunctionCore.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunctionCore.Services
{
    public class SensorInputService : ISensorInputService
    {
        private readonly ILogger<ISensorInputService> _logger;

        public SensorInputService(ILogger<ISensorInputService> logger)
        {
            _logger = logger;
        }

        public Task ProcessInputAsync(SensorInput input)
        {
            _logger.LogInformation($"Process input {input.Data}");
            return Task.CompletedTask;
        }
    }
}
