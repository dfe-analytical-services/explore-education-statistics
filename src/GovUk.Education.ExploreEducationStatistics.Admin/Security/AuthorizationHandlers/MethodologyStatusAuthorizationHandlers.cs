#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
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
            private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

            public ApproveSpecificMethodologyAuthorizationHandler(
                IMethodologyRepository methodologyRepository,
                IUserReleaseRoleRepository userReleaseRoleRepository)
            {
                _methodologyRepository = methodologyRepository;
                _userReleaseRoleRepository = userReleaseRoleRepository;
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

                var owningPublication =
                    await _methodologyRepository.GetOwningPublicationByMethodologyParent(
                        methodology.MethodologyParentId);

                // If the user is an Approver of the latest (Live or non-Live) Release for the owning Publication of
                // this Methodology, they can approve it.
                if (await _userReleaseRoleRepository.IsUserApproverOnLatestRelease(context, owningPublication.Id))
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class MarkSpecificMethodologyAsDraftRequirement : IAuthorizationRequirement
        {
        }

        public class
            MarkSpecificMethodologyAsDraftAuthorizationHandler : AuthorizationHandler<
                MarkSpecificMethodologyAsDraftRequirement, Methodology>
        {
            private readonly IMethodologyRepository _methodologyRepository;
            private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

            public MarkSpecificMethodologyAsDraftAuthorizationHandler(
                IMethodologyRepository methodologyRepository,
                IUserReleaseRoleRepository userReleaseRoleRepository)
            {
                _methodologyRepository = methodologyRepository;
                _userReleaseRoleRepository = userReleaseRoleRepository;
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

                var owningPublication =
                    await _methodologyRepository.GetOwningPublicationByMethodologyParent(
                        methodology.MethodologyParentId);

                // If the user is an Approver of the latest (Live or non-Live) Release for the owning Publication of
                // this Methodology, they can mark it as draft.
                if (await _userReleaseRoleRepository.IsUserApproverOnLatestRelease(context, owningPublication.Id))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
