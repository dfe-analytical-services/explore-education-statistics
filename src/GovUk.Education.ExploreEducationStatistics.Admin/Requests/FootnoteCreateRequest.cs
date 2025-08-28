#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FootnoteCreateRequest
{
    private string _content = string.Empty;

    public string Content
    {
        get => _content;
        init => _content = value.Trim();
    }

    public IReadOnlySet<Guid> Filters { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> FilterGroups { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> FilterItems { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> Indicators { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> Subjects { get; init; } = new HashSet<Guid>();
}
