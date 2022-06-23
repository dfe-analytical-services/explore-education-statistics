using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class UserPublicationRole : ResourceRole<PublicationRole, Publication>
{
    public Publication Publication
    {
        get => Resource;
        set => Resource = value;
    }
    
    public Guid PublicationId 
    {
        get => ResourceId;
        set => ResourceId = value;
    }
}