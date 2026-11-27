#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

public class StatisticsDbDataSetWriterFactory(
    IServiceProvider serviceProvider,
    StatisticsDbDataSetFactory statisticsDbDataSetFactory
)
{
    public StatisticsDbDataSetWriter Create(Guid subjectId)
    {
        return ActivatorUtilities.CreateInstance<StatisticsDbDataSetWriter>(
            serviceProvider,
            statisticsDbDataSetFactory.Create(subjectId)
        );
    }
}
