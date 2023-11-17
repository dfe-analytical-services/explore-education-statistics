#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public enum ValidationErrorMessages
    {
        // Table builder queries
        QueryExceedsMaxAllowableTableSize,

        // Updating Filters
        FiltersDifferFromSubject,
        FilterGroupsDifferFromSubject,
        FilterItemsDifferFromSubject,

        // Updating Indicators
        IndicatorGroupsDifferFromSubject,
        IndicatorsDifferFromSubject,

        // Data Guidance
        // TODO EES-4661 Remove this when we remove checklist validation from DataGuidanceDataSetService.Validate
        PublicDataGuidanceRequired
    }
}
