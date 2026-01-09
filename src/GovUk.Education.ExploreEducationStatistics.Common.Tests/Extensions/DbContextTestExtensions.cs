#nullable enable
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

// TODO EES-6450 - remove when all integration tests are migrated to using the new optimised framework.
public static class DbContextTestExtensions
{
    public static async Task ClearTestData<TDbContext>(this TDbContext context)
        where TDbContext : DbContext
    {
        if (context.Database.IsNpgsql())
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
}
