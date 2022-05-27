#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class UserReleaseRole : ResourceRole<ReleaseRole, Release>
{
    public Release Release
    {
        get => Resource;
        set => Resource = value;
    }

    public Guid ReleaseId 
    {
        get => ResourceId;
        set => ResourceId = value;
    }
        
    public bool SoftDeleted { get; set; }

    public UserReleaseRole CopyForAmendment(Release amendment)
    {
        var copy = MemberwiseClone() as UserReleaseRole;
        copy.Id = Guid.NewGuid();
        copy.Resource = amendment;
        copy.ResourceId = amendment.Id;
        return copy;
    }
}