using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class DbContextTestExtensions
{
    public static void ClearTestData<TDbContext>(this TDbContext context) where TDbContext : DbContext
    {
        var tables = context.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Distinct()
            .ToList();

        foreach (var table in tables)
        {
#pragma warning disable EF1002
            context.Database.ExecuteSqlRaw($@"TRUNCATE TABLE ""{table}"" RESTART IDENTITY CASCADE;");
#pragma warning restore EF1002
        }
    }
}
