#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Update : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    // TODO - Can this be non-nullable?
    public DateTime? Created { get; set; }

    // TODO - Can this be non-nullable?
    public User? CreatedBy { get; set; }

    // TODO - Can this be non-nullable?
    public Guid? CreatedById { get; set; }

    public DateTime On { get; set; }

    public string Reason { get; set; } = null!;
}
