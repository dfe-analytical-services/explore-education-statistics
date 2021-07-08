using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum FileType
    {
        [EnumLabelValue("ancillary")]
        Ancillary,
        [EnumLabelValue("chart")]
        Chart,
        [EnumLabelValue("data")]
        Data,
        [EnumLabelValue("zip")]
        DataZip,
        [EnumLabelValue("image")]
        Image,
        [EnumLabelValue("metadata")]
        Metadata,
        [EnumLabelValue("data-guidance")]
        DataGuidance
    }
}