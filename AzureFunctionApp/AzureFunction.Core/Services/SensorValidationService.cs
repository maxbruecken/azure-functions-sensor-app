using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Core.Services;

public class SensorValidationService : ISensorValidationService
{
    private static readonly List<AggregationType> AggregationTypesToValidate = new()
    {
        AggregationType.Mean,
        AggregationType.Max,
        AggregationType.Min
    };
        
    private readonly ISensorRepository _sensorRepository;
    private readonly ISensorAlarmRepository _sensorAlarmRepository;
    private readonly ILogger<ISensorValidationService> _logger;

    public SensorValidationService(ISensorRepository sensorRepository,
        ISensorAlarmRepository sensorAlarmRepository,
        ILogger<SensorValidationService> logger)
    {
        _sensorRepository = sensorRepository;
        _sensorAlarmRepository = sensorAlarmRepository;
        _logger = logger;
    }

    public async Task ValidateSensorDataAsync(AggregatedSensorData aggregatedSensorData)
    {
        _logger.LogDebug("Incoming aggregated sensor data: sensor id {SensorBoxId}", aggregatedSensorData.Sensor.BoxId);

        var sensor = await _sensorRepository.GetByBoxIdAndTypeAsync(aggregatedSensorData.Sensor.BoxId, aggregatedSensorData.Sensor.Type, true);

        if (sensor == null)
        {
            _logger.LogError("No sensor found for id {SensorBoxId}", aggregatedSensorData.Sensor.BoxId);
            return;
        }
        await CheckSensorAndUpdateLastSeen(sensor);
        await ValidateAggregatedData(aggregatedSensorData, sensor);
    }

    public async Task CheckSensorsAndAlarmsAsync()
    {
        var deadLine = DateTimeOffset.UtcNow.AddMinutes(-5);
        var sensors = await _sensorRepository.GetAllAsync(true);
        await RemoveObsoleteAlarms(sensors, deadLine);
        await FireAlarmsForDeadSensors(sensors, deadLine);
    }

    private async Task RemoveObsoleteAlarms(IEnumerable<Sensor> sensors, DateTimeOffset deadLine)
    {
        await sensors
            .Where(s => s.LastSeen > deadLine)
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async s =>
            {
                var sensorAlarm= s.Alarms.FirstOrDefault(a => a.Status == SensorAlarmStatus.Dead);
                if (sensorAlarm == null)
                {
                    return;
                }
                await _sensorAlarmRepository.DeleteAsync(sensorAlarm);
            });
    }

    private async Task FireAlarmsForDeadSensors(IEnumerable<Sensor> sensors, DateTimeOffset deadLine)
    {
        await sensors
            .Where(s => s.LastSeen < deadLine && s.Alarms.All(a => a.Status != SensorAlarmStatus.Dead))
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(s => CreateSensorAlarm(s, SensorAlarmStatus.Dead, true));
    }

    private async Task CheckSensorAndUpdateLastSeen(Sensor sensor)
    {
        var now = DateTimeOffset.UtcNow;
        if (sensor.LastSeen < now) sensor.LastSeen = now;
        await _sensorRepository.UpdateAsync(sensor);
    }

    private async Task ValidateAggregatedData(AggregatedSensorData aggregatedSensorData, Sensor sensor)
    {
        if (!AggregationTypesToValidate.Contains(aggregatedSensorData.AggregationType))
        {
            return;
        }
        if (aggregatedSensorData.Value < sensor.Min || aggregatedSensorData.Value > sensor.Max)
        {
            await CreateSensorAlarm(sensor, SensorAlarmStatus.InvalidData);
        }
    }

    private async Task CreateSensorAlarm(Sensor sensor, SensorAlarmStatus alarmStatus, bool singleton = false)
    {
        if (singleton)
        {
            var existingAlarm = sensor.Alarms.FirstOrDefault(x => x.Status == alarmStatus);
            if (existingAlarm != null) return;
        }
        var sensorAlarm = new SensorAlarm(sensor)
        {
            Status = alarmStatus
        };
        await _sensorAlarmRepository.InsertAsync(sensorAlarm);
    }
}