using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
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
            new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanSubmitAllReleasesToHigherReviewAuthorizationHandler 
            : EntityAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement, Release>
        {
            public CanSubmitAllReleasesToHigherReviewAuthorizationHandler() 
                : base(ctx => 
                {
                    if (ctx.Entity.Published != null) 
                    {
                        return false;
                    }

                    return SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview);
                }) {}
        }

        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}