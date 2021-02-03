using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class ViewSpecificMethodologyAuthorizationHandler
        : CompoundAuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
    {
        public ViewSpecificMethodologyAuthorizationHandler(IMethodologyRepository methodologyRepository) : base(
            new CanViewAllMethodologies(),
            new HasRoleOnAnyAssociatedRelease(methodologyRepository)) {}
    
        public class CanViewAllMethodologies
            : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
        {
            public CanViewAllMethodologies()
                : base(SecurityClaimTypes.AccessAllMethodologies) {}
        }

        public class HasRoleOnAnyAssociatedRelease
            : AuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
        {
            private readonly IMethodologyRepository _methodologyRepository;
            public HasRoleOnAnyAssociatedRelease(IMethodologyRepository methodologyRepository)
            {
                _methodologyRepository = methodologyRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext authContext,
                ViewSpecificMethodologyRequirement requirement,
                Methodology methodology)
            {
                if (await _methodologyRepository.UserHasReleaseRoleAssociatedWithMethodology(
                    authContext.User.GetUserId(),
                    methodology.Id))
                {
                    authContext.Succeed(requirement);
                }
            }
        }
    }
}
