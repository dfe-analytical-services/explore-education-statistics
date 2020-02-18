using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement
    {}
    
    public class CreateReleaseForSpecificPublicationAuthorizationHandler 
        : CompoundAuthorizationHandler<CreateReleaseForSpecificPublicationRequirement, Release>
    {
        public CreateReleaseForSpecificPublicationAuthorizationHandler() : base(
            new CreateReleaseForSpecificPublicationCanCreateForAnyPublicationAuthorizationHandler())
        {}
    }
    
    public class CreateReleaseForSpecificPublicationCanCreateForAnyPublicationAuthorizationHandler : 
        HasClaimAuthorizationHandler<CreateReleaseForSpecificPublicationRequirement>
    {
        public CreateReleaseForSpecificPublicationCanCreateForAnyPublicationAuthorizationHandler() 
            : base(SecurityClaimTypes.CreateAnyRelease) {}
    }
}