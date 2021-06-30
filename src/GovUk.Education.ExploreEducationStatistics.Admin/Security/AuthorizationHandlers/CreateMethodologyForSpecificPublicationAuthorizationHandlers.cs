using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
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
            // If a Publication is linked to an External Methodology, this should be unlinked first.
            if (publication.ExternalMethodology != null)
            {
                return;
            }
            
            await _context
                .Entry(publication)
                .Collection(p => p.Methodologies)
                .LoadAsync();
            
            // If a Publication owns a Methodology already, they cannot own another.
            if (publication.Methodologies.Any(m => m.Owner))
            {
                return;
            }
            
            if (SecurityUtils.HasClaim(context.User, CreateAnyMethodology))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles =
                await _userPublicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(), publication.Id);

            if (ContainPublicationOwnerRole(publicationRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
