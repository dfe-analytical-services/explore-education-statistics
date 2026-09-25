namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public enum ValidationErrorMessages
{
    // Table builder queries
    QueryExceedsMaxAllowableTableSize,
    FilterItemsNotFound,

    // Updating Filters
    FiltersDifferFromSubject,
    FilterGroupsDifferFromSubject,
    FilterItemsDifferFromSubject,

    // Updating Indicators
    IndicatorGroupsDifferFromSubject,
    IndicatorsDifferFromSubject,
}
