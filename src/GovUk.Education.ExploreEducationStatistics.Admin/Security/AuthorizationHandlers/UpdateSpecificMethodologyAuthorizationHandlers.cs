using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler
        : CompoundAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        public UpdateSpecificMethodologyAuthorizationHandler()
            : base(new CanUpdateAllMethodologies())
        {
        }
        public class CanUpdateAllMethodologies
            : HasClaimAuthorizationHandler<UpdateSpecificMethodologyRequirement>
        {
            public CanUpdateAllMethodologies()
                : base(SecurityClaimTypes.UpdateAllMethodologies) {}
        }

    }
}
