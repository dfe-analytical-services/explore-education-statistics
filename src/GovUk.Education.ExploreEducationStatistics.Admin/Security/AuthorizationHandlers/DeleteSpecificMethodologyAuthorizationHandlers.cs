using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public DeleteSpecificMethodologyAuthorizationHandler(
            ContentDbContext context,
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _context = context;
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DeleteSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            if (!methodology.Amendment || methodology.PubliclyAccessible)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, DeleteAllMethodologyAmendments))
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
            
            // If the user has a Publication Owner role on a Publication that uses this Methodology, they can create 
            // an Amendment of this Methodology.
            if (publications.IsNullOrEmpty())
            {
                return;
            }

            // TODO: this will need changing in the future to only allow owning Publications the permission to make
            // Amendments, rather than just any linked Methodologies
            foreach (var publication in publications)
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
