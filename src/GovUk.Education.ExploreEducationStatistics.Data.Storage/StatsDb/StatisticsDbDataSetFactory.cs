#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

public class StatisticsDbDataSetFactory(IServiceProvider serviceProvider)
{
    public StatisticsDbDataSet Create(Guid subjectId)
    {
        return ActivatorUtilities.CreateInstance<StatisticsDbDataSet>(serviceProvider, subjectId);
    }
}
