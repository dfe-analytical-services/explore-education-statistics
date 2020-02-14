using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class SubmitSpecificReleaseToHigherReviewRequirement : IAuthorizationRequirement
    {}
    
    public class SubmitSpecificReleaseToHigherReviewAuthorizationHandler 
        : CompoundAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement, Release>
    {
        public SubmitSpecificReleaseToHigherReviewAuthorizationHandler(ContentDbContext context) : base(
            new SubmitSpecificEntityToHigherReviewCanSubmitAllEntitiesAuthorizationHandler(),
            new SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    }
    
    public class SubmitSpecificEntityToHigherReviewCanSubmitAllEntitiesAuthorizationHandler 
        : EntityAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement, Release>
    {
        public SubmitSpecificEntityToHigherReviewCanSubmitAllEntitiesAuthorizationHandler() 
            : base(ctx => 
                ctx.Entity.Status != ReleaseStatus.Approved 
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview)) {}
    }

    public class SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement>
    {
        public SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
        {}
    }
}