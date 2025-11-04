namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public enum PublicationsSortBy
{
    /// <summary>
    /// Sort by the published date of the latest published release version by time series associated with the publication.
    /// </summary>
    Published,

    /// <summary>
    /// Sort by the relevance of the publication to the search term. This option is only applicable when a search term is provided.
    /// </summary>
    Relevance,

    /// <summary>
    /// Sort by the title of the publication.
    /// </summary>
    Title,
}
