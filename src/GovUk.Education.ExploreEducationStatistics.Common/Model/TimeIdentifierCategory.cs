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

        // EES-3959 Temporary category for the National tutoring programme publication
        // TODO remove this once we've got a solution for ordering releases which mix categories of time periods
        [EnumLabelValue("National tutoring programme")]
        NationalTutoringProgramme,
    }
}
