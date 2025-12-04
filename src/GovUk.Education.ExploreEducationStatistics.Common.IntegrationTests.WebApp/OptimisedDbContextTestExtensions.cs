using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

public static class OptimisedDbContextTestExtensions
{
    /// <summary>
    /// A method to add test data to a given DbContext. This should be used in conjunction with a reusable
    /// DbContext from an appropriate Collection-level test fixture where possible to reduce the number
    /// of DbContext lookups (and therefore re-instantiations) of the owning
    /// <see cref="WebApplicationFactory{TEntryPoint}"/>.
    /// </summary>
    public static async Task AddTestData<TDbContext>(this TDbContext context, Action<TDbContext> supplier)
        where TDbContext : DbContext
    {
        try
        {
            supplier.Invoke(context);
            await context.SaveChangesAsync();
        }
        finally
        {
            // As this DbContext is ideally being reused to set up test data for various tests within a
            // Collection, ensure that we don't track any entities between tests within this context, or
            // they will "leak" into the next test.
            context.ChangeTracker.Clear();
        }
    }
}
