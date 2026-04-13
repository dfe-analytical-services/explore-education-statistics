namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the release id's for a publication fails.
/// </summary>
public class UnableToGetPublicationReleaseIdsException(string publicationSlug, string errorMessage)
    : Exception(
        $"""
        Unable to get release id's for publication "{publicationSlug}". Error: "{errorMessage}"
        """
    );
