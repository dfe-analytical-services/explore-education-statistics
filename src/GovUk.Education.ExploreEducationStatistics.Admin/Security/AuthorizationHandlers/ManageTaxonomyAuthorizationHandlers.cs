using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ManageTaxonomyRequirement : IAuthorizationRequirement
    {
    }

    public class ManageTaxonomyAuthorizationHandler : HasClaimAuthorizationHandler<ManageTaxonomyRequirement>
    {
        public ManageTaxonomyAuthorizationHandler() : base(SecurityClaimTypes.ManageAllTaxonomy)
        {
        }
    }
}