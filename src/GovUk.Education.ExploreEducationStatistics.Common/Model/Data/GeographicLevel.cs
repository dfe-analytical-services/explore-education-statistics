using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
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

        [EnumLabelValue("Institution", "INS")] Institution,

        [EnumLabelValue("Mayoral combined authority", "MCA")]
        MayoralCombinedAuthority,

        [EnumLabelValue("MAT", "MAT")] MultiAcademyTrust,

        [EnumLabelValue("National", "NAT")] Country,

        [EnumLabelValue("Opportunity area", "OA")]
        OpportunityArea,

        [EnumLabelValue("Parliamentary constituency", "PC")]
        ParliamentaryConstituency,

        [EnumLabelValue("Provider", "PRO")] Provider,

        [EnumLabelValue("Regional", "REG")] Region,

        // Regional School Commissioner Region
        [EnumLabelValue("RSC region", "RSCR")] RscRegion,

        [EnumLabelValue("School", "SCH")] School,

        [EnumLabelValue("Sponsor", "SPO")] Sponsor,

        [EnumLabelValue("Ward", "WAR")] Ward,
        
        [EnumLabelValue("Planning area", "PA")] PlanningArea
    }
}
