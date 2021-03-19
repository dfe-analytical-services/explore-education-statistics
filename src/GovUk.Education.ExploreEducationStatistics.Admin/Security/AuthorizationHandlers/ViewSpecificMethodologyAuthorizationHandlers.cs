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
        public ViewSpecificMethodologyAuthorizationHandler() 
            : base(new CanViewAllMethodologies()) {}
    
        public class CanViewAllMethodologies
            : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
        {
            public CanViewAllMethodologies()
                : base(SecurityClaimTypes.AccessAllMethodologies) {}
        }
    }
}
