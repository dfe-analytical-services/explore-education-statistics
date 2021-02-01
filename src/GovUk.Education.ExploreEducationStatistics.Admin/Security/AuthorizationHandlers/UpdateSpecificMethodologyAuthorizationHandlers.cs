using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler
        : CompoundAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        public UpdateSpecificMethodologyAuthorizationHandler(IMethodologyRepository methodologyRepository)
            : base(
                new UpdateAllSpecificMethodologiesAuthorizationHandler(),
                new HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler(methodologyRepository)) {}
    }

    public class UpdateAllSpecificMethodologiesAuthorizationHandler
        : HasClaimAuthorizationHandler<UpdateSpecificMethodologyRequirement>
    {
        public UpdateAllSpecificMethodologiesAuthorizationHandler()
            : base(SecurityClaimTypes.UpdateAllMethodologies) {}
    }

    public class HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        private readonly IMethodologyRepository _methodologyRepository;

        public HasNonPrereleaseRoleOnAnyAssociatedReleaseAuthorizationHandler(
            IMethodologyRepository methodologyRepository)
        {
            _methodologyRepository = methodologyRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext authContext,
            UpdateSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            if(await _methodologyRepository.UserHasReleaseRoleAssociatedWithMethodology(
                authContext.User.GetUserId(),
                methodology))    
            {
                authContext.Succeed(requirement);
            }
        }
    }
}
