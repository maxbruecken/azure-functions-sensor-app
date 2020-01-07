using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Repositories;
using AzureFunction.Core.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunction.App
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddLogging(c => c.AddConsole())
                .AddScoped<ISensorRepository, SensorRepository>()
                .AddScoped<ISensorInputService, SensorInputService>()
                .AddScoped<ISensorValidationService, SensorValidationService>();
        }
    }
}
