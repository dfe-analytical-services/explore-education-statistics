using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
        public PublishSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new CanPublishAllReleasesAuthorizationHandler(),
            new HasApproverRoleOnReleaseAuthorizationHandler(context))
        {
        }

        private class CanPublishAllReleasesAuthorizationHandler :
            EntityAuthorizationHandler<PublishSpecificReleaseRequirement, Release>
        {
            public CanPublishAllReleasesAuthorizationHandler() : base(ctx =>
                ctx.Entity.Status == ReleaseStatus.Approved
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.PublishAllReleases))
            {
            }
        }

        private class HasApproverRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<PublishSpecificReleaseRequirement>
        {
            public HasApproverRoleOnReleaseAuthorizationHandler(ContentDbContext context)
                : base(context, ctx => ContainsApproverRole(ctx.Roles))
            {
            }
        }
    }
}