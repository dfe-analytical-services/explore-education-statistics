using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public enum ContentSectionType
{
    Generic,
    ReleaseSummary,
    KeyStatisticsSecondary,
    Headlines,
    RelatedDashboards,
}

public class ContentSection
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Heading { get; set; }

    public List<ContentBlock> Content { get; set; } = new();

    public ReleaseVersion ReleaseVersion { get; set; }

    public Guid ReleaseVersionId { get; set; }

    [JsonIgnore] public ContentSectionType Type { get; set; }
}
