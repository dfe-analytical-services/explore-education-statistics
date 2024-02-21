namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMeta
{
    public int Id { get; set; }

    public required string Label { get; set; }

    public bool? IsAggregate { get; set; }

    public List<FilterMeta> Metas { get; set; } = [];

    public List<FilterOptionMetaLink> MetaLinks { get; set; } = [];
}
