#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class DbContextTestExtensions
{
    public static async Task ClearTestData<TDbContext>(this TDbContext context) where TDbContext : DbContext
    {
        if (context.Database.IsNpgsql())
        {
#pragma warning disable EF1002
            var tables = context.Model.GetEntityTypes()
                .Select(type => type.GetTableName())
                .OfType<string>()
                .Distinct()
                .ToList();

            var schemaName = context.Model
                .GetEntityTypes()
                .Select(e => e.GetSchema())
                .First(s => !string.IsNullOrEmpty(s));
            
            foreach (var table in tables)
            {
                await context.Database.ExecuteSqlRawAsync($$"""TRUNCATE TABLE "{{schemaName}}"."{{table}}" RESTART IDENTITY CASCADE;""");
            }

            var sequences = context.Model.GetSequences();

            foreach (var sequence in sequences)
            {
                await context.Database.ExecuteSqlRawAsync($$"""ALTER SEQUENCE "{{schemaName}}"."{{sequence.Name}}" RESTART WITH 1;""");
            }
#pragma warning restore EF1002
            
            context.ChangeTracker.Clear();
        }
        else
        {
            throw new NotImplementedException(
                $"Clearing test data is not supported for type {context.Database.ProviderName}");
        }
    }
}
