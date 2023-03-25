using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureFunction.Tests.UnitTests.Functions;

public class SensorInputFunctionTests
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
    public async Task CreatesOutputForEachAggregation()
    {
        var sensor = new Sensor("test", SensorType.Temperature);
        var aggregatedSensorData = new List<AggregatedSensorData> {new(sensor, AggregationType.Min), new(sensor, AggregationType.Max)};

        var sensorInputService = A.Fake<ISensorInputService>();
        A.CallTo(() => sensorInputService.ProcessInputAsync(Input)).Returns(aggregatedSensorData.ToAsyncEnumerable());

        var function = new SensorInputFunction(sensorInputService, A.Fake<ILogger<SensorInputFunction>>());

        var outputCollector = A.Fake<IAsyncCollector<AggregatedSensorData>>();
            
        var result = await function.Run(Input,
            outputCollector, 
            CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => outputCollector.AddAsync(A<AggregatedSensorData>._, A<CancellationToken>._)).MustHaveHappened(aggregatedSensorData.Count, Times.Exactly);
        A.CallTo(() => outputCollector.FlushAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}