using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement
    {}
    
    public class CreateReleaseForSpecificPublicationCanCreateForAnyPublicationAuthorizationHandler : 
        HasClaimAuthorizationHandler<CreateReleaseForSpecificPublicationRequirement>
    {
        public CreateReleaseForSpecificPublicationCanCreateForAnyPublicationAuthorizationHandler() 
            : base(SecurityClaimTypes.CreateAnyRelease) {}
    }
}