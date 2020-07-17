using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class LegacyReleaseAuthorizationHandlersTests
    {
        public class CreateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public void CanCreateAnyLegacyRelease()
            {
                AssertReleaseHandlerSucceedsWithCorrectClaims<CreateLegacyReleaseRequirement>(
                    new CreateLegacyReleaseAuthorizationHandler.CanCreateAnyLegacyRelease(), 
                    CreateAnyRelease
                );
            }
        }

        public class ViewLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public void CanViewAllLegacyReleases()
            {
                AssertReleaseHandlerSucceedsWithCorrectClaims<ViewLegacyReleaseRequirement>(
                    new ViewLegacyReleaseAuthorizationHandler.CanViewAllLegacyReleases(), 
                    AccessAllReleases
                );
            }
        }

        public class UpdateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public void CanUpdateAllLegacyReleases()
            {
                AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateLegacyReleaseRequirement>(
                    new UpdateLegacyReleaseAuthorizationHandler.CanUpdateAllLegacyReleases(), 
                    UpdateAllReleases
                );
            }
        }

        public class DeleteLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public void CanDeleteAllLegacyReleases()
            {
                AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteLegacyReleaseRequirement>(
                    new DeleteLegacyReleaseAuthorizationHandler.CanDeleteAllLegacyReleases(), 
                    UpdateAllReleases
                );
            }
        }
    }
}