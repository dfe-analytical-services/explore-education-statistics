using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.TimeIdentifierUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class TimeIdentifierExtensions
{
    public static bool IsAlike(this TimeIdentifier timeIdentifier, TimeIdentifier compare)
    {
        if (timeIdentifier.Equals(compare))
        {
            return true;
        }

        return IsAcademicQuarter(timeIdentifier) && IsAcademicQuarter(compare)
            || IsCalendarQuarter(timeIdentifier) && IsCalendarQuarter(compare)
            || IsFinancialQuarter(timeIdentifier) && IsFinancialQuarter(compare)
            || IsTaxQuarter(timeIdentifier) && IsTaxQuarter(compare)
            || IsMonth(timeIdentifier) && IsMonth(compare)
            || IsWeek(timeIdentifier) && IsWeek(compare)
            || IsTerm(timeIdentifier) && IsTerm(compare)
            || IsFinancialYearPart(timeIdentifier) && IsFinancialYearPart(compare);
    }

    public static bool IsYear(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingYears().Contains(timeIdentifier);
    }

    public static bool IsAcademicQuarter(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingAcademicQuarters().Contains(timeIdentifier);
    }

    public static bool IsCalendarQuarter(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingCalendarQuarters().Contains(timeIdentifier);
    }

    public static bool IsFinancialQuarter(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingFinancialQuarters().Contains(timeIdentifier);
    }

    public static bool IsTaxQuarter(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingTaxQuarters().Contains(timeIdentifier);
    }

    public static bool IsMonth(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingMonths().Contains(timeIdentifier);
    }

    public static bool IsWeek(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingWeeks().Contains(timeIdentifier);
    }

    public static bool IsTerm(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingTerms().Contains(timeIdentifier);
    }

    public static bool IsFinancialYearPart(this TimeIdentifier timeIdentifier)
    {
        return GetIdentifiersUsingFinancialYearParts().Contains(timeIdentifier);
    }

    public static bool HasAssociatedRange(this TimeIdentifier timeIdentifier)
    {
        return !timeIdentifier.IsYear();
    }

    public static TimeIdentifier[] GetAssociatedRange(this TimeIdentifier timeIdentifier)
    {
        if (timeIdentifier.IsMonth())
        {
            return GetIdentifiersUsingMonths();
        }

        if (timeIdentifier.IsWeek())
        {
            return GetIdentifiersUsingWeeks();
        }

        if (timeIdentifier.IsAcademicQuarter())
        {
            return GetIdentifiersUsingAcademicQuarters();
        }

        if (timeIdentifier.IsCalendarQuarter())
        {
            return GetIdentifiersUsingCalendarQuarters();
        }

        if (timeIdentifier.IsFinancialQuarter())
        {
            return GetIdentifiersUsingFinancialQuarters();
        }

        if (timeIdentifier.IsTaxQuarter())
        {
            return GetIdentifiersUsingTaxQuarters();
        }

        if (timeIdentifier.IsTerm())
        {
            return GetIdentifiersUsingTerms();
        }

        if (timeIdentifier.IsFinancialYearPart())
        {
            return GetIdentifiersUsingFinancialYearParts();
        }

        throw new ArgumentOutOfRangeException(nameof(timeIdentifier), "The time identifier has no associated range");
    }
}
