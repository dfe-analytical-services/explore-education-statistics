#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

/// <summary>
/// Component for supplying services with DbContexts when testing with InMemory DbContexts.
/// </summary>
public class InMemoryDbContextSupplier : IDbContextSupplier
{
    private readonly string? _contentDbContextId;
    private readonly string? _statisticsDbContextId;

    public InMemoryDbContextSupplier(
        string? contentDbContextId = null, 
        string? statisticsDbContextId = null)
    {
        _contentDbContextId = contentDbContextId;
        _statisticsDbContextId = statisticsDbContextId;
    }

    public ContentDbContext CreateContentDbContext()
    {
        return ContentDbUtils.InMemoryContentDbContext(_contentDbContextId);
    }

    public StatisticsDbContext CreateStatisticsDbContext()
    {
        return StatisticsDbUtils.InMemoryStatisticsDbContext(_statisticsDbContextId);
    }
}