using GovUk.Education.ExploreEducationStatistics.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public enum Unit
    {
        [EnumLabelValue("", "")]
        Number,
        
        [EnumLabelValue("%", "%")]
        Percent
    }
}