using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LegacyReleaseAuthorizationHandlersTests
    {
        public class CreateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CreateLegacyRelease()
            {
                await AssertHandlerSucceedsWithCorrectClaims<Publication, CreateLegacyReleaseRequirement>(
                    contentDbContext =>
                        new CreateLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new Publication(),
                    CreateAnyRelease
                );
            }

            // TODO Publication owner test @MarkFix
        }

        public class ViewLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task ViewLegacyRelease()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, ViewLegacyReleaseRequirement>(
                    contentDbContext =>
                        new ViewLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new LegacyRelease(),
                    AccessAllReleases
                );
            }

            // TODO Publication owner test @MarkFix
        }

        public class UpdateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task UpdateLegacyRelease()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, UpdateLegacyReleaseRequirement>(
                    contentDbContext =>
                        new UpdateLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new LegacyRelease(),
                    UpdateAllReleases
                );
            }

            // TODO Publication owner test @MarkFix
        }

        public class DeleteLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CanDeleteAllLegacyReleases()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, DeleteLegacyReleaseRequirement>(
                    contentDbContext =>
                        new DeleteLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new LegacyRelease(),
                    UpdateAllReleases
                );
            }

            // TODO Publication owner test @MarkFix
        }
    }
}
