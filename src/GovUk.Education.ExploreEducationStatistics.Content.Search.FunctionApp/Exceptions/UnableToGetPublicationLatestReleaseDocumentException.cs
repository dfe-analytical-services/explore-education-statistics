namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// The call to the ContentAPI to retrieve the latest release as a release document failed
/// </summary>
public class UnableToGetPublicationLatestReleaseDocumentException(string publicationSlug, string? errorMessage)
    : Exception(
        $"""
        Unable to get latest release document for publication "{publicationSlug}". Error: "{errorMessage}"
        """
    );
