using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace AzureFunction.Start
{
    class Program
    {
        static async Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", false)
                .Build();
            
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
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken) }
            };

            var uri = new Uri($"{configuration["ApplicationUri"]}api/SensorInput");
            var tasks = Enumerable.Repeat(new SensorInput
                {
                    SensorId = "test",
                    Values = new[] {1d, 2d, 3d}
                }, 10)
                .Select(i =>
                {
                    var content = new StringContent(JsonConvert.SerializeObject(i), Encoding.UTF8, "application/json");
                    return httpClient.PostAsync(uri, content);
                });

            await Task.WhenAll(tasks);
        }
    }
}