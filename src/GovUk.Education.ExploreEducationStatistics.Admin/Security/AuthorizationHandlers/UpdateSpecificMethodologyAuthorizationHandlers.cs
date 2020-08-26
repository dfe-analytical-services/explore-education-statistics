using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class
        UpdateSpecificMethodologyAuthorizationHandler : HasClaimAuthorizationHandler<
            UpdateSpecificMethodologyRequirement>
    {
        public UpdateSpecificMethodologyAuthorizationHandler() : base(UpdateAllMethodologies)
        {
        }
    }
}