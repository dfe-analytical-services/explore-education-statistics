using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public enum Unit
    {
        [EnumLabelValue("", "")]
        Number,
        
        [EnumLabelValue("%", "%")]
        Percent,
        
        [EnumLabelValue("£", "£")]
        Pound
    }
}