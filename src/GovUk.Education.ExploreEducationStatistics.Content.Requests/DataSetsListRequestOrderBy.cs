namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public enum DataSetsListRequestOrderBy
{
    /// <summary>
    /// Order by the natural order of the data set, as defined by analysts. All results must be for the same release version
    /// to ensure a stable sort order. This option is only applicable in combination with the release version filter.
    /// </summary>
    Natural,

    /// <summary>
    /// Order by the published date of the release version that the data set is associated with.
    /// </summary>
    Published,

    /// <summary>
    /// Order by the relevance of the data set to the search term. This option is only applicable when a search term is provided.
    /// </summary>
    Relevance,

    /// <summary>
    /// Order by the title of the data set.
    /// </summary>
    Title
}
