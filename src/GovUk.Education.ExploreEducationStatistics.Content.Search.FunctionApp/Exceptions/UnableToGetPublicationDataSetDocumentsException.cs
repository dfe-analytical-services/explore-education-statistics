namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the data set documents of a publication fails
/// </summary>
public class UnableToGetPublicationDataSetDocumentsException(string publicationSlug, string errorMessage)
    : Exception(
        $"""
        Unable to get the Data Set Documents for publication "{publicationSlug}". Error: "{errorMessage}"
        """
    );
