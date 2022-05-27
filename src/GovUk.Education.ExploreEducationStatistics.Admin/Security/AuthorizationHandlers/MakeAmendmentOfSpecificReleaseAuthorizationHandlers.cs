using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class MakeAmendmentOfSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public MakeAmendmentOfSpecificReleaseAuthorizationHandler(
            ContentDbContext contentDbContext,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _contentDbContext = contentDbContext;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MakeAmendmentOfSpecificReleaseRequirement requirement,
            Release release)
        {
            if (!release.Live || !IsLatestVersionOfRelease(_contentDbContext, release))
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, MakeAmendmentsOfAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        release.PublicationId,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }

        // TODO DW - can this be replaced with publicationRepo.GetLatestReleaseForPublication()? 
        private static bool IsLatestVersionOfRelease(ContentDbContext context, Release release)
        {
            var releases = context.Releases.AsNoTracking()
                .Where(r => r.PublicationId == release.PublicationId && r.Id != release.Id);

            return !releases.Any(r => r.PreviousVersionId == release.Id);
        }
    }
}
