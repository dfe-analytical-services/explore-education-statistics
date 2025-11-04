using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

public enum GeographicLevel
{
    [EnumLabelValue("English devolved area", "EDA")]
    EnglishDevolvedArea,

    [EnumLabelValue("Local authority", "LA")]
    LocalAuthority,

    [EnumLabelValue("Local authority district", "LAD")]
    LocalAuthorityDistrict,

    [EnumLabelValue("Local enterprise partnership", "LEP")]
    LocalEnterprisePartnership,

    [EnumLabelValue("Local skills improvement plan area", "LSIP")]
    LocalSkillsImprovementPlanArea,

    [EnumLabelValue("Institution", "INST")]
    Institution,

    [EnumLabelValue("Mayoral combined authority", "MCA")]
    MayoralCombinedAuthority,

    [EnumLabelValue("MAT", "MAT")]
    MultiAcademyTrust,

    [EnumLabelValue("National", "NAT")]
    Country,

    [EnumLabelValue("Opportunity area", "OA")]
    OpportunityArea,

    [EnumLabelValue("Parliamentary constituency", "PCON")]
    ParliamentaryConstituency,

    [EnumLabelValue("Provider", "PROV")]
    Provider,

    [EnumLabelValue("Regional", "REG")]
    Region,

    // Regional School Commissioner Region
    [EnumLabelValue("RSC region", "RSC")]
    RscRegion,

    [EnumLabelValue("School", "SCH")]
    School,

    [EnumLabelValue("Sponsor", "SPON")]
    Sponsor,

    [EnumLabelValue("Ward", "WARD")]
    Ward,

    [EnumLabelValue("Planning area", "PA")]
    PlanningArea,
}
