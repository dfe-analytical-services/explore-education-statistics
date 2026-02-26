#nullable enable
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public enum ContentSectionType
{
    Generic,
    ReleaseSummary,
    KeyStatisticsSecondary,
    Headlines,
    RelatedDashboards,
    Warning,
}

public class ContentSection
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string? Heading { get; set; }

    public List<ContentBlock> Content { get; set; } = [];

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    [JsonIgnore]
    public ContentSectionType Type { get; set; }

    public T? FindSingleContentBlockOfType<T>()
        where T : ContentBlock => Content.OfType<T>().SingleOrDefault();
}
