using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreatePublicationForSpecificThemeRequirement : IAuthorizationRequirement
    {}
    
    public class CreatePublicationForSpecificThemeAuthorizationHandler : CompoundAuthorizationHandler<
        CreatePublicationForSpecificThemeRequirement, Theme>
    {
        public CreatePublicationForSpecificThemeAuthorizationHandler()
            : base(new CanCreateForAnyThemeAuthorizationHandler()) {}
        
        public class CanCreateForAnyThemeAuthorizationHandler : HasClaimAuthorizationHandler<
            CreatePublicationForSpecificThemeRequirement>
        {
            public CanCreateForAnyThemeAuthorizationHandler()
                : base(SecurityClaimTypes.CreateAnyPublication) {}
        }
    }
}
