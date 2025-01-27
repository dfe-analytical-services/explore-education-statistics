using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public enum TimeIdentifierCategory
    {
        [EnumLabelValue("Academic year")]
        AcademicYear,

        [EnumLabelValue("Calendar year")]
        CalendarYear,

        [EnumLabelValue("Financial year")]
        FinancialYear,

        [EnumLabelValue("Tax year")]
        TaxYear,

        [EnumLabelValue("Reporting year")]
        ReportingYear,

        [EnumLabelValue("Term")]
        Term,

        [EnumLabelValue("Month")]
        Month,

        [EnumLabelValue("Week")]
        Week,

        [EnumLabelValue("Financial year part")]
        FinancialYearPart,
    }
}
