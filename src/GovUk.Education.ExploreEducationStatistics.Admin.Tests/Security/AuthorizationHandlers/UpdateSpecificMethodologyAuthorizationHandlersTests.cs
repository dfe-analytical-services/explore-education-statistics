using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyAuthorizationHandlersTests
    {
        [Fact]
        public async Task UpdateAllSpecificMethodologies_DraftMethodology()
        {
            var draftMethodology = new Methodology
            {
                Status = Draft
            };
            
            await AssertHandlerSucceedsWithCorrectClaims<Methodology, UpdateSpecificMethodologyRequirement>(
                new UpdateSpecificMethodologyAuthorizationHandler(),
                draftMethodology,
                UpdateAllMethodologies
            );
        }

        [Fact]
        public async Task UpdateAllSpecificMethodologies_ApprovedMethodologyFails()
        {
            var approvedMethodology = new Methodology
            {
                Status = Approved
            };
            
            await AssertHandlerSucceedsWithCorrectClaims<Methodology, UpdateSpecificMethodologyRequirement>(
                new UpdateSpecificMethodologyAuthorizationHandler(),
                approvedMethodology
            );
        }
    }
}
