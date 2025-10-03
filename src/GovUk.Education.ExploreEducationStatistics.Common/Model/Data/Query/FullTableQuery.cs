#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

public record FullTableQuery
{
    public Guid SubjectId { get; set; }

    public List<Guid> LocationIds { get; set; } = new();

    public TimePeriodQuery? TimePeriod { get; set; }

    public IEnumerable<Guid> Filters
    {
        [Obsolete("Use GetFilterItemIds() or GetNonHierarchicalFilterItemIds")]
        get;
        set;
    } = new List<Guid>();

    public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

    [JsonConverter(typeof(FilterHierarchiesOptionsConverter))]
    public List<FilterHierarchyOptions>? FilterHierarchiesOptions { get; set; } = null;

    public IEnumerable<Guid> GetNonHierarchicalFilterItemIds()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return Filters;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public List<Guid> GetFilterItemIds()
    {
        var filterItemIds = GetNonHierarchicalFilterItemIds().ToList();
        if (FilterHierarchiesOptions != null)
        {
            filterItemIds.AddRange(
                FilterHierarchiesOptions.SelectMany(filterHierarchyOptions =>
                    filterHierarchyOptions.Options.SelectMany(hierarchyOption => hierarchyOption)
                )
            );
        }

        // NOTE: We don't include FilterHierarchiesOptions.LeafFilterIds as they are filterIds, not filterItemIds

        return filterItemIds.Distinct().ToList();
    }
}
