using System;
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
            _logger.LogDebug($"Incoming aggregated sensor data: sensor id {aggregatedSensorData.SensorId}");

            var sensor = await _sensorRepository.GetById(aggregatedSensorData.SensorId);

            if (sensor == null)
            {
                _logger.LogError($"No sensor found for id {aggregatedSensorData.SensorId}");
                return;
            }
            await CheckSensorAndUpdateLastSeen(sensor);
            await ValidateAggregatedData(aggregatedSensorData, sensor);
        }

        private async Task CheckSensorAndUpdateLastSeen(Sensor sensor)
        {
            var now = DateTimeOffset.UtcNow;
            if (sensor.LastSeen < now)
                sensor.LastSeen = now;
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
                var existingAlarm = _sensorAlarmRepository.GetBySensorIdAndStatus(sensor.Id, alarmStatus);
                if (existingAlarm != null) return;
            }
            var sensorAlarm = new SensorAlarm
            {
                SensorId = sensor.Id,
                Status = alarmStatus
            };
            await _sensorAlarmRepository.Insert(sensorAlarm);
        }

    }
}
