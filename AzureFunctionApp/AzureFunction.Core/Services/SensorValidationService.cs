using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Core.Services
{
    public class SensorValidationService : ISensorValidationService
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorAlarmRepository _sensorAlarmRepository;
        private readonly ILogger<ISensorValidationService> _logger;

        public SensorValidationService(ISensorRepository sensorRepository,
            ISensorAlarmRepository sensorAlarmRepository,
            ILogger<ISensorValidationService> logger)
        {
            _sensorRepository = sensorRepository;
            _sensorAlarmRepository = sensorAlarmRepository;
            _logger = logger;
        }

        public async Task ValidateSensorDataAsync(AggregatedSensorData aggregatedSensorData)
        {
            _logger.LogDebug($"Incoming aggregated sensor data: sensor id {aggregatedSensorData.SensorBoxId}");

            var sensor = await _sensorRepository.GetByBoxIdAndType(aggregatedSensorData.SensorBoxId, aggregatedSensorData.SensorType);

            if (sensor == null)
            {
                _logger.LogError($"No sensor found for id {aggregatedSensorData.SensorBoxId}");
                return;
            }
            await CheckSensorAndUpdateLastSeen(sensor);
            await ValidateAggregatedData(aggregatedSensorData, sensor);
        }

        public async Task CheckSensorsAndAlarmsAsync()
        {
            var deadLine = DateTimeOffset.UtcNow.AddMinutes(-5);
            var sensors = await _sensorRepository.GetAll();
            await FireAlarmsForDeadSensors(sensors, deadLine);
            await RemoveObsoleteAlarms(sensors, deadLine);
        }

        private async Task RemoveObsoleteAlarms(IEnumerable<Sensor> sensors, DateTimeOffset deadLine)
        {
            var tasks = sensors
                .Where(s => s.LastSeen > deadLine)
                .ToList()
                .Select(async s =>
                {
                    var sensorAlarm = await _sensorAlarmRepository.GetBySensorBoxIdAndSensorTypeAndStatus(s.BoxId, s.Type, AlarmStatus.Dead);
                    if (sensorAlarm == null)
                    {
                        return;
                    }
                    await _sensorAlarmRepository.Delete(sensorAlarm);
                });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task FireAlarmsForDeadSensors(IEnumerable<Sensor> sensors, DateTimeOffset deadLine)
        {
            var tasks = sensors
                .Where(s => s.LastSeen < deadLine)
                .ToList()
                .Select(s => CreateSensorAlarm(s, AlarmStatus.Dead, true));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task CheckSensorAndUpdateLastSeen(Sensor sensor)
        {
            var now = DateTimeOffset.UtcNow;
            if (sensor.LastSeen < now) sensor.LastSeen = now;
            await _sensorRepository.Update(sensor);
        }

        private async Task ValidateAggregatedData(AggregatedSensorData aggregatedSensorData, Sensor sensor)
        {
            if (aggregatedSensorData.AggregationType != AggregationType.Min && aggregatedSensorData.AggregationType != AggregationType.Max)
            {
                return;
            }
            if (aggregatedSensorData.Value < sensor.Min || aggregatedSensorData.Value > sensor.Max)
            {
                await CreateSensorAlarm(sensor, AlarmStatus.InvalidData);
            }
        }

        private async Task CreateSensorAlarm(Sensor sensor, AlarmStatus alarmStatus, bool singleton = false)
        {
            if (singleton)
            {
                var existingAlarm = await _sensorAlarmRepository.GetBySensorBoxIdAndSensorTypeAndStatus(sensor.BoxId, sensor.Type, alarmStatus);
                if (existingAlarm != null) return;
            }
            var sensorAlarm = new SensorAlarm
            {
                SensorBoxId = sensor.BoxId,
                SensorType = sensor.Type,
                Status = alarmStatus
            };
            await _sensorAlarmRepository.Insert(sensorAlarm);
        }

    }
}
