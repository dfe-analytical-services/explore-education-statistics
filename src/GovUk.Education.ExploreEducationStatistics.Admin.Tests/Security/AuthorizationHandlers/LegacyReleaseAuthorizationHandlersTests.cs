using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LegacyReleaseAuthorizationHandlersTests
    {
        public class CreateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CanCreateAnyLegacyRelease()
            {
                await AssertReleaseHandlerSucceedsWithCorrectClaims<CreateLegacyReleaseRequirement>(
                    new CreateLegacyReleaseAuthorizationHandler.CanCreateAnyLegacyRelease(), 
                    CreateAnyRelease
                );
            }
        }

        public class ViewLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CanViewAllLegacyReleases()
            {
                await AssertReleaseHandlerSucceedsWithCorrectClaims<ViewLegacyReleaseRequirement>(
                    new ViewLegacyReleaseAuthorizationHandler.CanViewAllLegacyReleases(), 
                    AccessAllReleases
                );
            }
        }

        public class UpdateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CanUpdateAllLegacyReleases()
            {
                await AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateLegacyReleaseRequirement>(
                    new UpdateLegacyReleaseAuthorizationHandler.CanUpdateAllLegacyReleases(), 
                    UpdateAllReleases
                );
            }
        }

        public class DeleteLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CanDeleteAllLegacyReleases()
            {
                await AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteLegacyReleaseRequirement>(
                    new DeleteLegacyReleaseAuthorizationHandler.CanDeleteAllLegacyReleases(), 
                    UpdateAllReleases
                );
            }
        }
    }
}
