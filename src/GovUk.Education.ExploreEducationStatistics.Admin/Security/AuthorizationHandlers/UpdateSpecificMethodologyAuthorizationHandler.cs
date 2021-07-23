using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler : 
        AuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        private readonly IMethodologyRepository _methodologyRepository;

        public UpdateSpecificMethodologyAuthorizationHandler(IMethodologyRepository methodologyRepository)
        {
            _methodologyRepository = methodologyRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            // If the Methodology is already public, it cannot be updated.
            if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, UpdateAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            // TODO SOW4 EES-2166 When Status is Approved, succeed for Approvers on the latest Release of the owning Publication
            // TODO SOW4 EES-2170 When Status is not Approved, succeed for Publication owners
            // TODO SOW4 EES-2168 When Status is not Approved, succeed for Editors on the latest Release of the owning Publication
        }
    }
}
