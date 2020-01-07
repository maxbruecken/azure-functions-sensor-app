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
        private readonly ILogger<ISensorValidationService> _logger;

        public SensorValidationService(ISensorRepository sensorRepository, ILogger<ISensorValidationService> logger)
        {
            _sensorRepository = sensorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<SensorAlarm>> ProcessInputAsync(AggregatedSensorData aggregatedSensorData)
        {
            _logger.LogDebug($"Incoming aggregated sensor data: sensor id {aggregatedSensorData.SensorId}");

            var sensor = await _sensorRepository.GetById(aggregatedSensorData.SensorId);

            if (sensor == null)
            {
                _logger.LogError($"No sensor found for id {aggregatedSensorData.SensorId}");
                return Enumerable.Empty<SensorAlarm>();
            }
            CheckSensorAndUpdateLastSeen(sensor);

            var sensorAlarms = await ValidateAggregatedData(aggregatedSensorData, sensor);

            return sensorAlarms;
        }

        private void CheckSensorAndUpdateLastSeen(Sensor sensor)
        {
            var now = DateTimeOffset.UtcNow;
            if (sensor.LastSeen < now)
                sensor.LastSeen = now;
            _sensorRepository.Update(sensor);
        }

        private static async Task<IEnumerable<SensorAlarm>> ValidateAggregatedData(AggregatedSensorData aggregatedSensorData, Sensor sensor)
        {
            var sensorAlarms = new List<SensorAlarm>();
            if (aggregatedSensorData.Value < sensor.Min || aggregatedSensorData.Value > sensor.Max)
            {
                await CreateSensorAlarm(sensorAlarms, sensor, AlarmStatus.InvalidData);
            }
            return sensorAlarms;
        }

        private static async Task CreateSensorAlarm(List<SensorAlarm> sensorAlarms, Sensor s, AlarmStatus alarmStatus, bool singleton = false)
        {
            if (singleton)
            {
                var existingAlarm = sensorAlarms.Where(a => a.SensorId == s.Id && a.StatusString == alarmStatus.ToString()).FirstOrDefault();
                if (existingAlarm != null) return;
            }
            var sensorAlarm = new SensorAlarm
            {
                SensorId = s.Id,
                Status = alarmStatus
            };
            await Task.Run(()=> sensorAlarms.Add(sensorAlarm));
        }

    }
}
