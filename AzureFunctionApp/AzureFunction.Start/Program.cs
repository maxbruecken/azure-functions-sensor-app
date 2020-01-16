using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using AzureFunction.Core.Models;
using AzureFunction.Core.Repositories;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace AzureFunction.Start
{
    class Program
    {
        private const int SensorCount = 10;
        private const int SensorInputCount = 2;
        
        private const int MinSensorTemperature = -60;
        private const int MaxSensorTemperature = 80;
        private const int MinSensorVoltage = 300;
        private const int MaxSensorVoltage = 400;

        static async Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", false)
                //.AddJsonFile("appsettings.json")
                .Build();

            var random = new Random();
            var sensorRepository = new SensorRepository(configuration["AzureWebJobsStorage"], "sensors");
            var sensors = (await sensorRepository.GetAll()).ToList();
            while(sensors.Count < SensorCount)
            {
                var sensorType = random.Next(0, 2) == 0 ? SensorType.Temperature : SensorType.Voltage;
                var sensor = new Sensor
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = sensorType,
                    Min = sensorType == SensorType.Temperature ? MinSensorTemperature : MinSensorVoltage,
                    Max = sensorType == SensorType.Temperature ? MaxSensorTemperature : MaxSensorVoltage
                };
                await sensorRepository.Insert(sensor);
                sensors.Add(sensor);
            }
            
            var clientApplication = ConfidentialClientApplicationBuilder
                .Create(configuration["ClientId"])
                .WithAuthority(AzureCloudInstance.AzurePublic, configuration["TenantId"])
                .WithClientSecret(configuration["ClientSecret"])
                .Build();

            var token = await clientApplication
                .AcquireTokenForClient(new[] {configuration["ApplicationScope"]})
                .ExecuteAsync();
            
            var httpClient = new HttpClient
            {
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken) },
                Timeout = TimeSpan.FromMinutes(5)
            };

            var uri = new Uri($"{configuration["ApplicationUri"]}api/SensorInput");
            var tasks = new List<Task<HttpResponseMessage>>();
            for (var i = 0; i < SensorInputCount; i++)
            {
                var sensor = sensors[random.Next(0, sensors.Count)];
                var sensorInput = new SensorInput
                {
                    SensorId = sensor.Id,
                    Values = Enumerable.Repeat(0, 10).Select(_ => CreateSensorValue(sensor, random)).ToList()
                };
                var content = new StringContent(JsonConvert.SerializeObject(sensorInput), Encoding.UTF8, "application/json");
                tasks.Add(httpClient.PostAsync(uri, content));
            }

            var responses = await Task.WhenAll(tasks);
            Console.Out.WriteLine($"Sent {tasks.Count()} sensor inputs for {sensors.Count} sensors. {responses.Count(r => r.IsSuccessStatusCode)} tasks completed successfully, {responses.Count(r => !r.IsSuccessStatusCode)} tasks failed.");
        }

        private static double CreateSensorValue(Sensor sensor, Random random)
        {
            var x = random.NextDouble();
            return sensor.Type == SensorType.Temperature 
                ? MinSensorTemperature + x * (MaxSensorTemperature - MinSensorTemperature)
                : MinSensorVoltage + x * (MaxSensorVoltage - MinSensorVoltage);
        }
    }
}