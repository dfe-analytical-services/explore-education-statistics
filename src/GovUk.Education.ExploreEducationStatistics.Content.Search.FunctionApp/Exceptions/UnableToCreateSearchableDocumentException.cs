namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

public class UnableToCreateSearchableDocumentException(
    string publicationSlug, 
    string message) 
    : Exception(message)
    {
        public string PublicationSlug { get; } = publicationSlug;
    }
