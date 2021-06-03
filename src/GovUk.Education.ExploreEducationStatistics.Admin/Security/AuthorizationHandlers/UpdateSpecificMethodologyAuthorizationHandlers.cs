using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler :
        HasClaimAuthorizationHandler<UpdateSpecificMethodologyRequirement>
    {
        public UpdateSpecificMethodologyAuthorizationHandler() : base(SecurityClaimTypes.UpdateAllMethodologies)
        {
        }
    }
}
