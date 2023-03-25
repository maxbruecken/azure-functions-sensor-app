using System;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureFunction.Tests.UnitTests.Services;

public class SensorValidationServiceTests
{
    [Fact]
    public async Task ValidateAggregatedDataAsyncTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        var sensor = new Sensor("test", SensorType.Temperature) { Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10) };
        A.CallTo(() => sensorRepository.GetByBoxIdAndTypeAsync("test", SensorType.Temperature, A<bool>._)).Returns(sensor);
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.ValidateSensorDataAsync(new AggregatedSensorData(sensor, AggregationType.Max) { Value = 4 });
        A.CallTo(() => sensorAlarmRepository.InsertAsync(A<SensorAlarm>.That.Matches(a => a.Status == SensorAlarmStatus.InvalidData))).MustHaveHappened();

        await service.ValidateSensorDataAsync(new AggregatedSensorData(sensor, AggregationType.Max) { Value = 11 });
        A.CallTo(() => sensorAlarmRepository.InsertAsync(A<SensorAlarm>.That.Matches(a => a.Status == SensorAlarmStatus.InvalidData))).MustHaveHappened();
    }

    [Fact]
    public async Task CheckSensorsAndAlarmsTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetAllAsync(true))
            .Returns(new[]
            {
                new Sensor("test", SensorType.Temperature)
                {
                    Min = 5, 
                    Max = 10, 
                    LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10)
                }
            });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
        A.CallTo(() => sensorAlarmRepository.InsertAsync(A<SensorAlarm>.That.Matches(a => a.Status == SensorAlarmStatus.Dead))).MustHaveHappened();
    }

    [Fact]
    public async Task CheckObsoleteAlarmsTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        var sensor = new Sensor("test", SensorType.Temperature)
        {
            Min = 5, 
            Max = 10, 
            LastSeen = DateTimeOffset.UtcNow
        };
        var alarm = new SensorAlarm(sensor) { Status = SensorAlarmStatus.Dead };
        sensor.Alarms = new[] { alarm };
        A.CallTo(() => sensorRepository.GetAllAsync(true)).Returns(new[] { sensor });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
        A.CallTo(() => sensorAlarmRepository.DeleteAsync(alarm)).MustHaveHappened();
    }
        
    [Fact]
    public async Task CheckNoObsoleteAlarms()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetAllAsync(true))
            .Returns(new[]
            {
                new Sensor("test", SensorType.Temperature) { Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow }
            });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
            
        A.CallTo(() => sensorAlarmRepository.DeleteAsync(A<SensorAlarm>._)).MustNotHaveHappened();
    }
        
    [Fact]
    public async Task CheckNonObsoleteAlarmsOfDeadSensor()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        var sensor = new Sensor("test", SensorType.Temperature)
        {
            Min = 5, 
            Max = 10, 
            LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        var alarm = new SensorAlarm(sensor) { Status = SensorAlarmStatus.Dead };
        sensor.Alarms = new[] { alarm };
        A.CallTo(() => sensorRepository.GetAllAsync(true)).Returns(new[] { sensor });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
            
        A.CallTo(() => sensorAlarmRepository.DeleteAsync(A<SensorAlarm>._)).MustNotHaveHappened();
    }
}