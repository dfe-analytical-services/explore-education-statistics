namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The time period criteria to filter results by in a data set query.
/// </summary>
public record DataSetQueryCriteriaTimePeriods
{
    /// <summary>
    /// Filter the results to be in this time period.
    /// </summary>
    public DataSetQueryTimePeriod? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this time period.
    /// </summary>
    public DataSetQueryTimePeriod? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these time periods.
    /// </summary>
    public IReadOnlyList<DataSetQueryTimePeriod>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these time periods.
    /// </summary>
    public IReadOnlyList<DataSetQueryTimePeriod>? NotIn { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Gt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than or equal to the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Gte { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Lt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than or equal to the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Lte { get; init; }
}
