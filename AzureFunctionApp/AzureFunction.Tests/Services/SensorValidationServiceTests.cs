using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureFunction.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace AzureFunction.Core.Services.Tests
{
    [TestClass()]
    public class SensorValidationServiceTests
    {
        [TestMethod()]
        public async Task ValidateSensorDataAsyncTestAsync()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetById("test")).Returns(new Sensor { Id = "test", Type = SensorType.Temperature });
            var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

            var aggregatedSensorData = await service.ProcessInputAsync(new SensorInput { SensorId = "test", Values = new List<double> { 1, 2, 3 } });

            aggregatedSensorData.Should().HaveCount(4);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Mean);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Min);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Max);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.StandardDeviation);
            Assert.Fail();
        }

        [TestMethod()]
        public void CheckSensorsAndAlarmsTest()
        {
            Assert.Fail();
        }
    }
}