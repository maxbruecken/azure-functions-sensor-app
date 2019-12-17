using AzureFunctionApp;
using AzureFunctionCore.Interfaces;
using AzureFunctionCore.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddLogging(c => c.AddConsole())
                .AddScoped<ISensorInputService, SensorInputService>();
        }
    }
}
