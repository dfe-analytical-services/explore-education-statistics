using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum ReleaseFileTypes
    {
        [EnumLabelValue("Data")]
        Data,
        [EnumLabelValue("Ancillary")]
        Ancillary,
        [EnumLabelValue("Chart")]
        Chart
    }
}