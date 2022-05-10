using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class MakeAmendmentOfSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public MakeAmendmentOfSpecificReleaseAuthorizationHandler(ContentDbContext contentDbContext,
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _contentDbContext = contentDbContext;
            _userPublicationRoleRepository = userPublicationRoleRepository;
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

            var publicationRoles =
                await _userPublicationRoleRepository.GetAllRolesByUserAndPublicationId(context.User.GetUserId(), release.PublicationId);

            if (ContainPublicationOwnerRole(publicationRoles))
            {
                context.Succeed(requirement);
            }
        }

        private static bool IsLatestVersionOfRelease(ContentDbContext context, Release release)
        {
            var releases = context.Releases.AsNoTracking()
                .Where(r => r.PublicationId == release.PublicationId && r.Id != release.Id);

            return !releases.Any(r => r.PreviousVersionId == release.Id);
        }
    }
}
