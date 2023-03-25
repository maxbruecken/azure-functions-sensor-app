using System.Threading.Tasks;
using AzureFunction.Core.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Start;

public class Program
{
    public static async Task Main(params string[] args)
    {
        await CreateHostBuilder(args).RunConsoleAsync();
    }

    // ReSharper disable once MemberCanBePrivate.Global : will be used by Entity Framework at design time
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(builder => builder.AddConsole())
            .ConfigureServices(services =>
            {
                services
                    .AddDbContextFactory<SensorAppContext>((provider, builder) =>
                    {
                        builder.UseNpgsql(provider.GetRequiredService<IConfiguration>().GetConnectionString("SensorAppDatabase"));
                    })
                    .AddHostedService<SensorDataCreator>();
            });
    }
}