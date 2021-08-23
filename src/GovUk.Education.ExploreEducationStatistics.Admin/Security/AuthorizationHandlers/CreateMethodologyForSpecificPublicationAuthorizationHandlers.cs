using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreateMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class CreateMethodologyForSpecificPublicationAuthorizationHandler
        : AuthorizationHandler<CreateMethodologyForSpecificPublicationRequirement, Publication>
    {
        private readonly ContentDbContext _context;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public CreateMethodologyForSpecificPublicationAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository,
            ContentDbContext context)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CreateMethodologyForSpecificPublicationRequirement requirement,
            Publication publication)
        {
            // If a Publication owns a Methodology already, they cannot own another.
            if (await _context.PublicationMethodologies
                .AnyAsync(pm => pm.PublicationId == publication.Id && pm.Owner))
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, CreateAnyMethodology))
            {
                context.Succeed(requirement);
                return;
            }

            if (await _userPublicationRoleRepository.IsUserPublicationOwner(
                context.User.GetUserId(),
                publication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
