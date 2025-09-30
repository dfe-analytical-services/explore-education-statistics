#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class MethodologyNote : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime DisplayDate { get; set; }

    public Guid MethodologyVersionId { get; set; }

    public MethodologyVersion MethodologyVersion { get; set; } = null!;

    public DateTime Created { get; set; }

    public User CreatedBy { get; set; } = null!;

    public Guid CreatedById { get; set; }

    public DateTime? Updated { get; set; }

    public Guid? UpdatedById { get; set; }

    public User UpdatedBy { get; set; } = null!;
}
