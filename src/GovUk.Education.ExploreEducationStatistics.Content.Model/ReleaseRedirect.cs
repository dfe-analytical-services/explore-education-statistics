#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseRedirect : ICreatedTimestamp<DateTime>
{
    public string Slug { get; set; } = null!;

    public Guid ReleaseId { get; set; }

    public Release Release { get; set; } = null!;

    public DateTime Created { get; set; }
}
