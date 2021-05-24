using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificTopicRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSpecificTopicAuthorizationHandler : HasClaimAuthorizationHandler<ViewSpecificTopicRequirement>
    {
        public ViewSpecificTopicAuthorizationHandler() : base(SecurityClaimTypes.AccessAllTopics)
        {
        }
    }
}
