using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificMethodologyAsDraftRequirement : IAuthorizationRequirement
    {
    }

    public class
        MarkSpecificMethodologyAsDraftAuthorizationHandler : HasClaimAuthorizationHandler<
            MarkSpecificMethodologyAsDraftRequirement>
    {
        public MarkSpecificMethodologyAsDraftAuthorizationHandler() : base(MarkAllMethodologiesDraft)
        {
        }
    }
}