#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage;

public class StatisticsDbDataSetFactory(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Creates a <see cref="StatisticsDbDataSet" /> bound to <paramref name="subjectId" />.
    /// The Subject is not checked for existence.
    /// </summary>
    public StatisticsDbDataSet Create(Guid subjectId)
    {
        return ActivatorUtilities.CreateInstance<StatisticsDbDataSet>(serviceProvider, subjectId);
    }
}
