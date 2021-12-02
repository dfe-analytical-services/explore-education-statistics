#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    PublicationAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LegacyReleaseAuthorizationHandlersTests
    {
        public class CreateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task CreateLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<Publication, CreateLegacyReleaseRequirement>(
                    contentDbContext =>
                        new CreateLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new Publication(),
                    CreateAnyRelease
                );
            }

            [Fact]
            public async Task CreateLegacyRelease_PublicationRoles()
            {
                await AssertPublicationHandlerSucceedsWithPublicationOwnerRole<CreateLegacyReleaseRequirement>(
                    contentDbContext =>
                        new CreateLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)));
            }
        }

        public class ViewLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task ViewLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, ViewLegacyReleaseRequirement>(
                    contentDbContext =>
                        new ViewLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new LegacyRelease(),
                    AccessAllReleases
                );
            }

            [Fact]
            public void ViewLegacyRelease_PublicationRoles()
            {
                var legacyRelease = new LegacyRelease
                {
                    PublicationId = Guid.NewGuid(),
                };

                AssertHandlerOnlySucceedsWithPublicationRole<ViewLegacyReleaseRequirement, LegacyRelease>(
                    legacyRelease.PublicationId,
                    legacyRelease,
                    contentDbContext => contentDbContext.Add(legacyRelease),
                    contentDbContext =>
                        new ViewLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    PublicationRole.Owner);
            }
        }

        public class UpdateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task UpdateLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, UpdateLegacyReleaseRequirement>(
                    contentDbContext =>
                        new UpdateLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new LegacyRelease(),
                    UpdateAllReleases
                );
            }

            [Fact]
            public void UpdateLegacyRelease_PublicationRoles()
            {
                var legacyRelease = new LegacyRelease
                {
                    PublicationId = Guid.NewGuid(),
                };

                AssertHandlerOnlySucceedsWithPublicationRole<UpdateLegacyReleaseRequirement, LegacyRelease>(
                    legacyRelease.PublicationId,
                    legacyRelease,
                    contentDbContext => contentDbContext.Add(legacyRelease),
                    contentDbContext =>
                        new UpdateLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    PublicationRole.Owner);
            }
        }

        public class DeleteLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task DeleteLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, DeleteLegacyReleaseRequirement>(
                    contentDbContext =>
                        new DeleteLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    new LegacyRelease(),
                    UpdateAllReleases
                );
            }

            [Fact]
            public void DeleteLegacyRelease_PublicationRoles()
            {
                var legacyRelease = new LegacyRelease
                {
                    PublicationId = Guid.NewGuid(),
                };

                AssertHandlerOnlySucceedsWithPublicationRole<DeleteLegacyReleaseRequirement, LegacyRelease>(
                    legacyRelease.PublicationId,
                    legacyRelease,
                    contentDbContext => contentDbContext.Add(legacyRelease),
                    contentDbContext =>
                        new DeleteLegacyReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext)),
                    PublicationRole.Owner);
            }
        }
    }
}
