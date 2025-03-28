namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// The creation of a searchable document in Azure Storage failed
/// </summary>
public class UnableToCreateSearchableDocumentException(string publicationSlug, string message)
    : Exception($"Publication: \"{publicationSlug}\" Error: {message}");
