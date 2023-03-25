using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureFunction.Tests.UnitTests.Services;

public class SensorInputServiceTests
{
    private static readonly SensorInput Input = new()
    {
        SensorBoxId = "test",
        Timestamp = DateTimeOffset.UtcNow,
        Data = new List<SensorData>
        {
            new()
            {
                Type = SensorType.Temperature,
                Values = new List<double> { 1, 2, 3 }
            }
        }
    };
        
    [Fact]
    public async Task CreatesAllAggregations()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetByBoxIdAndTypeAsync("test", SensorType.Temperature, A<bool>._)).Returns(new Sensor("test", SensorType.Temperature));
        var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

        var aggregatedSensorData = await service.ProcessInputAsync(Input).ToListAsync();

        aggregatedSensorData.Should().HaveCount(4);
        aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Mean);
        aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Min);
        aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Max);
        aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.StandardDeviation);
    }

    [Fact]
    public async Task CalculatesAggregationsProperly()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetByBoxIdAndTypeAsync("test", SensorType.Temperature, A<bool>._)).Returns(new Sensor("test", SensorType.Temperature));
        var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

        var aggregatedSensorData = await service.ProcessInputAsync(Input).ToListAsync();

        aggregatedSensorData.First(a => a.AggregationType == AggregationType.Mean).Value.Should().Be(2d);
        aggregatedSensorData.First(a => a.AggregationType == AggregationType.Min).Value.Should().Be(1d);
        aggregatedSensorData.First(a => a.AggregationType == AggregationType.Max).Value.Should().Be(3d);
        aggregatedSensorData.First(a => a.AggregationType == AggregationType.StandardDeviation).Value.Should().Be(1d);
    }

    [Fact]
    public async Task ReturnsNoAggregationsIfInputIsEmpty()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetByBoxIdAndTypeAsync("test", SensorType.Temperature, A<bool>._)).Returns(new Sensor("test", SensorType.Temperature));
        var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

        var aggregatedSensorData = await service.ProcessInputAsync(new SensorInput
        {
            SensorBoxId = "test",
            Timestamp = DateTimeOffset.UtcNow,
            Data = new List<SensorData>()
        }).ToListAsync();

        aggregatedSensorData.Should().BeEmpty();
    }
}