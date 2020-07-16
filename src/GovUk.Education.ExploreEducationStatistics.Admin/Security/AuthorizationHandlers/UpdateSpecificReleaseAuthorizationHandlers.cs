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
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {}

    public class UpdateSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        public UpdateSpecificReleaseAuthorizationHandler(ContentDbContext context, IReleaseStatusRepository releaseStatusRepository) : base(
            new CanUpdateAllReleasesAuthorizationHandler(releaseStatusRepository),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    
        public class CanUpdateAllReleasesAuthorizationHandler : AuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
        {
            private readonly IReleaseStatusRepository _releaseStatusRepository;

            public CanUpdateAllReleasesAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository) 
            {
                _releaseStatusRepository = releaseStatusRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context, 
                UpdateSpecificReleaseRequirement requirement,
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

                if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllReleases)) 
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<UpdateSpecificReleaseRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}