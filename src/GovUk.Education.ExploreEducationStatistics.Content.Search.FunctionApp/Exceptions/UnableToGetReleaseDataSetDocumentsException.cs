namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

/// <summary>
/// Exception thrown when the call to the Content API to retrieve the data set documents of a release fails
/// </summary>
public class UnableToGetReleaseDataSetDocumentsException(
    string publicationSlug,
    string releaseSlug,
    string errorMessage
)
    : Exception(
        $"""
        Unable to get the Data Set Documents for release "{releaseSlug}" under publication "{publicationSlug}". Error: "{errorMessage}"
        """
    );
