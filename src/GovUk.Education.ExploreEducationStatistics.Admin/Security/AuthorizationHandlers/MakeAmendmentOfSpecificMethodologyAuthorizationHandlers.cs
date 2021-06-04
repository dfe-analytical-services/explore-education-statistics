using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class MakeAmendmentOfSpecificMethodologyAuthorizationHandler
        : AuthorizationHandler<MakeAmendmentOfSpecificMethodologyRequirement, Methodology>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MakeAmendmentOfSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            // Amendments can only be created from Methodologies that are already publicly-accessible.
            if (!methodology.PubliclyAccessible)
            {
                return;
            }

            // Any user with the "MakeAmendmentsOfAllMethodologies" Claim can create an amendment of a
            // publicly-accessible Methodology.
            if (SecurityUtils.HasClaim(context.User, MakeAmendmentsOfAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            // If the user has a Publication Owner role on a Publication that uses this Methodology, they can create 
            // an Amendment of this Methodology.
            if (methodology.Publications.IsNullOrEmpty())
            {
                return;
            }

            foreach (var publication in methodology.Publications)
            {
                var publicationRoles =
                    await _userPublicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(),
                        publication.Id);

                if (ContainPublicationOwnerRole(publicationRoles))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
