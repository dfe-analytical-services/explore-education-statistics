namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMeta
{
    public required string PublicId { get; set; }

    public required int PrivateId { get; set; }

    public required string Label { get; set; }

    public bool? IsAggregate { get; set; } = null!;
}
