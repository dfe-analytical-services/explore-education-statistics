using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class ViewSpecificMethodologyAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
    {
        public ViewSpecificMethodologyAuthorizationHandler() : base(
            new CanViewAllMethodologiesAuthorizationHandler())
        {
            
        }
    
        public class CanViewAllMethodologiesAuthorizationHandler 
            : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
        {
            public CanViewAllMethodologiesAuthorizationHandler()
                : base(SecurityClaimTypes.AccessAllMethodologies)
            {
            
            }
        }
    }
}