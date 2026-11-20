#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

public class StatisticsDbDataSetWriterFactory(IServiceProvider serviceProvider)
{
    public StatisticsDbDataSetWriter Create(Guid subjectId)
    {
        return ActivatorUtilities.CreateInstance<StatisticsDbDataSetWriter>(serviceProvider, subjectId);
    }
}
