using System.Diagnostics.CodeAnalysis;
using AzureFunction.Core.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace AzureFunction.Tests;

// ReSharper disable once UnusedType.Global
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("Performance", "CA1822")]
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration((_, builder) => builder.AddEnvironmentVariables().AddJsonFile("appsettings.json"));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddDbContextFactory<SensorAppContext>((p, b) =>
            {
                b.UseNpgsql(p.GetRequiredService<IConfiguration>().GetConnectionString("SensorAppDatabase"));
            })
            .AddSingleton<SensorAppContextProvider>();
    }
    
    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor)
    {
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
    }
}

public class SensorAppContextProvider
{
    private readonly IDbContextFactory<SensorAppContext> _dbContextFactory;

    public SensorAppContextProvider(IDbContextFactory<SensorAppContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public SensorAppContext GetContext()
    {
        return _dbContextFactory.CreateDbContext();
    }
}