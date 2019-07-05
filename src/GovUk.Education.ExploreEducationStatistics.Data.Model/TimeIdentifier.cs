using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.TimePeriodLabelFormat;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum TimeIdentifier
    {
        [TimeIdentifierLabelValue("Academic Year", "AY", AcademicOrFiscalNoLabel)]
        AcademicYear,

        [TimeIdentifierLabelValue("Academic Year Q1", "AYQ1", AcademicOrFiscalShort, "Q1")]
        AcademicYearQ1,

        [TimeIdentifierLabelValue("Academic Year Q1-Q2", "AYQ1Q2", AcademicOrFiscalShort, "Q1-Q2")]
        AcademicYearQ1Q2,

        [TimeIdentifierLabelValue("Academic Year Q1-Q3", "AYQ1Q3", AcademicOrFiscalShort, "Q1-Q3")]
        AcademicYearQ1Q3,

        [TimeIdentifierLabelValue("Academic Year Q1-Q4", "AYQ1Q4", AcademicOrFiscalShort, "Q1-Q4")]
        AcademicYearQ1Q4,

        [TimeIdentifierLabelValue("Academic Year Q2", "AYQ2", AcademicOrFiscalShort, "Q2")]
        AcademicYearQ2,

        [TimeIdentifierLabelValue("Academic Year Q2-Q3", "AYQ2Q3", AcademicOrFiscalShort, "Q2-Q3")]
        AcademicYearQ2Q3,

        [TimeIdentifierLabelValue("Academic Year Q2-Q4", "AYQ2Q4", AcademicOrFiscalShort, "Q2-Q4")]
        AcademicYearQ2Q4,

        [TimeIdentifierLabelValue("Academic Year Q3", "AYQ3", AcademicOrFiscalShort, "Q3")]
        AcademicYearQ3,

        [TimeIdentifierLabelValue("Academic Year Q3-Q4", "AYQ3Q4", AcademicOrFiscalShort, "Q3-Q4")]
        AcademicYearQ3Q4,

        [TimeIdentifierLabelValue("Academic Year Q4", "AYQ4", AcademicOrFiscalShort, "Q4")]
        AcademicYearQ4,

        [TimeIdentifierLabelValue("Calendar Year", "CY", NoLabel)]
        CalendarYear,

        [TimeIdentifierLabelValue("Calendar Year Q1", "CYQ1", Short, "Q1")]
        CalendarYearQ1,

        [TimeIdentifierLabelValue("Calendar Year Q1-Q2", "CYQ1Q2", Short, "Q1-Q2")]
        CalendarYearQ1Q2,

        [TimeIdentifierLabelValue("Calendar Year Q1-Q3", "CYQ1Q3", Short, "Q1-Q3")]
        CalendarYearQ1Q3,

        [TimeIdentifierLabelValue("Calendar Year Q1-Q4", "CYQ1Q4", Short, "Q1-Q4")]
        CalendarYearQ1Q4,

        [TimeIdentifierLabelValue("Calendar Year Q2", "CYQ2", Short, "Q2")]
        CalendarYearQ2,

        [TimeIdentifierLabelValue("Calendar Year Q2-Q3", "CYQ2Q3", Short, "Q2-Q3")]
        CalendarYearQ2Q3,

        [TimeIdentifierLabelValue("Calendar Year Q2-Q4", "CYQ2Q4", Short, "Q2-Q4")]
        CalendarYearQ2Q4,

        [TimeIdentifierLabelValue("Calendar Year Q3", "CYQ3", Short, "Q3")]
        CalendarYearQ3,

        [TimeIdentifierLabelValue("Calendar Year Q3-Q4", "CYQ3Q4", Short, "Q3-Q4")]
        CalendarYearQ3Q4,

        [TimeIdentifierLabelValue("Calendar Year Q4", "CYQ4", Short, "Q4")]
        CalendarYearQ4,

        [TimeIdentifierLabelValue("Financial Year", "FY", AcademicOrFiscalNoLabel)]
        FinancialYear,

        [TimeIdentifierLabelValue("Financial Year Q1", "FYQ1", AcademicOrFiscalShort, "Q1")]
        FinancialYearQ1,

        [TimeIdentifierLabelValue("Financial Year Q1-Q2", "FYQ1Q2", AcademicOrFiscalShort, "Q1-Q2")]
        FinancialYearQ1Q2,

        [TimeIdentifierLabelValue("Financial Year Q1-Q3", "FYQ1Q3", AcademicOrFiscalShort, "Q1-Q3")]
        FinancialYearQ1Q3,

        [TimeIdentifierLabelValue("Financial Year Q1-Q4", "FYQ1Q4", AcademicOrFiscalShort, "Q1-Q4")]
        FinancialYearQ1Q4,

        [TimeIdentifierLabelValue("Financial Year Q2", "FYQ2", AcademicOrFiscalShort, "Q2")]
        FinancialYearQ2,

        [TimeIdentifierLabelValue("Financial Year Q2-Q3", "FYQ2Q3", AcademicOrFiscalShort, "Q2-Q3")]
        FinancialYearQ2Q3,

        [TimeIdentifierLabelValue("Financial Year Q2-Q4", "FYQ2Q4", AcademicOrFiscalShort, "Q2-Q4")]
        FinancialYearQ2Q4,

        [TimeIdentifierLabelValue("Financial Year Q3", "FYQ3", AcademicOrFiscalShort, "Q3")]
        FinancialYearQ3,

        [TimeIdentifierLabelValue("Financial Year Q3-Q4", "FYQ3Q4", AcademicOrFiscalShort, "Q3-Q4")]
        FinancialYearQ3Q4,

        [TimeIdentifierLabelValue("Financial Year Q4", "FYQ4", AcademicOrFiscalShort, "Q4")]
        FinancialYearQ4,

        [TimeIdentifierLabelValue("Tax Year", "TY", AcademicOrFiscalNoLabel)]
        TaxYear,

        [TimeIdentifierLabelValue("Tax Year Q1", "TYQ1", AcademicOrFiscalShort, "Q1")]
        TaxYearQ1,

        [TimeIdentifierLabelValue("Tax Year Q1-Q2", "TYQ1Q2", AcademicOrFiscalShort, "Q1-Q2")]
        TaxYearQ1Q2,

        [TimeIdentifierLabelValue("Tax Year Q1-Q3", "TYQ1Q3", AcademicOrFiscalShort, "Q1-Q3")]
        TaxYearQ1Q3,

        [TimeIdentifierLabelValue("Tax Year Q1-Q4", "TYQ1Q4", AcademicOrFiscalShort, "Q1-Q4")]
        TaxYearQ1Q4,

        [TimeIdentifierLabelValue("Tax Year Q2", "TYQ2", AcademicOrFiscalShort, "Q2")]
        TaxYearQ2,

        [TimeIdentifierLabelValue("Tax Year Q2-Q3", "TYQ2Q3", AcademicOrFiscalShort, "Q2-Q3")]
        TaxYearQ2Q3,

        [TimeIdentifierLabelValue("Tax Year Q2-Q4", "TYQ2Q4", AcademicOrFiscalShort, "Q2-Q4")]
        TaxYearQ2Q4,

        [TimeIdentifierLabelValue("Tax Year Q3", "TYQ3", AcademicOrFiscalShort, "Q3")]
        TaxYearQ3,

        [TimeIdentifierLabelValue("Tax Year Q3-Q4", "TYQ3Q4", AcademicOrFiscalShort, "Q3-Q4")]
        TaxYearQ3Q4,

        [TimeIdentifierLabelValue("Tax Year Q4", "TYQ4", AcademicOrFiscalShort, "Q4")]
        TaxYearQ4,

        [TimeIdentifierLabelValue("Five Half Terms", "HT5", AcademicOrFiscalNoLabel)]
        FiveHalfTerms,

        [TimeIdentifierLabelValue("Six Half Terms", "HT6", AcademicOrFiscalNoLabel)]
        SixHalfTerms,

        [TimeIdentifierLabelValue("Up Until 31st March", "EOM")]
        EndOfMarch,

        [TimeIdentifierLabelValue("Autumn Term", "T1", AcademicOrFiscal)]
        AutumnTerm,

        [TimeIdentifierLabelValue("Autumn and Spring Term", "T1T2", AcademicOrFiscal)]
        AutumnSpringTerm,

        [TimeIdentifierLabelValue("Spring Term", "T2", AcademicOrFiscal)]
        SpringTerm,

        [TimeIdentifierLabelValue("Summer Term", "T3", AcademicOrFiscal)]
        SummerTerm,

        [TimeIdentifierLabelValue("January", "M1")]
        January,

        [TimeIdentifierLabelValue("February", "M2")]
        February,

        [TimeIdentifierLabelValue("March", "M3")]
        March,

        [TimeIdentifierLabelValue("April", "M4")]
        April,

        [TimeIdentifierLabelValue("May", "M5")]
        May,

        [TimeIdentifierLabelValue("June", "M6")]
        June,

        [TimeIdentifierLabelValue("July", "M7")]
        July,

        [TimeIdentifierLabelValue("August", "M8")]
        August,

        [TimeIdentifierLabelValue("September", "M9")]
        September,

        [TimeIdentifierLabelValue("October", "M10")]
        October,

        [TimeIdentifierLabelValue("November", "M11")]
        November,

        [TimeIdentifierLabelValue("December", "M12")]
        December
    }
}