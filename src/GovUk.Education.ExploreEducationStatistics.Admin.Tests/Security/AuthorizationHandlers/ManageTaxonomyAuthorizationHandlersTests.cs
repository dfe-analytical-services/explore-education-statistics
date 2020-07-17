using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ManageTaxonomyAuthorizationHandlerTests
    {
        [Fact]
        public void ManageTaxonomyAuthorizationHandler()
        {
            // Assert that any users with the "ManageAllTaxonomy" claim can manage all taxonomy
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ManageTaxonomyRequirement>(
                new ManageTaxonomyAuthorizationHandler(), ManageAllTaxonomy);
        }
    }
}