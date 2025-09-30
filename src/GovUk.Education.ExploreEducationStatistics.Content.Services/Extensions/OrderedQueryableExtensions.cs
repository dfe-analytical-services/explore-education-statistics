using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;

public static class OrderedQueryableExtensions
{
    public static IOrderedQueryable<FreeTextValueResult<Publication>> ThenByReleaseType(
        this IOrderedQueryable<FreeTextValueResult<Publication>> orderedQueryable
    )
    {
        return orderedQueryable.ThenBy(publication =>
            publication.Value.LatestPublishedReleaseVersion!.Type
            == ReleaseType.AccreditedOfficialStatistics
                ? 0
            : publication.Value.LatestPublishedReleaseVersion!.Type
            == ReleaseType.OfficialStatistics
                ? 1
            : publication.Value.LatestPublishedReleaseVersion!.Type
            == ReleaseType.OfficialStatisticsInDevelopment
                ? 2
            : publication.Value.LatestPublishedReleaseVersion!.Type
            == ReleaseType.ExperimentalStatistics
                ? 3
            : publication.Value.LatestPublishedReleaseVersion!.Type == ReleaseType.AdHocStatistics
                ? 4
            : 5
        );
    }
}
