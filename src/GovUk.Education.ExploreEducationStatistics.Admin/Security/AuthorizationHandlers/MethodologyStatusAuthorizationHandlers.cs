#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MethodologyStatusAuthorizationHandlers
    {
        public class ApproveSpecificMethodologyRequirement : IAuthorizationRequirement
        {
        }

        public class ApproveSpecificMethodologyAuthorizationHandler :
            AuthorizationHandler<ApproveSpecificMethodologyRequirement, Methodology>
        {
            private readonly IMethodologyRepository _methodologyRepository;

            public ApproveSpecificMethodologyAuthorizationHandler(IMethodologyRepository methodologyRepository)
            {
                _methodologyRepository = methodologyRepository;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                ApproveSpecificMethodologyRequirement requirement,
                Methodology methodology)
            {
                // If the Methodology is already public, it cannot be approved
                // An approved Methodology that isn't public can be approved to change attributes associated with approval  
                if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
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
