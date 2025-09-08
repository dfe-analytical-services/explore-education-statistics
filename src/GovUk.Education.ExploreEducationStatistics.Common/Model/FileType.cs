using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public enum FileType
{
    [EnumLabelValue("ancillary")]
    Ancillary,

    [EnumLabelValue("zip")]
    AllFilesZip,

    [EnumLabelValue("chart")]
    Chart,

    [EnumLabelValue("data")]
    Data,

    [EnumLabelValue("data-zip")]
    DataZip,

    [EnumLabelValue("bulk-data-zip")]
    BulkDataZip,

    [EnumLabelValue("bulk-data-zip-index")]
    BulkDataZipIndex,

    [EnumLabelValue("image")]
    Image,

    [EnumLabelValue("metadata")]
    Metadata,

    [EnumLabelValue("data-guidance")]
    DataGuidance,
}
