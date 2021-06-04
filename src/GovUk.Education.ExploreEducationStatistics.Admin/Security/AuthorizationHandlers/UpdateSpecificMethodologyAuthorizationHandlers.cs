using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler :
        AuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UpdateSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            if (methodology.Status == Draft && HasClaim(context.User, SecurityClaimTypes.UpdateAllMethodologies))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
