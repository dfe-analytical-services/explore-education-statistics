using System.ComponentModel;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A data set query to match results against.
/// </summary>
public record DataSetQueryRequest
{
    /// <summary>
    /// The criteria to match.
    /// </summary>
    public DataSetQueryCriteria? Criteria { get; init; }

    /// <summary>
    /// The IDs of indicators in the data set to return values for.
    /// </summary>
    public required IReadOnlyList<string> Indicators { get; init; }

    /// <summary>
    /// The sorts to sort the results by. Sorts at the start of the
    /// list will be applied first.
    ///
    /// By default, results are sorted by time period in descending order.
    /// </summary>
    public IReadOnlyList<DataSetQuerySort>? Sorts { get; init; }

    /// <summary>
    /// Enable debug mode. Results will be formatted with human-readable
    /// labels to assist in identification.
    ///
    /// This **should not** be enabled in a production environment.
    /// </summary>
    public bool Debug { get; init; }

    /// <summary>
    /// The page of results to fetch.
    /// </summary>
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    [DefaultValue(1000)]
    public int PageSize { get; init; } = 1000;
}
