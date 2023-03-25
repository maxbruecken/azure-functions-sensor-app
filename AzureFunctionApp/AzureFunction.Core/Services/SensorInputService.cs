using System;
using System.Collections.Generic;
using System.Linq;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Core.Services;

public class SensorInputService : ISensorInputService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ILogger<ISensorInputService> _logger;

    public SensorInputService(ISensorRepository sensorRepository, ILogger<SensorInputService> logger)
    {
        _sensorRepository = sensorRepository;
        _logger = logger;
    }

    public async IAsyncEnumerable<AggregatedSensorData> ProcessInputAsync(SensorInput input)
    {
        _logger.LogDebug("Incoming raw sensor data: sensor id {SensorBoxId}", input.SensorBoxId);

        if (!input.Data.SelectMany(x => x.Values).Any())
        {
            yield break;
        }

        foreach (var sensorData in input.Data)
        {
            var sensor = await _sensorRepository.GetByBoxIdAndTypeAsync(input.SensorBoxId, sensorData.Type);
            if (sensor is null)
            {
                throw new InvalidOperationException($"Sensor with box id {input.SensorBoxId} and type {sensorData.Type} not found.");
            }

            foreach (var data in CalculateAggregatedData(input, sensor, sensorData))
            {
                yield return data;
            }
        }
    }

    private static IEnumerable<AggregatedSensorData> CalculateAggregatedData(SensorInput input, Sensor sensor, SensorData sensorData)
    {
        yield return new AggregatedSensorData(sensor, AggregationType.Mean)
        {
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Average()
        };
        yield return new AggregatedSensorData(sensor, AggregationType.Min)
        {
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Min()
        };
        yield return new AggregatedSensorData(sensor, AggregationType.Max)
        {
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Max()
        };
        yield return new AggregatedSensorData(sensor, AggregationType.StandardDeviation)
        {
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Aggregate(0d, (a, x) => a + Math.Pow(x - sensorData.Values.Average(), 2)) / (sensorData.Values.Count() - 1)
        };
    }
}