using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

/// <summary>
/// DI Component responsible for supplying dependant services with DbContexts.
/// </summary>
public interface IDbContextSupplier
{
    ContentDbContext CreateContentDbContext();
    
    StatisticsDbContext CreateStatisticsDbContext();
}