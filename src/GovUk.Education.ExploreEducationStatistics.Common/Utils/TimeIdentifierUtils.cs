using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Category = GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifierCategory;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class TimeIdentifierUtils
{
    private static readonly Lazy<IReadOnlyList<TimeIdentifier>> EnumsLazy = new(() =>
        EnumUtil.GetEnums<TimeIdentifier>().ToList()
    );

    private static readonly Lazy<IReadOnlyList<string>> CodesLazy = new(() =>
        EnumsLazy.Value.Select(ti => ti.GetEnumValue()).ToList()
    );

    private static readonly Lazy<IReadOnlyList<string>> LabelsLazy = new(() =>
        EnumsLazy.Value.Select(ti => ti.GetEnumLabel()).ToList()
    );

    public static IReadOnlyList<TimeIdentifier> Enums => EnumsLazy.Value;

    public static IReadOnlyList<string> Codes => CodesLazy.Value;

    public static IReadOnlyList<string> Labels => LabelsLazy.Value;

    public static TimeIdentifier[] GetIdentifiersUsingFinancialYearParts()
    {
        return Category.FinancialYearPart.GetTimeIdentifiers();
    }

    public static TimeIdentifier[] GetIdentifiersUsingMonths()
    {
        return Category.Month.GetTimeIdentifiers();
    }

    public static TimeIdentifier[] GetIdentifiersUsingYears()
    {
        return
        [
            TimeIdentifier.AcademicYear,
            TimeIdentifier.CalendarYear,
            TimeIdentifier.FinancialYear,
            TimeIdentifier.TaxYear,
            TimeIdentifier.ReportingYear,
        ];
    }

    public static TimeIdentifier[] GetIdentifiersUsingWeeks()
    {
        return Category.Week.GetTimeIdentifiers();
    }

    public static TimeIdentifier[] GetIdentifiersUsingTerms()
    {
        return Category.Term.GetTimeIdentifiers();
    }

    public static TimeIdentifier[] GetIdentifiersUsingAcademicQuarters()
    {
        return
        [
            TimeIdentifier.AcademicYearQ1,
            TimeIdentifier.AcademicYearQ2,
            TimeIdentifier.AcademicYearQ3,
            TimeIdentifier.AcademicYearQ4,
        ];
    }

    public static TimeIdentifier[] GetIdentifiersUsingCalendarQuarters()
    {
        return
        [
            TimeIdentifier.CalendarYearQ1,
            TimeIdentifier.CalendarYearQ2,
            TimeIdentifier.CalendarYearQ3,
            TimeIdentifier.CalendarYearQ4,
        ];
    }

    public static TimeIdentifier[] GetIdentifiersUsingFinancialQuarters()
    {
        return
        [
            TimeIdentifier.FinancialYearQ1,
            TimeIdentifier.FinancialYearQ2,
            TimeIdentifier.FinancialYearQ3,
            TimeIdentifier.FinancialYearQ4,
        ];
    }

    public static TimeIdentifier[] GetIdentifiersUsingTaxQuarters()
    {
        return [TimeIdentifier.TaxYearQ1, TimeIdentifier.TaxYearQ2, TimeIdentifier.TaxYearQ3, TimeIdentifier.TaxYearQ4];
    }
}
