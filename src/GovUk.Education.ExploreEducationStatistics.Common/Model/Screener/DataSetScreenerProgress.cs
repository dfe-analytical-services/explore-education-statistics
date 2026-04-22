namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

public record DataSetScreenerProgress
{
    public int PercentageComplete { get; set; }

    public string Stage { get; set; } = null!;

    public bool Completed { get; set; }

    public bool Passed { get; set; }
}
