using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    // Create

    public class CreateLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class CreateLegacyReleaseAuthorizationHandler 
        : CompoundAuthorizationHandler<CreateLegacyReleaseRequirement, Publication>
    {
        public CreateLegacyReleaseAuthorizationHandler() : base(
            new CanCreateAnyLegacyRelease()
        )
        {}
    
        public class CanCreateAnyLegacyRelease : 
            HasClaimAuthorizationHandler<CreateLegacyReleaseRequirement>
        {
            public CanCreateAnyLegacyRelease() 
                : base(SecurityClaimTypes.CreateAnyRelease) {}
        }
    }

    // View

    public class ViewLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class ViewLegacyReleaseAuthorizationHandler 
        : CompoundAuthorizationHandler<ViewLegacyReleaseRequirement, LegacyRelease>
    {
        public ViewLegacyReleaseAuthorizationHandler() : base(
            new CanViewAllLegacyReleases()
        )
        {}
    
        public class CanViewAllLegacyReleases : 
            HasClaimAuthorizationHandler<ViewLegacyReleaseRequirement>
        {
            public CanViewAllLegacyReleases() 
                : base(SecurityClaimTypes.AccessAllReleases) {}
        }
    }

    // Update

    public class UpdateLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class UpdateLegacyReleaseAuthorizationHandler 
        : CompoundAuthorizationHandler<UpdateLegacyReleaseRequirement, LegacyRelease>
    {
        public UpdateLegacyReleaseAuthorizationHandler() : base(
            new CanUpdateAllLegacyReleases()
        )
        {}
    
        public class CanUpdateAllLegacyReleases : 
            HasClaimAuthorizationHandler<UpdateLegacyReleaseRequirement>
        {
            public CanUpdateAllLegacyReleases() 
                : base(SecurityClaimTypes.UpdateAllReleases) {}
        }
    }

    // Delete

    public class DeleteLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class DeleteLegacyReleaseAuthorizationHandler 
        : CompoundAuthorizationHandler<DeleteLegacyReleaseRequirement, LegacyRelease>
    {
        public DeleteLegacyReleaseAuthorizationHandler() : base(
            new CanDeleteAllLegacyReleases()
        )
        {}
    
        public class CanDeleteAllLegacyReleases : 
            HasClaimAuthorizationHandler<DeleteLegacyReleaseRequirement>
        {
            public CanDeleteAllLegacyReleases()
                : base(SecurityClaimTypes.UpdateAllReleases) {}
        }
    }
}