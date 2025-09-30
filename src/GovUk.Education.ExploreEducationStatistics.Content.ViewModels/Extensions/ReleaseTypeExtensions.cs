using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

public static class ReleaseTypeExtensions
{
    public static int ToSearchDocumentTypeBoost(this ReleaseType releaseType) => releaseType switch
    {
        ReleaseType.AccreditedOfficialStatistics => 6,
        ReleaseType.OfficialStatistics => 5,
        ReleaseType.OfficialStatisticsInDevelopment => 4,
        ReleaseType.ExperimentalStatistics => 3,
        ReleaseType.AdHocStatistics => 2,
        ReleaseType.ManagementInformation => 1,
        _ => throw new ArgumentOutOfRangeException()
    };
}
