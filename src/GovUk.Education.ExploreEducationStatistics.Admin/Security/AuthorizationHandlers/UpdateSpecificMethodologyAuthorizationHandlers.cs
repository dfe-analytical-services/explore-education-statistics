using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler :
        HasClaimAuthorizationHandler<UpdateSpecificMethodologyRequirement>
    {
        // TODO SOW4 EES-2166: when adding in Publication Owner permissions here:
        //
        // In future the approver of the latest release of the publication which the methodology belongs to should be
        // able to unapprove the methodology as long as it's not publicly accessible yet
        public UpdateSpecificMethodologyAuthorizationHandler() : base(SecurityClaimTypes.UpdateAllMethodologies)
        {
        }
    }
}
