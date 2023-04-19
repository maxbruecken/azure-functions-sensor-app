using System;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Entities;
using AzureFunction.Core.Models;
using AzureFunction.Core.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureFunction.Tests.IntegrationTests;

public class SensorAppContextTests : DatabaseTestBase
{
    public SensorAppContextTests(SensorAppContextProvider contextProvider) : base(contextProvider)
    {
    }

    [Fact]
    public async Task IncludeTest()
    {
        await SeedDatabase();
        await InvokeOperationOnFreshContext(async context =>
        {
            var query = context.Set<SensorDataEntity>()
                .Include(x => x.Sensor).ThenInclude(x => x.Alarms);

            var action = () => query.ToListAsync();

            await action.Should().NotThrowAsync();
        });
    }

    [Fact]
    public async Task GroupByTest()
    {
        await SeedDatabase();
        await InvokeOperationOnFreshContext(async context =>
        {
            var query = context.Set<SensorDataEntity>()
                .GroupBy(
                    x => new { x.Sensor.BoxId, x.Sensor.Type },
                    (_, y) => new SensorEntityWithDataCount
                    {
                        Sensor = y.First().Sensor,
                        DataCount = y.Count()
                    });

            var action = () => query.ToListAsync();

            await action.Should().NotThrowAsync();
        });
    }

    [Fact]
    public async Task GroupByComplexPropertyTest()
    {
        await SeedDatabase();
        await InvokeOperationOnFreshContext(async context =>
        {
            var query = context.Set<SensorDataEntity>()
                .GroupBy(
                    x => x.Sensor,
                    (x, y) => new SensorEntityWithDataCount
                    {
                        Sensor = x,
                        DataCount = y.Count()
                    });

            var action = () => query.ToListAsync();

            await action.Should().NotThrowAsync();
        });
    }

    [Fact]
    public async Task DoesNotDeleteAlarmsIfNotLoadedExplicitly()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
            var alarmRepository = new SensorAlarmRepository(context);
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow, Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-1), Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-2), Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-3), Status = SensorAlarmStatus.Dead });
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Include(x => x.Alarms)
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().HaveCount(4);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>().Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type).FirstAsync();
            firstPersistedSensor.Alarms.Should().BeEmpty();
            firstPersistedSensor.LastSeen = DateTimeOffset.UtcNow;
            context.Set<SensorEntity>().Update(firstPersistedSensor);
            await context.SaveChangesAsync();
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Include(x => x.Alarms)
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().HaveCount(4);
        });
    }

    [Fact]
    public async Task DeletesAlarmsIfLoadedExplicitlyAndRemoved()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
            var alarmRepository = new SensorAlarmRepository(context);
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow, Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-1), Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-2), Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-3), Status = SensorAlarmStatus.Dead });
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Include(x => x.Alarms)
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().HaveCount(4);
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Include(x => x.Alarms)
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().HaveCount(4);
            firstPersistedSensor.Alarms.RemoveAt(0);
            context.Set<SensorEntity>().Update(firstPersistedSensor);
            await context.SaveChangesAsync();
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Include(x => x.Alarms)
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().HaveCount(3);
        });
    }

    [Fact]
    public async Task InsertsAlarmsIfNotLoadedExplicitlyAndAdded()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        await InvokeOperationOnFreshContext(async context =>
        {
            var repository = new SensorRepository(context);
            await repository.InsertAsync(sensor);
            var alarmRepository = new SensorAlarmRepository(context);
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow, Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-1), Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-2), Status = SensorAlarmStatus.InvalidData });
            await alarmRepository.InsertAsync(new SensorAlarm(sensor) { FiredAt = DateTimeOffset.UtcNow.AddDays(-3), Status = SensorAlarmStatus.Dead });
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().BeEmpty();
            firstPersistedSensor.Alarms.Add(new SensorAlarmEntity
            {
                Sensor = firstPersistedSensor,
                Identifier = Guid.NewGuid().ToString(),
                Status = SensorAlarmStatus.InvalidData,
                FiredAt = DateTimeOffset.UtcNow
            });
            context.Set<SensorEntity>().Update(firstPersistedSensor);
            await context.SaveChangesAsync();
        });

        await InvokeOperationOnFreshContext(async context =>
        {
            var firstPersistedSensor = await context.Set<SensorEntity>()
                .Include(x => x.Alarms)
                .Where(s => s.BoxId == sensor.BoxId && s.Type == sensor.Type)
                .FirstAsync();
            firstPersistedSensor.Alarms.Should().HaveCount(5);
        });
    }

    private async Task SeedDatabase()
    {
        await InvokeOperationOnFreshContext(async context =>
        {
            var firstSensor = new SensorEntity
            {
                BoxId = "test",
                Type = SensorType.Temperature
            };
            await context.Set<SensorEntity>().AddAsync(firstSensor);
            var secondSensor = new SensorEntity
            {
                BoxId = "test",
                Type = SensorType.Pressure
            };
            await context.Set<SensorEntity>().AddAsync(secondSensor);
            var now = DateTimeOffset.UtcNow;
            await context.Set<SensorAlarmEntity>().AddAsync(new SensorAlarmEntity
            {
                Sensor = firstSensor,
                FiredAt = now.AddMinutes(-5),
                Identifier = Guid.NewGuid().ToString(),
                Status = SensorAlarmStatus.InvalidData
            });
            await context.Set<SensorAlarmEntity>().AddAsync(new SensorAlarmEntity
            {
                Sensor = firstSensor,
                FiredAt = now,
                Identifier = Guid.NewGuid().ToString(),
                Status = SensorAlarmStatus.InvalidData
            });
            await context.Set<SensorDataEntity>().AddAsync(new SensorDataEntity
            {
                Sensor = firstSensor,
                AggregationType = AggregationType.Mean,
                CreatedAt = now.AddMinutes(-5),
                Value = 1
            });
            await context.Set<SensorDataEntity>().AddAsync(new SensorDataEntity
            {
                Sensor = firstSensor,
                AggregationType = AggregationType.Min,
                CreatedAt = now.AddMinutes(-5),
                Value = 0
            });
            await context.Set<SensorDataEntity>().AddAsync(new SensorDataEntity
            {
                Sensor = firstSensor,
                AggregationType = AggregationType.Max,
                CreatedAt = now.AddMinutes(-5),
                Value = 2
            });
            await context.Set<SensorDataEntity>().AddAsync(new SensorDataEntity
            {
                Sensor = firstSensor,
                AggregationType = AggregationType.Mean,
                CreatedAt = now,
                Value = 1
            });
            await context.Set<SensorDataEntity>().AddAsync(new SensorDataEntity
            {
                Sensor = firstSensor,
                AggregationType = AggregationType.Min,
                CreatedAt = now,
                Value = 0
            });
            await context.Set<SensorDataEntity>().AddAsync(new SensorDataEntity
            {
                Sensor = firstSensor,
                AggregationType = AggregationType.Max,
                CreatedAt = now,
                Value = 2
            });
            await context.SaveChangesAsync();
        });
    }
}

internal class SensorEntityWithDataCount
{
    public SensorEntity Sensor { get; set; }
    public int DataCount { get; set; }
}