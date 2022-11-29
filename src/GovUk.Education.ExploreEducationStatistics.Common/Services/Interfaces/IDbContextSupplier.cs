using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

/// <summary>
/// DI Component responsible for supplying dependant services with DbContexts.
/// </summary>
public interface IDbContextSupplier
{
    TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext;
    
    TDbContext CreateDbContextDelegate<TDbContext>() where TDbContext : DbContext;
}