#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockParent
{
    public Guid Id { get; set; }

    public Guid? LatestDraftVersionId { get; set; }

    public DataBlockVersion? LatestDraftVersion { get; set; }

    public Guid? LatestPublishedVersionId { get; set; }

    public DataBlockVersion? LatestPublishedVersion { get; set; }
}
