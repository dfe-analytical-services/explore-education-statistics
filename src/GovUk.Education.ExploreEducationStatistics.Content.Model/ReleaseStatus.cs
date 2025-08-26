#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseStatus : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public string? InternalReleaseNote { get; set; }

    public ReleaseApprovalStatus ApprovalStatus { get; set; }

    public DateTime? Created { get; set; }

    public Guid? CreatedById { get; set; }

    public User? CreatedBy { get; set; }
}
