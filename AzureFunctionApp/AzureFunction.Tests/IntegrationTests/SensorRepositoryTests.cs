using System;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Models;
using AzureFunction.Core.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureFunction.Tests.IntegrationTests;

public class SensorRepositoryTests : DatabaseTestBase
{
    public SensorRepositoryTests(SensorAppContextProvider contextProvider) : base(contextProvider)
    {
    }

    [Fact]
    public async Task CanCreateSensors()
    {
        var sensor = new Sensor("test", SensorType.Temperature)
        {
            Information = new SensorInformation("Test", "0815", "4711")
        };
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var allSensors = await repository.GetAllAsync();

            allSensors.Should().Contain(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type && s.Information.Manufacturer == sensor.Information.Manufacturer);
        });
    }

    [Fact]
    public async Task ThrowsWhileInsertingDuplicates()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var action = () => repository.InsertAsync(sensor);

            await action.Should().ThrowAsync<DbUpdateException>();
        });
    }

    [Fact]
    public async Task CanUpdateSensors()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
        });
        
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var persistedSensor = await repository.GetByBoxIdAndTypeAsync(sensor.BoxId, sensor.Type);

            persistedSensor!.LastSeen.Should().Be(DateTimeOffset.MinValue);
        });
        
        var lastSeen = DateTimeOffset.UtcNow;
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            sensor.LastSeen = lastSeen;
            await repository.UpdateAsync(sensor);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var updatedSensor = await repository.GetByBoxIdAndTypeAsync(sensor.BoxId, sensor.Type);

            updatedSensor!.LastSeen.Should().BeCloseTo(lastSeen, TimeSpan.FromMilliseconds(1));
        });
    }

    [Fact]
    public async Task CanUpdateSensorInformation()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
        });
        
        const string manufacturer = "Test";
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            sensor.Information = sensor.Information with { Manufacturer = manufacturer };
            await repository.UpdateAsync(sensor);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var updatedSensor = await repository.GetByBoxIdAndTypeAsync(sensor.BoxId, sensor.Type);

            updatedSensor!.Information.Manufacturer.Should().Be(manufacturer);
        });
    }

    [Fact]
    public async Task CanGetSensorsWithAlarms()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
            var alarmRepository = new SensorAlarmRepository(context);
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow, Status = SensorAlarmStatus.InvalidData });
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            var allSensors = await repository.GetAllAsync(true);

            var firstPersistedSensor = allSensors.First(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type);
            firstPersistedSensor.Alarms.Should().HaveCount(1);
            firstPersistedSensor.Alarms.First().Status.Should().Be(SensorAlarmStatus.InvalidData);
        });
    }
}