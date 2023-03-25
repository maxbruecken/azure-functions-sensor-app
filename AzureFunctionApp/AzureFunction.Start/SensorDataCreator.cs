using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.Core.DbContext;
using AzureFunction.Core.Models;
using AzureFunction.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Start;

public class SensorDataCreator : BackgroundService
{
    private const int BoxCount = 10;
    private const int InputCount = 100;
        
    private static readonly IDictionary<SensorType, (double Min, double Max)> Ranges = new Dictionary<SensorType, (double Min, double Max)>
    {
        {SensorType.Temperature, (Min: -60, Max: 80)},
        {SensorType.Humidity, (Min: 0, Max: 100)},
        {SensorType.Pressure, (Min: 800, Max: 1200)},
        {SensorType.Quality, (Min: 0, Max: 500)}
    };
    
    private readonly IDbContextFactory<SensorAppContext> _contextFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SensorDataCreator> _logger;

    public SensorDataCreator(IDbContextFactory<SensorAppContext> contextFactory, IConfiguration configuration, ILogger<SensorDataCreator> logger)
    {
        _contextFactory = contextFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(stoppingToken);
        await context.Database.MigrateAsync(cancellationToken: stoppingToken);
        var defaultJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
        var random = new Random();

        var sensorRepository = new SensorRepository(context);
        var sensors = (await sensorRepository.GetAllAsync()).ToList();
        var sensorBoxes = sensors.GroupBy(x => x.BoxId).Select(g => new SensorBox {Id = g.Key}).ToList();
        while(sensorBoxes.Count < BoxCount)
        {
            var boxId = Guid.NewGuid().ToString();
            foreach (var (sensorType, (min, max)) in Ranges)
            {
                var sensor = new Sensor(boxId, sensorType)
                {
                    Min = min,
                    Max = max
                };
                await sensorRepository.InsertAsync(sensor);
                sensors.Add(sensor);   
            }
            sensorBoxes.Add(new SensorBox {Id = boxId});
        }

        var utcNow = DateTimeOffset.UtcNow;
        foreach (var sensorBox in sensorBoxes)
        {
            sensorBox.LastSend = utcNow.Subtract(TimeSpan.FromMinutes(1.0 + random.NextDouble()));
        }
            
        _logger.LogInformation("All sensors are ready. Starting sending of inputs ...");
            
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        var uri = new Uri($"{_configuration["ApplicationUri"]}api/SensorInput");
        var tasks = new List<Task<HttpResponseMessage>>();
        var taskChunk = new List<Task<HttpResponseMessage>>();
        var sentCount = 0;
        while (sentCount < InputCount)
        {
            var timeWindow = TimeSpan.FromMinutes(1.0 + random.NextDouble());
            utcNow = DateTimeOffset.UtcNow;
            var oldSensorBoxes = sensorBoxes.Where(x => x.LastSend < utcNow - timeWindow).ToList();
            if (!oldSensorBoxes.Any())
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }
            var sensorBox = oldSensorBoxes.ElementAt(random.Next(0, oldSensorBoxes.Count));
            var sensorInput = new SensorInput
            {
                SensorBoxId = sensorBox.Id,
                Timestamp = utcNow,
                Data = Ranges
                    .Select(x => new SensorData
                    {
                        Type = x.Key,
                        Values = Enumerable.Range(0, (int) ((utcNow - sensorBox.LastSend).TotalSeconds / 3.0)).Select(_ => CreateSensorValue(x.Key, random)).ToList()
                    })
                    .ToList()
            };
            sensorBox.LastSend = utcNow;
            var content = new StringContent(JsonSerializer.Serialize(sensorInput, defaultJsonOptions), Encoding.UTF8, "application/json");
            var task = httpClient.PostAsync(uri, content, stoppingToken);
            tasks.Add(task);
            taskChunk.Add(task);
            sentCount++;
                
            if (taskChunk.Count >= 10)
            {
                await Task.WhenAll(taskChunk);
                taskChunk.Clear();
                _logger.LogInformation("Sent {SentCount} inputs so far ...", sentCount);
            }
        }

        var responses = await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Sent {TaskCount} sensor inputs for {SensorBoxCount} sensors boxes. {SuccessfulRequestCount} tasks completed successfully, {FailedRequestCount} tasks failed",
            tasks.Count(),
            sensorBoxes.Count,
            responses.Count(r => r.IsSuccessStatusCode),
            responses.Count(r => !r.IsSuccessStatusCode));
    }

    private static double CreateSensorValue(SensorType sensorType, Random random)
    {
        var x = random.NextDouble();
        var (min, max) = Ranges[sensorType];
        return min + x * (max - min);
    }
}

internal class SensorBox
{
    internal string Id { get; init; } = null!;
        
    internal DateTimeOffset LastSend { get; set; }
}