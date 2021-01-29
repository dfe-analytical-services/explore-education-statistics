using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler
        : CompoundAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        public UpdateSpecificMethodologyAuthorizationHandler(ContentDbContext context)
            : base(
                new UpdateAllSpecificMethodologiesAuthorizationHandler(),
                new HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler(context)) {}
    }

    public class UpdateAllSpecificMethodologiesAuthorizationHandler
        : HasClaimAuthorizationHandler<UpdateSpecificMethodologyRequirement>
    {
        public UpdateAllSpecificMethodologiesAuthorizationHandler()
            : base(SecurityClaimTypes.UpdateAllMethodologies) {}
    }

    public class HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        private readonly ContentDbContext _contentDbContext;

        public HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler(
            ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext authContext,
            UpdateSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            if (methodology.Publications == null)
            {
                methodology = await _contentDbContext.Methodologies
                    .Include(m => m.Publications)
                    .Where(m => m.Id == methodology.Id)
                    .SingleAsync();
            }

            var userId = authContext.User.GetUserId();
            var updateablePublications = await _contentDbContext.UserReleaseRoles
                    .Include(urr => urr.Release)
                    .ThenInclude(r => r.Publication)
                    .Where(urr =>
                        urr.UserId == userId
                        && urr.Role != ReleaseRole.PrereleaseViewer)
                    .Select(urr => urr.Release.Publication)
                    .Distinct()
                    .ToListAsync();

            if (updateablePublications
                .Any(p1 => methodology.Publications
                    .Select(p2 => p2.Id)
                    .Contains(p1.Id)))
            {
                authContext.Succeed(requirement);
            }
        }
    }
}
