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
            var tables = context.Model.GetEntityTypes()
                .Select(type => type.GetTableName())
                .Distinct()
                .ToList();

            foreach (var table in tables)
            {
#pragma warning disable EF1002
                await context.Database.ExecuteSqlRawAsync($"""TRUNCATE TABLE "{table}" RESTART IDENTITY CASCADE;""");
#pragma warning restore EF1002
            }
        }
        else
        {
            throw new NotImplementedException(
                $"Clearing test data is not supported for type {context.Database.ProviderName}");
        }
    }
}
