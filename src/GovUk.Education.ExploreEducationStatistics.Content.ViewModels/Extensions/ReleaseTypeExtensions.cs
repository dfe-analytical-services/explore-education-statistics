using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

public static class ReleaseTypeExtensions
{
    public static string ToDisplayString(this ReleaseType releaseType) => releaseType switch
    {
        ReleaseType.AdHocStatistics => "Ad Hoc Statistics",
        ReleaseType.AccreditedOfficialStatistics => "Accredited Official Statistics",
        ReleaseType.ExperimentalStatistics => "Experimental Statistics",
        ReleaseType.ManagementInformation => "Management Information",
        ReleaseType.OfficialStatistics => "Official Statistics",
        ReleaseType.OfficialStatisticsInDevelopment => "Official Statistics In Development",
        _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
    };

    public static int ToTypeBoost(this ReleaseType releaseType) => releaseType switch
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
