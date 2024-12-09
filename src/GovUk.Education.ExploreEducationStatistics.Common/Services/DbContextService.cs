using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DbContextService : IDbContextService
{
    public DbContextService()
    {
    }

    // Created to be mocked by tests for cases when the in-memory db doesn't emulate the real
    // db, e.g. with EF8 JSON columns
    public async Task SaveChangesAsync(DbContext context)
    {
        await context.SaveChangesAsync();
    }
}
