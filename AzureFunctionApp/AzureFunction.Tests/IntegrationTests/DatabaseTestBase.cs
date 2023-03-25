using System;
using System.Threading.Tasks;
using AzureFunction.Core.DbContext;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureFunction.Tests.IntegrationTests;

[Collection("Database tests")]
public abstract class DatabaseTestBase
{
    private readonly SensorAppContextProvider ContextProvider;

    protected DatabaseTestBase(SensorAppContextProvider contextProvider)
    {
        ContextProvider = contextProvider;
        using var context = contextProvider.GetContext();
        context.Database.EnsureDeleted();
        context.Database.Migrate();
    }

    protected async Task InvokeOperationOnFreshContext(Func<SensorAppContext, Task> operation)
    {
        await using var context = ContextProvider.GetContext();
        await operation(context);
    }
}