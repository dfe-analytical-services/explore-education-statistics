namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the list of publication infos fails
/// </summary>
public class UnableToGetPublicationInfosException(string errorMessage)
    : Exception($"Unable to get Publication Infos. Error: {errorMessage}");
