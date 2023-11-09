#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class MakeAmendmentOfSpecificMethodologyAuthorizationHandler
        : AuthorizationHandler<MakeAmendmentOfSpecificMethodologyRequirement, MethodologyVersion>
    {
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            AuthorizationHandlerService authorizationHandlerService)
        {
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _authorizationHandlerService = authorizationHandlerService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MakeAmendmentOfSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            // Amendments can only be created from Methodologies that are already publicly-accessible.
            if (!await _methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
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
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);
            
            // If the user is a Publication Owner of the Publication that owns this Methodology, they can create 
            // an Amendment of this Methodology.
            if (await _authorizationHandlerService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        owningPublication.Id,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
