using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
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