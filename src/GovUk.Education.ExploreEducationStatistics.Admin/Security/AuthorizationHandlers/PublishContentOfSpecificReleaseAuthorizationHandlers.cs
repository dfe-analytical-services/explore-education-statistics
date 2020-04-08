using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class PublishContentOfSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class
        PublishContentOfSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<
            PublishContentOfSpecificReleaseRequirement, Release>
    {
        public PublishContentOfSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new CanPublishContentOfAllReleasesAuthorizationHandler(),
            new HasApproverRoleOnReleaseAuthorizationHandler(context))
        {
        }

        private class CanPublishContentOfAllReleasesAuthorizationHandler :
            EntityAuthorizationHandler<PublishContentOfSpecificReleaseRequirement, Release>
        {
            public CanPublishContentOfAllReleasesAuthorizationHandler() : base(ctx =>
                ctx.Entity.Status == ReleaseStatus.Approved
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.PublishContentOfAllReleases))
            {
            }
        }

        private class HasApproverRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<PublishContentOfSpecificReleaseRequirement>
        {
            public HasApproverRoleOnReleaseAuthorizationHandler(ContentDbContext context)
                : base(context, ctx => ContainsApproverRole(ctx.Roles))
            {
            }
        }
    }
}