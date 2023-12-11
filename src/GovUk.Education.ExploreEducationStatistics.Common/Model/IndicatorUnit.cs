using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum IndicatorUnit
    {
        [EnumLabelValue("", "")]
        Number,

        [EnumLabelValue("%", "%")]
        Percent,

        [EnumLabelValue("£", "£")]
        Pound,

        [EnumLabelValue("£m", "£m")]
        MillionPounds,

        [EnumLabelValue("pp", "pp")]
        PercentagePoint,
    }
}
