using System.Threading.Tasks;
using AzureFunctionCore.Interfaces;
using AzureFunctionCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionApp
{
    public class SensorInputFunction
    {
        private readonly ISensorInputService _inputService;
        private readonly ILogger<SensorInputFunction> _logger;

        public SensorInputFunction(ISensorInputService inputService, ILogger<SensorInputFunction> logger)
        {
            _inputService = inputService;
            _logger = logger;
        }

        [FunctionName("SensorInput")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] SensorInput input)
        {
            await _inputService.ProcessInputAsync(input);

            return new OkResult();
        }
    }
}
