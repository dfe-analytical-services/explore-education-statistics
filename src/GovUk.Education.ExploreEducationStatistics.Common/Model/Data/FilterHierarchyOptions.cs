#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

public record FilterHierarchyOptions
{
    public Guid LeafFilterId { get; set; }
    public List<FilterHierarchyOption> Options { get; set; } = [];
}
