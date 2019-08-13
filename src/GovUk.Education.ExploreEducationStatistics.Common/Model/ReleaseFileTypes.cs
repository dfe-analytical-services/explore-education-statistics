using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum ReleaseFileTypes
    {
        [EnumLabelValue("data")]
        Data,
        [EnumLabelValue("ancillary")]
        Ancillary,
        [EnumLabelValue("chart")]
        Chart
    }
}