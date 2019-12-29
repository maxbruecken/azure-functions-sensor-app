using AzureFunction.App;
using AzureFunction.App.Authentication;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Repositories;
using AzureFunction.Core.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunction.App
{
    public class Startup : FunctionsStartup, IWebJobsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddLogging(c => c.AddConsole())
                .AddScoped<ISensorRepository, SensorRepository>()
                .AddScoped<ISensorInputService, SensorInputService>();
        }

        void IWebJobsStartup.Configure(IWebJobsBuilder builder)
        {
            Configure(builder);
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            builder.AddExtension(new PrincipalExtensionProvider(configuration));
        }
    }
}
