using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificThemeRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSpecificThemeAuthorizationHandler : HasClaimAuthorizationHandler<ViewSpecificThemeRequirement>
    {
        public ViewSpecificThemeAuthorizationHandler() : base(SecurityClaimTypes.AccessAllTopics)
        {
        }
    }
}
