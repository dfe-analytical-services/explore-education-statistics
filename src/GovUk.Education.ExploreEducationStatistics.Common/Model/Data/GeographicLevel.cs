using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public enum GeographicLevel
    {
        [EnumLabelValue("English Devolved Area", "EDA")]
        EnglishDevolvedArea,

        [EnumLabelValue("Local Authority", "LA")]
        LocalAuthority,

        [EnumLabelValue("Local Authority District", "LAD")]
        LocalAuthorityDistrict,

        [EnumLabelValue("Local Enterprise Partnership", "LEP")]
        LocalEnterprisePartnership,

        [EnumLabelValue("Institution", "INS")] Institution,

        [EnumLabelValue("Mayoral Combined Authority", "MCA")]
        MayoralCombinedAuthority,

        [EnumLabelValue("MAT", "MAT")] MultiAcademyTrust,

        [EnumLabelValue("National", "NAT")] Country,

        [EnumLabelValue("Opportunity Area", "OA")]
        OpportunityArea,

        [EnumLabelValue("Parliamentary Constituency", "PC")]
        ParliamentaryConstituency,

        [EnumLabelValue("Provider", "PRO")] Provider,

        [EnumLabelValue("Regional", "REG")] Region,

        // Regional School Commissioner Region
        [EnumLabelValue("RSC Region", "RSCR")] RscRegion,

        [EnumLabelValue("School", "SCH")] School,

        [EnumLabelValue("Sponsor", "SPO")] Sponsor,

        [EnumLabelValue("Ward", "WAR")] Ward,
        
        [EnumLabelValue("Planning Area", "PA")] PlanningArea
    }
}
