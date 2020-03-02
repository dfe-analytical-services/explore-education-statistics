using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement
    {}
    
    public class CreateReleaseForSpecificPublicationAuthorizationHandler 
        : CompoundAuthorizationHandler<CreateReleaseForSpecificPublicationRequirement, Publication>
    {
        public CreateReleaseForSpecificPublicationAuthorizationHandler() : base(
            new CanCreateReleaseForAnyPublicationAuthorizationHandler())
        {}
    }
    
    public class CanCreateReleaseForAnyPublicationAuthorizationHandler : 
        HasClaimAuthorizationHandler<CreateReleaseForSpecificPublicationRequirement>
    {
        public CanCreateReleaseForAnyPublicationAuthorizationHandler() 
            : base(SecurityClaimTypes.CreateAnyRelease) {}
    }
}