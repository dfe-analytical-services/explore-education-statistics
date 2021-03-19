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
            : base(new CanUpdateAllSpecificMethodologies())
        {
        }
        public class CanUpdateAllSpecificMethodologies
            : HasClaimAuthorizationHandler<UpdateSpecificMethodologyRequirement>
        {
            public CanUpdateAllSpecificMethodologies()
                : base(SecurityClaimTypes.UpdateAllMethodologies) {}
        }

    }
}
