using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSpecificMethodologyAuthorizationHandler
        : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
    {
        public ViewSpecificMethodologyAuthorizationHandler() : base(SecurityClaimTypes.AccessAllMethodologies)
        {
            // TODO SOW4 EES-2603 Limit to users that should be able to view a methodology
        }
    }
}
