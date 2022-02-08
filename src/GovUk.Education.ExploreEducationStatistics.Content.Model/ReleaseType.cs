using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public enum ReleaseType
    {
        [EnumLabelValue("Ad Hoc Statistics")]
        AdHocStatistics,

        [EnumLabelValue("National Statistics")]
        NationalStatistics,

        [EnumLabelValue("Experimental Statistics")]
        ExperimentalStatistics,

        [EnumLabelValue("Management Information")]
        ManagementInformation,

        [EnumLabelValue("Official Statistics")]
        OfficialStatistics,
    }

    /// TODO EES-3127 Titles can be removed after removing the backwards compatibility of
    /// CachedReleaseViewModel.Type in cached Release content. This will require a content cache refresh.
    public static class ReleaseTypeExtensions
    {
        public static string GetTitle(this ReleaseType releaseType)
        {
            return releaseType.GetEnumLabel();
        }
    }
}
