namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionChangeState
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    public required string FilterId { get; set; }

    public bool? IsAggregate { get; set; } = null!;
}
