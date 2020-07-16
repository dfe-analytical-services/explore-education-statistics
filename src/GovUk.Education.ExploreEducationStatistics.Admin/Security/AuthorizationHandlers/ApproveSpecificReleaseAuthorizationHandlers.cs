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
    public class ApproveSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class ApproveSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ApproveSpecificReleaseRequirement, Release>
    {
        public ApproveSpecificReleaseAuthorizationHandler(ContentDbContext context, IReleaseStatusRepository releaseStatusRepository) : base(
            new CanApproveAllReleasesAuthorizationHandler(releaseStatusRepository),
            new HasApproverRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanApproveAllReleasesAuthorizationHandler : 
            AuthorizationHandler<ApproveSpecificReleaseRequirement, Release>
        {
            private IReleaseStatusRepository _releaseStatusRepository;

            public CanApproveAllReleasesAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository)
            {
                _releaseStatusRepository = releaseStatusRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context, 
                ApproveSpecificReleaseRequirement requirement,
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

                if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllReleases)) 
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class HasApproverRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ApproveSpecificReleaseRequirement>
        {
            public HasApproverRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsApproverRole(ctx.Roles))
            {}
        }
    }
}