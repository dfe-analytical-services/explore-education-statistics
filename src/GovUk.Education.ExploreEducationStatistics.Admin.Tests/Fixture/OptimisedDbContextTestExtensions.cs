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
            context.ChangeTracker.Clear();
        }
    }
}
