#nullable enable
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
    public class CreateMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class CreateMethodologyForSpecificPublicationAuthorizationHandler
        : AuthorizationHandler<CreateMethodologyForSpecificPublicationRequirement, Publication>
    {
        private readonly ContentDbContext _context;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public CreateMethodologyForSpecificPublicationAuthorizationHandler(
            ContentDbContext context,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _context = context;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CreateMethodologyForSpecificPublicationRequirement requirement,
            Publication publication)
        {
            // No user is allowed to create a new methodology of an archived or to-be-archived publication
            if (publication.SupersededById.HasValue)
            {
                return;
            }

            // If a publication owns a methodology already, they cannot own another
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

            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        publication.Id,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
