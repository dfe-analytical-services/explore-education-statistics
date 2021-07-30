using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DeleteSpecificMethodologyRequirement : IAuthorizationRequirement
    {}
    
    public class DeleteSpecificMethodologyAuthorizationHandler
        : AuthorizationHandler<DeleteSpecificMethodologyRequirement, Methodology>
    {
        private readonly ContentDbContext _context;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public DeleteSpecificMethodologyAuthorizationHandler(
            ContentDbContext context,
            IMethodologyRepository methodologyRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _context = context;
            _methodologyRepository = methodologyRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DeleteSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            // If the Methodology is already public, it cannot be deleted.
            if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
            {
                return;
            }
         
            // If the Methodology is the first version added to a Publication and is still in Draft, or if it is a 
            // subsequent version but is still an amendment, it can potentially be deleted.  Otherwise it cannot.
            if (!methodology.Amendment && !methodology.DraftFirstVersion)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, DeleteAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }
            
            await _context
                .Entry(methodology)
                .Reference(p => p.MethodologyParent)
                .LoadAsync();

            await _context
                .Entry(methodology.MethodologyParent)
                .Collection(p => p.Publications)
                .LoadAsync();

            var publications = methodology.MethodologyParent.Publications;
            
            if (publications.IsNullOrEmpty())
            {
                return;
            }

            // If the user has a Publication Owner role on a Publication that owns this Methodology, they can delete 
            // this Methodology.
            foreach (var publication in publications.Where(p => p.Owner))
            {
                var publicationRoles =
                    await _userPublicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(),
                        publication.PublicationId);

                if (ContainPublicationOwnerRole(publicationRoles))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
