using System;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using Xunit;

namespace AzureFunction.Tests.UnitTests.Services;

public class SensorDataServiceTests
{
    [Fact]
    public async Task InsertAsyncTest()
    {
        var sensorDataRepository = A.Fake<ISensorDataRepository>();
        var service = new SensorDataService(sensorDataRepository);
        var sensor = new Sensor("test", SensorType.Temperature);
        var aggregatedSensorData = new AggregatedSensorData(sensor, AggregationType.Mean)
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Value = 1
        };

        await service.InsertAsync(aggregatedSensorData);
            
        A.CallTo(() => sensorDataRepository.InsertAsync(aggregatedSensorData)).MustHaveHappened();
    }
        
}