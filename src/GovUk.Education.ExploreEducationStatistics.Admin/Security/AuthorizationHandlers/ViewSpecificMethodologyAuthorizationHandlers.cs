using System.Dynamic;
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
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class ViewSpecificMethodologyAuthorizationHandler
        : CompoundAuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
    {
        public ViewSpecificMethodologyAuthorizationHandler(IMethodologyRepository methodologyRepository) : base(
            new CanViewAllMethodologiesAuthorizationHandler(),
            new HasRoleOnAnyAssociatedReleaseAuthorizationHandler(methodologyRepository)) {}
    
        public class CanViewAllMethodologiesAuthorizationHandler 
            : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
        {
            public CanViewAllMethodologiesAuthorizationHandler()
                : base(SecurityClaimTypes.AccessAllMethodologies) {}
        }

        public class HasRoleOnAnyAssociatedReleaseAuthorizationHandler
            : AuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
        {
            private readonly IMethodologyRepository _methodologyRepository;
            public HasRoleOnAnyAssociatedReleaseAuthorizationHandler(IMethodologyRepository methodologyRepository)
            {
                _methodologyRepository = methodologyRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext authContext,
                ViewSpecificMethodologyRequirement requirement,
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
}
