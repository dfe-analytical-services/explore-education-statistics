using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public enum GeographicLevel
    {
        [EnumLabel("EST")] Establishment,

        [EnumLabel("LA")] Local_Authority,

        [EnumLabel("LAD")] Local_Authority_District,

        [EnumLabel("LEP")] Local_Enterprise_Partnerships,

        [EnumLabel("MATS")] MAT_Or_Sponsor,
        
        [EnumLabel("MCA")] Mayoral_Combined_Authorities,
        
        [EnumLabel("NAT")] National,

        [EnumLabel("OA")] Opportunity_Areas,

        [EnumLabel("PC")] Parliamentary_Constituency,
        
        [EnumLabel("PRO")] Provider,

        [EnumLabel("REG")] Regional,

        [EnumLabel("RSCR")] RSC_Region,

        [EnumLabel("SCH")] School,
        
        [EnumLabel("WAR")] Ward
    }
}