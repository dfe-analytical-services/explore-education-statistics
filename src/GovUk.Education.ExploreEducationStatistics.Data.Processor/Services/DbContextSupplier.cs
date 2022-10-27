using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DbContextSupplier : IDbContextSupplier
{
    private readonly DbContextOptions<ContentDbContext> _contentDbContextOptions;

    public DbContextSupplier(DbContextOptions<ContentDbContext> contentDbContextOptions)
    {
        _contentDbContextOptions = contentDbContextOptions;
    }

    public ContentDbContext CreateContentDbContext()
    {
        return new ContentDbContext(_contentDbContextOptions);
    }

    public StatisticsDbContext CreateStatisticsDbContext()
    {
        return DbUtils.CreateStatisticsDbContext();
    }
}