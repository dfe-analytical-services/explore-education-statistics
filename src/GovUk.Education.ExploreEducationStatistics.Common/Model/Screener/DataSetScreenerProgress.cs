namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

/// <summary>
/// A record for capturing regular updates to the screening progress
/// for a particular data set.
/// </summary>
public record DataSetScreenerProgress
{
    public int PercentageComplete { get; set; }

    public string Stage { get; set; } = null!;

    public bool Completed { get; set; }

    public bool Passed { get; set; }
}
