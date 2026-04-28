namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the latest published release version summary fails.
/// </summary>
public class UnableToGetReleaseVersionSummaryException(string publicationSlug, string releaseSlug, string errorMessage)
    : Exception(
        $"""
        Unable to get the latest published release version summary for release "{releaseSlug}" under publication "{publicationSlug}". Error: "{errorMessage}"
        """
    );
