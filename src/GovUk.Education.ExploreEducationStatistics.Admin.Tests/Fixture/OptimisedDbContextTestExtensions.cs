using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public static class OptimisedDbContextTestExtensions
{
    public static async Task AddTestData<TDbContext>(
        this TDbContext context,
        Action<TDbContext> supplier
    )
        where TDbContext : DbContext
    {
        try
        {
            supplier.Invoke(context);
            await context.SaveChangesAsync();
        }
        finally
        {
            // As this DbContext is being reused to set up test data for
            // various tests within a collection / class fixture scope,
            // ensure that we don't track any entities between tests within
            // this context.
            context.ChangeTracker.Clear();
        }
    }
}
