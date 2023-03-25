using System;
using System.Threading.Tasks;
using AzureFunction.Core.Models;
using AzureFunction.Core.Repositories;
using FluentAssertions;
using Xunit;

namespace AzureFunction.Tests.IntegrationTests;

public class SensorAlarmRepositoryTests : DatabaseTestBase
{
    public SensorAlarmRepositoryTests(SensorAppContextProvider contextProvider) : base(contextProvider)
    {
    }

    [Fact]
    public async Task CanCreateAlarms()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        var alarm = new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow, Status = SensorAlarmStatus.InvalidData };
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
            var alarmRepository = new SensorAlarmRepository(context);
            await alarmRepository.InsertAsync(alarm);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var persistedSensor = await repository.GetByBoxIdAndTypeAsync(sensor.BoxId, sensor.Type, true);

            persistedSensor!.Alarms.Should().Contain(a => a.Identifier == alarm.Identifier);
        });
    }
}