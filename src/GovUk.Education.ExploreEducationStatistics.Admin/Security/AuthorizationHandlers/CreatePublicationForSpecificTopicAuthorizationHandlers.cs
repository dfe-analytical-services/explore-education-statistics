using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreatePublicationForSpecificTopicRequirement : IAuthorizationRequirement
    {}

    public class CreatePublicationForSpecificTopicAuthorizationHandler :
        AuthorizationHandler<CreatePublicationForSpecificTopicRequirement, Topic>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CreatePublicationForSpecificTopicRequirement requirement, Topic resource)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.CreateAnyPublication))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}