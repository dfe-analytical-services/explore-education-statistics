#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class UserReleaseRole : ResourceRole<ReleaseRole, ReleaseVersion>
{
    public ReleaseVersion ReleaseVersion
    {
        get => Resource;
        set => Resource = value;
    }

    public Guid ReleaseVersionId
    {
        get => ResourceId;
        set => ResourceId = value;
    }

    public bool SoftDeleted { get; set; }
}
