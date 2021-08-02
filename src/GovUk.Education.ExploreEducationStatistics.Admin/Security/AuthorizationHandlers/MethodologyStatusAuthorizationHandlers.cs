using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MethodologyStatusAuthorizationHandlers
    {
        public class ApproveSpecificMethodologyRequirement : IAuthorizationRequirement
        {
        }

        public class ApproveSpecificMethodologyAuthorizationHandler :
            AuthorizationHandler<ApproveSpecificMethodologyRequirement, Methodology>
        {
            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                ApproveSpecificMethodologyRequirement requirement,
                Methodology methodology)
            {
                if (methodology.Status == Approved)
                {
                    return;
                }

                if (SecurityUtils.HasClaim(context.User, ApproveAllMethodologies))
                {
                    context.Succeed(requirement);
                    return;
                }

                // TODO SOW4 EES-2162 Succeed for Approvers on the latest Release of the owning Publication
            }
        }

        public class MarkSpecificMethodologyAsDraftRequirement : IAuthorizationRequirement
        {
        }

        public class MarkSpecificMethodologyAsDraftAuthorizationHandler : AuthorizationHandler<MarkSpecificMethodologyAsDraftRequirement, Methodology>
        {
            private readonly IMethodologyRepository _methodologyRepository;

            public MarkSpecificMethodologyAsDraftAuthorizationHandler(IMethodologyRepository methodologyRepository)
            {
                _methodologyRepository = methodologyRepository;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                MarkSpecificMethodologyAsDraftRequirement requirement,
                Methodology methodology)
            {
                // If the Methodology is already public, it cannot be marked as draft
                if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
                {
                    return;
                }

                if (SecurityUtils.HasClaim(context.User, MarkAllMethodologiesDraft))
                {
                    context.Succeed(requirement);
                    return;
                }

                // TODO SOW4 EES-2166 Succeed for Approvers on the latest Release of the owning Publication
            }
        }
    }
}
