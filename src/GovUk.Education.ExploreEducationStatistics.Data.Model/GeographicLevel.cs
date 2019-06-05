using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public enum GeographicLevel
    {
        [EnumLabelValue("Local Authority", "LA")]
        Local_Authority,

        [EnumLabelValue("Local Authority District", "LAD")]
        Local_Authority_District,

        [EnumLabelValue("Local Enterprise Partnerships", "LEP")]
        Local_Enterprise_Partnerships,

        [EnumLabelValue("Institution", "INS")]
        Institution,
        
        [EnumLabelValue("MAT Or Sponsor", "MATS")]
        MAT_Or_Sponsor,

        [EnumLabelValue("Mayoral Combined Authorities", "MCA")]
        Mayoral_Combined_Authorities,

        [EnumLabelValue("National", "NAT")]
        National,

        [EnumLabelValue("Opportunity Areas", "OA")]
        Opportunity_Areas,

        [EnumLabelValue("Parliamentary Constituency", "PC")]
        Parliamentary_Constituency,

        [EnumLabelValue("Provider", "PRO")]
        Provider,

        [EnumLabelValue("Regional", "REG")]
        Regional,

        [EnumLabelValue("RSC Region", "RSCR")]
        RSC_Region,

        [EnumLabelValue("School", "SCH")]
        School,

        [EnumLabelValue("Ward", "WAR")]
        Ward
    }
}