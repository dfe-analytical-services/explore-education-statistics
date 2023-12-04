namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMeta
{
    public int Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public bool? IsAggregate { get; set; } = null!;
}
