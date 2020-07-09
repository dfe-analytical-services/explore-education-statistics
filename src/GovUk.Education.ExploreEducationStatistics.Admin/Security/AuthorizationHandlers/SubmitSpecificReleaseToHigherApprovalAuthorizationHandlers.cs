using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using ReleaseStatusOverallStage = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class SubmitSpecificReleaseToHigherReviewRequirement : IAuthorizationRequirement
    {}
    
    public class SubmitSpecificReleaseToHigherReviewAuthorizationHandler 
        : CompoundAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement, Release>
    {
        public SubmitSpecificReleaseToHigherReviewAuthorizationHandler(ContentDbContext context, IReleaseStatusRepository releaseStatusRepository) : base(
            new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(releaseStatusRepository),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanSubmitAllReleasesToHigherReviewAuthorizationHandler 
            : AuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement, Release>
        {
            private IReleaseStatusRepository _releaseStatusRepository;

            public CanSubmitAllReleasesToHigherReviewAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository)
            {
                _releaseStatusRepository = releaseStatusRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context, 
                SubmitSpecificReleaseToHigherReviewRequirement requirement,
                Release release)
            {
                var statuses = await _releaseStatusRepository.GetAllByOverallStage(
                    release.Id, 
                    ReleaseStatusOverallStage.Started, 
                    ReleaseStatusOverallStage.Complete
                );

                if (statuses.Any() || release.Published != null)
                {
                    return;
                }

                if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview)) 
                {
                    context.Succeed(requirement);
                }
            }
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