namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the list of publications which have data set
/// documents fails
/// </summary>
public class UnableToGetDataSetDocumentPublicationsException(string errorMessage)
    : Exception($"Unable to get Data Set Document Publications. Error: {errorMessage}");
