using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdatePublicationRequirement : IAuthorizationRequirement
    {}
    
    public class UpdatePublicationAuthorizationHandler : CompoundAuthorizationHandler<
        UpdatePublicationRequirement, Publication>
    {
        public UpdatePublicationAuthorizationHandler() 
            : base(new CanUpdateAllPublications()) {}
        
        public class CanUpdateAllPublications : HasClaimAuthorizationHandler<
            UpdatePublicationRequirement>
        {
            public CanUpdateAllPublications() 
                : base(SecurityClaimTypes.UpdateAllPublications) {}
        }
    }
}