using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum ReleaseFileTypes
    {
        [EnumLabelValue("data")]
        Data,
        [EnumLabelValue("zip")]
        DataZip,
        [EnumLabelValue("metadata")]
        Metadata,
        [EnumLabelValue("ancillary")]
        Ancillary,
        [EnumLabelValue("chart")]
        Chart
    }
}