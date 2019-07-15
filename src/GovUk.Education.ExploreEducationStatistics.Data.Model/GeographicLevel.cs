using GovUk.Education.ExploreEducationStatistics.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public enum GeographicLevel
    {
        [EnumLabelValue("Local Authority", "LA")]
        Local_Authority,

        [EnumLabelValue("Local Authority District", "LAD")]
        Local_Authority_District,

        [EnumLabelValue("Local Enterprise Partnership", "LEP")]
        Local_Enterprise_Partnership,

        [EnumLabelValue("Institution", "INS")] Institution,

        [EnumLabelValue("Mayoral Combined Authority", "MCA")]
        Mayoral_Combined_Authority,

        [EnumLabelValue("Multi Academy Trust", "MAT")]
        Multi_Academy_Trust,

        [EnumLabelValue("Country", "NAT")] Country,

        [EnumLabelValue("Opportunity Area", "OA")]
        Opportunity_Area,

        [EnumLabelValue("Parliamentary Constituency", "PC")]
        Parliamentary_Constituency,

        [EnumLabelValue("Provider", "PRO")] Provider,

        [EnumLabelValue("Regional", "REG")] Region,

        [EnumLabelValue("RSC Region", "RSCR")] RSC_Region,

        [EnumLabelValue("School", "SCH")] School,

        [EnumLabelValue("Sponsor", "SPO")] Sponsor,

        [EnumLabelValue("Ward", "WAR")] Ward
    }
}