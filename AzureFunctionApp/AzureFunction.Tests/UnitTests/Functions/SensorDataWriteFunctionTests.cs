using System;
using System.Threading.Tasks;
using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureFunction.Tests.UnitTests.Functions;

public class SensorDataWriteFunctionTests
{
    [Fact]
    public async Task WritesAggregatedData()
    {
        var writeService = A.Fake<ISensorDataService>();
        var function = new SensorDataWriteFunction(writeService, A.Fake<ILogger<SensorDataWriteFunction>>());

        var sensor = new Sensor("test", SensorType.Temperature);
        var aggregatedSensorData = new AggregatedSensorData(sensor, AggregationType.Mean)
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Value = 1
        };
        await function.Run(aggregatedSensorData);

        A.CallTo(() => writeService.InsertAsync(aggregatedSensorData)).MustHaveHappened();
    }
}