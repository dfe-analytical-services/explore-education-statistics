using System.Net;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

public class UnableToCreateSearchableDocumentException(
    string publicationSlug, 
    string message) 
    : Exception(message)
    {
        public string PublicationSlug { get; } = publicationSlug;
    }

public class UnableToGetPublicationLatestReleaseSearchViewModelException : UnableToCreateSearchableDocumentException
{
    public UnableToGetPublicationLatestReleaseSearchViewModelException(
        string publicationSlug,
        HttpStatusCode? statusCode,
        string? errorMessage) : base(publicationSlug,
        $"Unable to get latest release search view model for publication \"{publicationSlug}\" with { (statusCode != null ? "status code \"{statusCode}\" and":string.Empty) } error message: \"{errorMessage}\"")
    {
    }
    
    public UnableToGetPublicationLatestReleaseSearchViewModelException(
        string publicationSlug,
        string? errorMessage) : base(publicationSlug,
        $"Unable to get latest release search view model for publication \"{publicationSlug}\". Error message: \"{errorMessage}\"")
    {
    }
}
