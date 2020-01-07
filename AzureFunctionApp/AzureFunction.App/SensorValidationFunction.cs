using System.Threading;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunction.App
{
    public class SensorValidationFunction
    {
        private readonly ISensorValidationService _validationService;
        private readonly ILogger<SensorValidationFunction> _logger;

        public SensorValidationFunction(ISensorValidationService validationService, ILogger<SensorValidationFunction> logger)
        {
            _validationService = validationService;
            _logger = logger;
        }

        [FunctionName("SensorValidation")]
        public async Task Run(
            [QueueTrigger("aggregated-sensor-data")] AggregatedSensorData aggregatedSensorData,
            [Table("sensoralarms")] IAsyncCollector<SensorAlarm> output,
            CancellationToken cancellationToken)
        {
            var sensorAlarms = await _validationService.ProcessInputAsync(aggregatedSensorData);

            foreach (var sensorAlarm in sensorAlarms)
            {
                await output.AddAsync(sensorAlarm, cancellationToken);
            }
            await output.FlushAsync(cancellationToken);

            return;
        }
    }
}
