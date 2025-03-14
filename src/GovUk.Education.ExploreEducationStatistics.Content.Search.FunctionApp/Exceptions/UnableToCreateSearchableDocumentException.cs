using System.Net;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// The creation of a searchable document in Azure Storage failed
/// </summary>
public class UnableToCreateSearchableDocumentException(string publicationSlug, string message)
    : Exception($"Publication: \"{publicationSlug}\" Error: {message}");

/// <summary>
/// The call to the ContentAPI to retrieve the searchable document form of the latest release failed
/// </summary>
public class UnableToGetPublicationLatestReleaseSearchViewModelException : UnableToCreateSearchableDocumentException
{
    public UnableToGetPublicationLatestReleaseSearchViewModelException(
        string publicationSlug,
        HttpStatusCode? statusCode,
        string? errorMessage)
        : base(
            publicationSlug,
            $"Unable to get latest release search view model for publication \"{publicationSlug}\" with {(statusCode != null ? $"status code \"{statusCode}\" and" : string.Empty)} error message: \"{errorMessage}\"")
    {
    }

    public UnableToGetPublicationLatestReleaseSearchViewModelException(string publicationSlug, string? errorMessage)
        : base(
            publicationSlug,
            $"Unable to get latest release search view model for publication \"{publicationSlug}\". Error message: \"{errorMessage}\"")
    {
    }
}
