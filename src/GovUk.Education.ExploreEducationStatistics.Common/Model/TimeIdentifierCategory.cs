using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum TimeIdentifierCategory
    {
        [EnumLabelValue("Academic year")]
        AcademicYear,

        [EnumLabelValue("Calendar year")]
        CalendarYear,

        [EnumLabelValue("Financial Year")]
        FinancialYear,

        [EnumLabelValue("Tax Year")]
        TaxYear,

        [EnumLabelValue("Term")]
        Term,
        
        [EnumLabelValue("Month")]
        Month,

        [EnumLabelValue("Other")]
        Other
    }
}