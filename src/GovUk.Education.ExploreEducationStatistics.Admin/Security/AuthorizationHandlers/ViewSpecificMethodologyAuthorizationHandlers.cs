using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class ViewSpecificMethodologyAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
    {
        public ViewSpecificMethodologyAuthorizationHandler(ContentDbContext context) : base(
            new ViewSpecificMethodologyCanUpdateAllMethodologiesAuthorizationHandler())
        {
            
        }
    }
    
    public class ViewSpecificMethodologyCanUpdateAllMethodologiesAuthorizationHandler 
        : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
    {
        public ViewSpecificMethodologyCanUpdateAllMethodologiesAuthorizationHandler()
            : base(SecurityClaimTypes.AccessAllMethodologies)
        {
            
        }
    }
}