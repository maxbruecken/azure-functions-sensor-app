using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using FakeItEasy;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests
{
    [TestClass]
    public class SensorInputFunctionTests
    {
        [TestMethod]
        public async Task CreatesOutputForEachAggregation()
        {
            var sensorInput = new SensorInput { SensorId = "test", Values = new List<double> { 1, 2, 3 } };
            var aggregatedSensorData = new List<AggregatedSensorData> {new AggregatedSensorData(), new AggregatedSensorData()};

            var sensorInputService = A.Fake<ISensorInputService>();
            A.CallTo(() => sensorInputService.ProcessInputAsync(sensorInput)).Returns(aggregatedSensorData);

            var function = new SensorInputFunction(sensorInputService, A.Fake<ILogger<SensorInputFunction>>());

            var outputCollector = A.Fake<IAsyncCollector<AggregatedSensorData>>();
            await function.Run(sensorInput,
                outputCollector, CancellationToken.None);

            A.CallTo(() => outputCollector.AddAsync(A<AggregatedSensorData>._, A<CancellationToken>._)).MustHaveHappened(aggregatedSensorData.Count, Times.Exactly);
            A.CallTo(() => outputCollector.FlushAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}