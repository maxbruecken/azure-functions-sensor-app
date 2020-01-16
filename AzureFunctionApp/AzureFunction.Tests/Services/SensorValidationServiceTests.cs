using System;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests.Services
{
    [TestClass()]
    public class SensorValidationServiceTests
    {
        [TestMethod()]
        public async Task ValidateAggregatedDataAsyncTest()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetById("test")).Returns(new Sensor { Id = "test", Type = SensorType.Temperature, Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10) });
            ISensorAlarmRepository sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
            A.CallTo(() => sensorAlarmRepository.GetBySensorIdAndStatus("test", AlarmStatus.InvalidData)).Returns((SensorAlarm)null);

            var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

            await service.ValidateSensorDataAsync(new AggregatedSensorData { SensorId = "test", AggregationType = AggregationType.Max, SensorType = SensorType.Temperature, Value = 4 });
            A.CallTo(() => sensorAlarmRepository.Insert(A<SensorAlarm>.That.Matches(a => a.Status == AlarmStatus.InvalidData))).MustHaveHappened();

            await service.ValidateSensorDataAsync(new AggregatedSensorData { SensorId = "test", AggregationType = AggregationType.Max, SensorType = SensorType.Temperature, Value = 11 });
            A.CallTo(() => sensorAlarmRepository.Insert(A<SensorAlarm>.That.Matches(a => a.Status == AlarmStatus.InvalidData))).MustHaveHappened();
        }

        [TestMethod()]
        public async Task CheckSensorsAndAlarmsTest()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetAll()).Returns(new[] { new Sensor { Id = "test", Type = SensorType.Temperature, LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10) } });
            ISensorAlarmRepository sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
            A.CallTo(() => sensorAlarmRepository.GetBySensorIdAndStatus("test", AlarmStatus.Dead)).Returns((SensorAlarm)null);

            var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

            await service.CheckSensorsAndAlarmsAsync();
            A.CallTo(() => sensorAlarmRepository.Insert(A<SensorAlarm>.That.Matches(a => a.Status == AlarmStatus.Dead))).MustHaveHappened();
        }

        [TestMethod()]
        public async Task CheckObsoleteAlarmsTest()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetAll()).Returns(new[] { new Sensor { Id = "test", Type = SensorType.Temperature, LastSeen = DateTimeOffset.UtcNow } });
            ISensorAlarmRepository sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
            SensorAlarm alarm = new SensorAlarm { SensorId = "test", Status = AlarmStatus.Dead };
            A.CallTo(() => sensorAlarmRepository.GetBySensorIdAndStatus("test", AlarmStatus.Dead)).Returns(alarm);

            var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

            await service.CheckSensorsAndAlarmsAsync();
            A.CallTo(() => sensorAlarmRepository.Delete(alarm)).MustHaveHappened();
        }
        
        [TestMethod]
        public async Task CheckObsoleteAlarmsResultsNullTest()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetAll()).Returns(new[] { new Sensor { Id = "test", Type = SensorType.Temperature, LastSeen = DateTimeOffset.UtcNow } });
            ISensorAlarmRepository sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
            SensorAlarm alarm = new SensorAlarm { SensorId = "test", Status = AlarmStatus.Dead };
            A.CallTo(() => sensorAlarmRepository.GetBySensorIdAndStatus("test", AlarmStatus.Dead)).Returns((SensorAlarm)null);

            var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

            await service.CheckSensorsAndAlarmsAsync();
            
            A.CallTo(() => sensorAlarmRepository.Delete(null)).MustNotHaveHappened();
        }
    }
}