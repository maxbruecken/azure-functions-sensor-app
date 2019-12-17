using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionCore.Interfaces;
using AzureFunctionCore.Models;
using COP.Cloud.Azure.Core.Models;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] SensorInput input,
            [ServiceBus("aggregated-sensor-data", Connection = "AzureServiceBus")] IAsyncCollector<AggregatedSensorData> output,
            CancellationToken cancellationToken)
        {
            var aggregatedSensorData = await _inputService.ProcessInputAsync(input);
            foreach (var sensorData in aggregatedSensorData)
            {
                await output.AddAsync(sensorData, cancellationToken);
            }
            await output.FlushAsync(cancellationToken);

            return new OkResult();
        }
    }
}
