#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class Filter
{
    public Guid Id { get; set; }
    public string? Hint { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GroupCsvColumn { get; set; }
    public string? ParentFilter { get; set; }
    public string? AutoSelectFilterItemLabel { get; set; }
    public Guid? AutoSelectFilterItemId { get; set; }
    public FilterItem? AutoSelectFilterItem { get; set; }
    public Subject Subject { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public List<FilterGroup> FilterGroups { get; set; } = new();
    public List<FilterFootnote> Footnotes { get; set; } = new();

    public static IEqualityComparer<Filter> IdComparer { get; } = new IdEqualityComparer();

    public Filter() { }

    public Filter(
        string? hint,
        string label,
        string name,
        string? groupCsvColumn,
        string? parentFilter,
        string? autoSelectFilterItemLabel,
        Guid subjectId
    )
    {
        Id = Guid.NewGuid();
        Hint = hint;
        Label = label;
        Name = name;
        GroupCsvColumn = groupCsvColumn;
        ParentFilter = parentFilter;
        AutoSelectFilterItemLabel = autoSelectFilterItemLabel;
        SubjectId = subjectId;
        FilterGroups = new List<FilterGroup>();
    }

    public Filter Clone()
    {
        return (Filter)MemberwiseClone();
    }

    private sealed class IdEqualityComparer : IEqualityComparer<Filter>
    {
        public bool Equals(Filter x, Filter y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Filter obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
