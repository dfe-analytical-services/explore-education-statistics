namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// The call to the ContentAPI to retrieve the searchable document form of the latest release failed
/// </summary>
public class UnableToGetPublicationLatestReleaseSearchViewModelException(string publicationSlug, string? errorMessage)
    : Exception($"Unable to get latest release search view model for publication \"{publicationSlug}\". Error: \"{errorMessage}\"");
