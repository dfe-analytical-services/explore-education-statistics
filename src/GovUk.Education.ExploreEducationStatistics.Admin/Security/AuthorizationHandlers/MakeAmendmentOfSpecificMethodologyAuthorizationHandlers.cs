#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class MakeAmendmentOfSpecificMethodologyAuthorizationHandler
        : AuthorizationHandler<MakeAmendmentOfSpecificMethodologyRequirement, Methodology>
    {
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
            IMethodologyRepository methodologyRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _methodologyRepository = methodologyRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MakeAmendmentOfSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            // Amendments can only be created from Methodologies that are already publicly-accessible.
            if (!await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
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

            var owningPublication =
                await _methodologyRepository.GetOwningPublicationByMethodologyParent(methodology.MethodologyParentId);

            // If the user is a Publication Owner of the Publication that owns this Methodology, they can create 
            // an Amendment of this Methodology.
            if (await _userPublicationRoleRepository.IsUserPublicationOwner(
                context.User.GetUserId(),
                owningPublication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
