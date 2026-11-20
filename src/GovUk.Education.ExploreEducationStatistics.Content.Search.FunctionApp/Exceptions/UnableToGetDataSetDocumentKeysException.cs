namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the list of data set document keys fails
/// </summary>
public class UnableToGetDataSetDocumentKeysException(string errorMessage)
    : Exception($"Unable to get Data Set Document Keys. Error: {errorMessage}");
