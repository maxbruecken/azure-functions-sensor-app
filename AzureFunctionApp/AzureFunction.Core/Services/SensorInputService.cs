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
            _logger.LogDebug($"Incoming raw sensor data: sensor id {input.SensorId}");

            if (!input.Values.Any())
            {
                return Enumerable.Empty<AggregatedSensorData>();
            }

            var aggregatedSensorData = new List<AggregatedSensorData>();
            var sensor = await _sensorRepository.GetById(input.SensorId);
            var timestamp = DateTimeOffset.UtcNow;
            var mean = input.Values.Average();
            aggregatedSensorData.Add(new AggregatedSensorData
            {
                SensorId = sensor.Id,
                AggregationType = AggregationType.Mean,
                CreatedAt = timestamp,
                SensorType = sensor.Type,
                Value = mean
            });
            aggregatedSensorData.Add(new AggregatedSensorData
            {
                SensorId = sensor.Id,
                AggregationType = AggregationType.Min,
                CreatedAt = timestamp,
                SensorType = sensor.Type,
                Value = input.Values.Min()
            });
            aggregatedSensorData.Add(new AggregatedSensorData
            {
                SensorId = sensor.Id,
                AggregationType = AggregationType.Max,
                CreatedAt = timestamp,
                SensorType = sensor.Type,
                Value = input.Values.Max()
            });
            aggregatedSensorData.Add(new AggregatedSensorData
            {
                SensorId = sensor.Id,
                AggregationType = AggregationType.StandardDeviation,
                CreatedAt = timestamp,
                SensorType = sensor.Type,
                Value = input.Values.Aggregate(0d, (a, x) => a += Math.Pow(x - mean, 2)) / (input.Values.Count() - 1)
            });
            return aggregatedSensorData;
        }
    }
}
