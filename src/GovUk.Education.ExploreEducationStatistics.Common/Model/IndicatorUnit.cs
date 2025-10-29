using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public enum IndicatorUnit
{
    [EnumLabelValue("", "")]
    None,

    [EnumLabelValue("%", "%")]
    Percent,

    [EnumLabelValue("£", "£")]
    Pound,

    [EnumLabelValue("£m", "£m")]
    MillionPounds,

    [EnumLabelValue("pp", "pp")]
    PercentagePoint,

    // We named this `numberstring` in EES-5478, but this is
    // likely to be confusing for end users who can see this unit
    // (e.g. in the public API).
    // It's really just saying 'this is a string without any numeric
    // formatting' so `string` is a more appropriate name.
    // To compromise, we'll display `string` to end users and persist
    // as `numberstring` internally (which analysts will publish with).
    [EnumLabelValue("string", "numberstring")]
    String,
}
