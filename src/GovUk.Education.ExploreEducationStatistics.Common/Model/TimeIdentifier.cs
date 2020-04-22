using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodLabelFormat;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifierCategory;
using Category = GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifierCategory;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum TimeIdentifier
    {
        [TimeIdentifierMeta("Academic Year", "AY", Category.AcademicYear, AcademicOrFiscalNoLabel)]
        AcademicYear,

        [TimeIdentifierMeta("Academic Year Q1", "AYQ1", Category.AcademicYear, AcademicOrFiscalShort, "Q1")]
        AcademicYearQ1,

        [TimeIdentifierMeta("Academic Year Q2", "AYQ2", Category.AcademicYear, AcademicOrFiscalShort, "Q2")]
        AcademicYearQ2,

        [TimeIdentifierMeta("Academic Year Q3", "AYQ3", Category.AcademicYear, AcademicOrFiscalShort, "Q3")]
        AcademicYearQ3,

        [TimeIdentifierMeta("Academic Year Q4", "AYQ4", Category.AcademicYear, AcademicOrFiscalShort, "Q4")]
        AcademicYearQ4,

        [TimeIdentifierMeta("Calendar Year", "CY", Category.CalendarYear, NoLabel)]
        CalendarYear,

        [TimeIdentifierMeta("Calendar Year Q1", "CYQ1", Category.CalendarYear, Short, "Q1")]
        CalendarYearQ1,

        [TimeIdentifierMeta("Calendar Year Q2", "CYQ2", Category.CalendarYear, Short, "Q2")]
        CalendarYearQ2,

        [TimeIdentifierMeta("Calendar Year Q3", "CYQ3", Category.CalendarYear, Short, "Q3")]
        CalendarYearQ3,

        [TimeIdentifierMeta("Calendar Year Q3-Q4", "CYQ3Q4", Category.CalendarYear, Short, "Q3-Q4")]
        CalendarYearQ3Q4,

        [TimeIdentifierMeta("Calendar Year Q4", "CYQ4", Category.CalendarYear, Short, "Q4")]
        CalendarYearQ4,

        [TimeIdentifierMeta("Financial Year", "FY", Category.FinancialYear, AcademicOrFiscalNoLabel)]
        FinancialYear,

        [TimeIdentifierMeta("Financial Year Q1", "FYQ1", Category.FinancialYear, AcademicOrFiscalShort, "Q1")]
        FinancialYearQ1,

        [TimeIdentifierMeta("Financial Year Q2", "FYQ2", Category.FinancialYear, AcademicOrFiscalShort, "Q2")]
        FinancialYearQ2,

        [TimeIdentifierMeta("Financial Year Q3", "FYQ3", Category.FinancialYear, AcademicOrFiscalShort, "Q3")]
        FinancialYearQ3,

        [TimeIdentifierMeta("Financial Year Q4", "FYQ4", Category.FinancialYear, AcademicOrFiscalShort, "Q4")]
        FinancialYearQ4,

        [TimeIdentifierMeta("Tax Year", "TY", Category.TaxYear, AcademicOrFiscalNoLabel)]
        TaxYear,

        [TimeIdentifierMeta("Tax Year Q1", "TYQ1", Category.TaxYear, AcademicOrFiscalShort, "Q1")]
        TaxYearQ1,

        [TimeIdentifierMeta("Tax Year Q2", "TYQ2", Category.TaxYear, AcademicOrFiscalShort, "Q2")]
        TaxYearQ2,

        [TimeIdentifierMeta("Tax Year Q3", "TYQ3", Category.TaxYear, AcademicOrFiscalShort, "Q3")]
        TaxYearQ3,

        [TimeIdentifierMeta("Tax Year Q4", "TYQ4", Category.TaxYear, AcademicOrFiscalShort, "Q4")]
        TaxYearQ4,

        [TimeIdentifierMeta("Reporting Year", "RY", Category.ReportingYear, NoLabel)]
        ReportingYear,

        [TimeIdentifierMeta("Autumn Term", "T1", Term, AcademicOrFiscal)]
        AutumnTerm,

        [TimeIdentifierMeta("Autumn and Spring Term", "T1T2", Term, AcademicOrFiscal)]
        AutumnSpringTerm,

        [TimeIdentifierMeta("Spring Term", "T2", Term, AcademicOrFiscal)]
        SpringTerm,

        [TimeIdentifierMeta("Summer Term", "T3", Term, AcademicOrFiscal)]
        SummerTerm,

        [TimeIdentifierMeta("January", "M1", Month)]
        January,

        [TimeIdentifierMeta("February", "M2", Month)]
        February,

        [TimeIdentifierMeta("March", "M3", Month)]
        March,

        [TimeIdentifierMeta("April", "M4", Month)]
        April,

        [TimeIdentifierMeta("May", "M5", Month)]
        May,

        [TimeIdentifierMeta("June", "M6", Month)]
        June,

        [TimeIdentifierMeta("July", "M7", Month)]
        July,

        [TimeIdentifierMeta("August", "M8", Month)]
        August,

        [TimeIdentifierMeta("September", "M9", Month)]
        September,

        [TimeIdentifierMeta("October", "M10", Month)]
        October,

        [TimeIdentifierMeta("November", "M11", Month)]
        November,

        [TimeIdentifierMeta("December", "M12", Month)]
        December
    }
}