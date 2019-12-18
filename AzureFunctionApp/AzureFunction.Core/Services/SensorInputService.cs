using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Core.Services
{
    public class SensorInputService : ISensorInputService
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly ILogger<ISensorInputService> _logger;

        public SensorInputService(ISensorRepository sensorRepository, ILogger<ISensorInputService> logger)
        {
            _sensorRepository = sensorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AggregatedSensorData>> ProcessInputAsync(SensorInput input)
        {
            _logger.LogInformation($"Incoming raw sensor data: sensor id {input.SensorId}");

            var aggregatedSensorData = new List<AggregatedSensorData>();
            var sensor = await _sensorRepository.GetById(input.SensorId);
            aggregatedSensorData.Add(new AggregatedSensorData
            {
                SensorId = sensor.Id,
                AggregationType = AggregationType.Mean,
                TimeStamp = DateTimeOffset.UtcNow,
                SensorType = sensor.Type,
                Value = input.Values.Average()
            });
            // ToDo weitere Aggregationen
            return aggregatedSensorData;
        }
    }
}
