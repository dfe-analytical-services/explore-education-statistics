using Microsoft.EntityFrameworkCore;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface ITemporaryTableCreator
{
    Task<ITempTableReference> CreateTemporaryTable<TEntity, TDbContext>(
        TDbContext context,
        CancellationToken cancellationToken
    )
        where TEntity : class
        where TDbContext : DbContext;

    Task<ITempTableQuery<TEntity>> CreateAndPopulateTemporaryTable<TEntity, TDbContext>(
        TDbContext context,
        IEnumerable<TEntity> values,
        CancellationToken cancellationToken
    )
        where TEntity : class
        where TDbContext : DbContext;
}
