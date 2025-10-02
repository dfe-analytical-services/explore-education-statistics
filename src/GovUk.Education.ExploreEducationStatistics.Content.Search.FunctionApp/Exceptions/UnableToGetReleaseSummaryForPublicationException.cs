namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the release summary for a publication fails.
/// </summary>
public class UnableToGetReleaseSummaryForPublicationException(
    string publicationSlug,
    string releaseSlug,
    string errorMessage
)
    : Exception(
        $"""
        Unable to get release summary for release "{releaseSlug}" for publication "{publicationSlug}". Error: "{errorMessage}"
        """
    );
