#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EmbedBlock : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Url { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}
