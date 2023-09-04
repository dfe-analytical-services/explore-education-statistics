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
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MakeAmendmentOfSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            if (!await _methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, MakeAmendmentsOfAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);
            
            if (await _authorizationHandlerResourceRoleService
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
