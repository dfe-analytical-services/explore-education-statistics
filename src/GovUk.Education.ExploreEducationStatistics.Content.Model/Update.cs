#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Update : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    // Nullable to support older records missing Created timestamps
    public DateTime? Created { get; set; }

    // Nullable to support older records missing CreatedBy references
    public User? CreatedBy { get; set; }

    // Nullable to support older records missing CreatedBy references
    public Guid? CreatedById { get; set; }

    public DateTime On { get; set; }

    public string Reason { get; set; } = null!;
}
