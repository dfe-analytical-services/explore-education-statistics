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
}
