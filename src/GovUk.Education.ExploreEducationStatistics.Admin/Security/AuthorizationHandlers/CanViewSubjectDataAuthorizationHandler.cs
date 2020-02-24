using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CanViewSubjectDataRequirement : IAuthorizationRequirement
    {
    }
    
    public class CanViewSubjectDataAuthorizationHandler :
        HasClaimAuthorizationHandler<CanViewSubjectDataRequirement>
    {
        public CanViewSubjectDataAuthorizationHandler() : base(SecurityClaimTypes
            .ApplicationAccessGranted)
        {
        }
    }
}