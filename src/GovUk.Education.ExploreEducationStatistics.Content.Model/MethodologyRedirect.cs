#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class MethodologyRedirect : ICreatedTimestamp<DateTime>
{
    public string Slug { get; set; } = null!;

    public Guid MethodologyVersionId { get; set; }

    public MethodologyVersion MethodologyVersion { get; set; } = null!;

    public DateTime Created { get; set; }
}
