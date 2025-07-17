namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the list of publication infos by theme fails
/// </summary>
public class UnableToGetPublicationsByThemeException(Guid themeId, string errorMessage)
    : Exception($"Unable to get Publication Infos for ThemeId {themeId}. Error: {errorMessage}");
