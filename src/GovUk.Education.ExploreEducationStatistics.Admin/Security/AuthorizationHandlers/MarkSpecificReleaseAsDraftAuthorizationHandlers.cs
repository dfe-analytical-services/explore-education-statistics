using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using ReleaseStatusOverallStage = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftRequirement : IAuthorizationRequirement
    {}
    
    public class MarkSpecificReleaseAsDraftAuthorizationHandler 
        : CompoundAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement, Release>
    {
        public MarkSpecificReleaseAsDraftAuthorizationHandler(ContentDbContext context, IReleaseStatusRepository releaseStatusRepository) : base(
            new CanMarkAllReleasesAsDraftAuthorizationHandler(releaseStatusRepository),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanMarkAllReleasesAsDraftAuthorizationHandler 
            : AuthorizationHandler<MarkSpecificReleaseAsDraftRequirement, Release>
        {
            private readonly IReleaseStatusRepository _releaseStatusRepository;
            
            public CanMarkAllReleasesAsDraftAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository)
            {
                _releaseStatusRepository = releaseStatusRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context, 
                MarkSpecificReleaseAsDraftRequirement requirement,
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

                if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllReleasesAsDraft)) 
                {
                    context.Succeed(requirement);
                }
            }
        }
    
        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}