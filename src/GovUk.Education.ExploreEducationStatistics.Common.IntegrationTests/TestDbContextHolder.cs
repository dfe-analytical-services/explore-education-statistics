using Microsoft.EntityFrameworkCore;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

/// <summary>
/// A convenience class for handling multiple DbContexts for the purposes of setting up test data and asserting
/// database updates during testing.
///
/// This class provides methods for handling lifecycle management of the DbContexts.
/// </summary>
public class TestDbContextHolder(DbContext[] dbContexts)
{
    public TDbContext GetDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        return (TDbContext)dbContexts.Single(context => context.GetType() == typeof(TDbContext));
    }

    public async Task ClearAllTestData()
    {
        foreach (var dbContext in dbContexts)
        {
            await dbContext.ClearTestData();
        }
    }

    public async Task ClearInMemoryTestData()
    {
        foreach (var dbContext in dbContexts)
        {
            await dbContext.ClearTestDataIfInMemory();
        }
    }

    public void ResetAnyMocks()
    {
        foreach (var dbContext in dbContexts)
        {
            dbContext.ResetIfMock();
        }
    }

    public async Task DisposeAll()
    {
        foreach (var dbContext in dbContexts)
        {
            await dbContext.DisposeAsync();
        }
    }
}

internal static class DbContextTestExtensions
{
    public static async Task ClearTestData<TDbContext>(this TDbContext context)
        where TDbContext : DbContext
    {
        if (context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
#pragma warning disable EF1002
            var tables = context
                .Model.GetEntityTypes()
                .Select(type => type.GetTableName())
                .OfType<string>()
                .Distinct()
                .ToList();

            foreach (var table in tables)
            {
                await context.Database.ExecuteSqlRawAsync($"""TRUNCATE TABLE "{table}" RESTART IDENTITY CASCADE;""");
            }

            var sequences = context.Model.GetSequences();

            foreach (var sequence in sequences)
            {
                await context.Database.ExecuteSqlRawAsync($"""ALTER SEQUENCE "{sequence.Name}" RESTART WITH 1;""");
            }
#pragma warning restore EF1002
        }
        else if (context.Database.IsInMemory())
        {
            await context.ClearTestDataIfInMemory();
        }
        else
        {
            throw new NotImplementedException(
                $"Clearing test data is not supported for type {context.Database.ProviderName}"
            );
        }
    }

    public static async Task ClearTestDataIfInMemory<TDbContext>(this TDbContext? context)
        where TDbContext : DbContext
    {
        // If a DbContext's Database property is null, it is most likely a Mock.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (context == null || context.Database == null)
        {
            return;
        }

        if (context.Database.IsInMemory())
        {
            await context.Database.EnsureDeletedAsync();
            context.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Resets a mock for a given service if the service has been mocked.
    ///
    /// We support this not necessarily being a mock because a fixture subclass may have chosen to inject a real
    /// service in place of a service that is generally mocked out.
    /// </summary>
    public static void ResetIfMock<TDbContext>(this TDbContext context)
        where TDbContext : DbContext
    {
        try
        {
            var mock = Mock.Get(context);
            mock.Reset();
        }
        catch
        {
            // "service" is not a Mock. This is fine.
        }
    }
}
