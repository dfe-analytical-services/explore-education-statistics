#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class UserReleaseInvite : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    public ReleaseRole Role { get; set; }

    public bool EmailSent { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public User CreatedBy { get; set; } = null!;

    public Guid CreatedById { get; set; }

    public bool SoftDeleted { get; set; }
}
