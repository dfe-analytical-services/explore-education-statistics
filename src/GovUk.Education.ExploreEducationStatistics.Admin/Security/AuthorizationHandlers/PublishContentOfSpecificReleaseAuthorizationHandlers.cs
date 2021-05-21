using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class PublishSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class
        PublishSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<
            PublishSpecificReleaseRequirement, Release>
    {
        public PublishSpecificReleaseAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository) : base(
            new CanPublishAllReleasesAuthorizationHandler(),
            new HasApproverRoleOnReleaseAuthorizationHandler(userReleaseRoleRepository))
        {
        }

        private class CanPublishAllReleasesAuthorizationHandler :
            EntityAuthorizationHandler<PublishSpecificReleaseRequirement, Release>
        {
            public CanPublishAllReleasesAuthorizationHandler() : base(ctx =>
                // TODO EES-2132 what is this?
                ctx.Entity.Status == ReleaseStatus.Approved
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.PublishAllReleases))
            {
            }
        }

        private class HasApproverRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<PublishSpecificReleaseRequirement>
        {
            public HasApproverRoleOnReleaseAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository)
                : base(userReleaseRoleRepository, ctx => ContainsApproverRole(ctx.Roles))
            {
            }
        }
    }
}
