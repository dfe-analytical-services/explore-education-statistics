#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodLabelFormat;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodYearFormat;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifierCategory;
using Category = GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifierCategory;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum TimeIdentifier
    {
        [TimeIdentifierMeta("Academic year", "AY", Category.AcademicYear, Academic, NoLabel)]
        AcademicYear,

        [TimeIdentifierMeta("Academic year Q1", "AYQ1", Category.AcademicYear, Academic, ShortLabel, "Q1")]
        AcademicYearQ1,

        [TimeIdentifierMeta("Academic year Q2", "AYQ2", Category.AcademicYear, Academic, ShortLabel, "Q2")]
        AcademicYearQ2,

        [TimeIdentifierMeta("Academic year Q3", "AYQ3", Category.AcademicYear, Academic, ShortLabel, "Q3")]
        AcademicYearQ3,

        [TimeIdentifierMeta("Academic year Q4", "AYQ4", Category.AcademicYear, Academic, ShortLabel, "Q4")]
        AcademicYearQ4,

        [TimeIdentifierMeta("Calendar year", "CY", Category.CalendarYear, Default, NoLabel)]
        CalendarYear,

        [TimeIdentifierMeta("Calendar year Q1", "CYQ1", Category.CalendarYear, Default, ShortLabel, "Q1")]
        CalendarYearQ1,

        [TimeIdentifierMeta("Calendar year Q2", "CYQ2", Category.CalendarYear, Default, ShortLabel, "Q2")]
        CalendarYearQ2,

        [TimeIdentifierMeta("Calendar year Q3", "CYQ3", Category.CalendarYear, Default, ShortLabel, "Q3")]
        CalendarYearQ3,

        [TimeIdentifierMeta("Calendar year Q4", "CYQ4", Category.CalendarYear, Default, ShortLabel, "Q4")]
        CalendarYearQ4,

        [TimeIdentifierMeta("Part 1 (April to September)", "P1", FinancialYearPart, Fiscal, ShortLabel,
            "Part 1 (Apr to Sep)")]
        FinancialYearPart1,

        [TimeIdentifierMeta("Part 2 (October to March)", "P2", FinancialYearPart, Fiscal, ShortLabel,
            "Part 2 (Oct to Mar)")]
        FinancialYearPart2,

        [TimeIdentifierMeta("Financial year", "FY", Category.FinancialYear, Fiscal, NoLabel)]
        FinancialYear,

        [TimeIdentifierMeta("Financial year Q1", "FYQ1", Category.FinancialYear, Fiscal, ShortLabel, "Q1")]
        FinancialYearQ1,

        [TimeIdentifierMeta("Financial year Q2", "FYQ2", Category.FinancialYear, Fiscal, ShortLabel, "Q2")]
        FinancialYearQ2,

        [TimeIdentifierMeta("Financial year Q3", "FYQ3", Category.FinancialYear, Fiscal, ShortLabel, "Q3")]
        FinancialYearQ3,

        [TimeIdentifierMeta("Financial year Q4", "FYQ4", Category.FinancialYear, Fiscal, ShortLabel, "Q4")]
        FinancialYearQ4,

        [TimeIdentifierMeta("Tax year", "TY", Category.TaxYear, Fiscal, NoLabel)]
        TaxYear,

        [TimeIdentifierMeta("Tax year Q1", "TYQ1", Category.TaxYear, Fiscal, ShortLabel, "Q1")]
        TaxYearQ1,

        [TimeIdentifierMeta("Tax year Q2", "TYQ2", Category.TaxYear, Fiscal, ShortLabel, "Q2")]
        TaxYearQ2,

        [TimeIdentifierMeta("Tax year Q3", "TYQ3", Category.TaxYear, Fiscal, ShortLabel, "Q3")]
        TaxYearQ3,

        [TimeIdentifierMeta("Tax year Q4", "TYQ4", Category.TaxYear, Fiscal, ShortLabel, "Q4")]
        TaxYearQ4,

        [TimeIdentifierMeta("Reporting year", "RY", Category.ReportingYear, Default, NoLabel)]
        ReportingYear,

        [TimeIdentifierMeta("Autumn term", "T1", Term, Academic)]
        AutumnTerm,

        [TimeIdentifierMeta("Autumn and spring term", "T1T2", Term, Academic)]
        AutumnSpringTerm,

        [TimeIdentifierMeta("Spring term", "T2", Term, Academic)]
        SpringTerm,

        [TimeIdentifierMeta("Summer term", "T3", Term, Academic)]
        SummerTerm,

        [TimeIdentifierMeta("Week 1", "W1", Week)]
        Week1,

        [TimeIdentifierMeta("Week 2", "W2", Week)]
        Week2,

        [TimeIdentifierMeta("Week 3", "W3", Week)]
        Week3,

        [TimeIdentifierMeta("Week 4", "W4", Week)]
        Week4,

        [TimeIdentifierMeta("Week 5", "W5", Week)]
        Week5,

        [TimeIdentifierMeta("Week 6", "W6", Week)]
        Week6,

        [TimeIdentifierMeta("Week 7", "W7", Week)]
        Week7,

        [TimeIdentifierMeta("Week 8", "W8", Week)]
        Week8,

        [TimeIdentifierMeta("Week 9", "W9", Week)]
        Week9,

        [TimeIdentifierMeta("Week 10", "W10", Week)]
        Week10,

        [TimeIdentifierMeta("Week 11", "W11", Week)]
        Week11,

        [TimeIdentifierMeta("Week 12", "W12", Week)]
        Week12,

        [TimeIdentifierMeta("Week 13", "W13", Week)]
        Week13,

        [TimeIdentifierMeta("Week 14", "W14", Week)]
        Week14,

        [TimeIdentifierMeta("Week 15", "W15", Week)]
        Week15,

        [TimeIdentifierMeta("Week 16", "W16", Week)]
        Week16,

        [TimeIdentifierMeta("Week 17", "W17", Week)]
        Week17,

        [TimeIdentifierMeta("Week 18", "W18", Week)]
        Week18,

        [TimeIdentifierMeta("Week 19", "W19", Week)]
        Week19,

        [TimeIdentifierMeta("Week 20", "W20", Week)]
        Week20,

        [TimeIdentifierMeta("Week 21", "W21", Week)]
        Week21,

        [TimeIdentifierMeta("Week 22", "W22", Week)]
        Week22,

        [TimeIdentifierMeta("Week 23", "W23", Week)]
        Week23,

        [TimeIdentifierMeta("Week 24", "W24", Week)]
        Week24,

        [TimeIdentifierMeta("Week 25", "W25", Week)]
        Week25,

        [TimeIdentifierMeta("Week 26", "W26", Week)]
        Week26,

        [TimeIdentifierMeta("Week 27", "W27", Week)]
        Week27,

        [TimeIdentifierMeta("Week 28", "W28", Week)]
        Week28,

        [TimeIdentifierMeta("Week 29", "W29", Week)]
        Week29,

        [TimeIdentifierMeta("Week 30", "W30", Week)]
        Week30,

        [TimeIdentifierMeta("Week 31", "W31", Week)]
        Week31,

        [TimeIdentifierMeta("Week 32", "W32", Week)]
        Week32,

        [TimeIdentifierMeta("Week 33", "W33", Week)]
        Week33,

        [TimeIdentifierMeta("Week 34", "W34", Week)]
        Week34,

        [TimeIdentifierMeta("Week 35", "W35", Week)]
        Week35,

        [TimeIdentifierMeta("Week 36", "W36", Week)]
        Week36,

        [TimeIdentifierMeta("Week 37", "W37", Week)]
        Week37,

        [TimeIdentifierMeta("Week 38", "W38", Week)]
        Week38,

        [TimeIdentifierMeta("Week 39", "W39", Week)]
        Week39,

        [TimeIdentifierMeta("Week 40", "W40", Week)]
        Week40,

        [TimeIdentifierMeta("Week 41", "W41", Week)]
        Week41,

        [TimeIdentifierMeta("Week 42", "W42", Week)]
        Week42,

        [TimeIdentifierMeta("Week 43", "W43", Week)]
        Week43,

        [TimeIdentifierMeta("Week 44", "W44", Week)]
        Week44,

        [TimeIdentifierMeta("Week 45", "W45", Week)]
        Week45,

        [TimeIdentifierMeta("Week 46", "W46", Week)]
        Week46,

        [TimeIdentifierMeta("Week 47", "W47", Week)]
        Week47,

        [TimeIdentifierMeta("Week 48", "W48", Week)]
        Week48,

        [TimeIdentifierMeta("Week 49", "W49", Week)]
        Week49,

        [TimeIdentifierMeta("Week 50", "W50", Week)]
        Week50,

        [TimeIdentifierMeta("Week 51", "W51", Week)]
        Week51,

        [TimeIdentifierMeta("Week 52", "W52", Week)]
        Week52,

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
        December,

        // EES-3959 Temporary time identifier added to give the correct order to this release
        // The publication is made up of months and academic years, so by placing this last, the release
        // is ordered after all the other monthly releases
        // https://explore-education-statistics.service.gov.uk/find-statistics/national-tutoring-programme/2022-23,
        // TODO remove this once we've got a solution for ordering releases which mix categories of time periods
        [TimeIdentifierMeta("Academic year ", "AYNTP", NationalTutoringProgramme, Academic, NoLabel)]
        AcademicYearNationalTutoringProgramme,

    }
}
