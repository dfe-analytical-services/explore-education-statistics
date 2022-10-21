#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LegacyReleaseAuthorizationHandlersTests
    {
        public class ManageLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task ManageLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<Publication, ManageLegacyReleasesRequirement>(
                    CreateHandler,
                    new Publication(),
                    CreateAnyRelease
                );
            }

            [Fact]
            public async Task ManageLegacyRelease_PublicationRoles()
            {
                await AssertPublicationHandlerSucceedsWithPublicationRoles<ManageLegacyReleasesRequirement>(
                    CreateHandler, Owner);
            }

            private static ManageLegacyReleasesAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
            {
                return new ManageLegacyReleasesAuthorizationHandler(
                    new AuthorizationHandlerResourceRoleService(
                        Mock.Of<IUserReleaseRoleRepository>(Strict),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPublicationRepository>(Strict)));
            }
        }

        public class ViewLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task ViewLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, ViewLegacyReleaseRequirement>(
                    CreateHandler,
                    new LegacyRelease(),
                    AccessAllReleases
                );
            }

            [Fact]
            public async Task ViewLegacyRelease_PublicationRoles()
            {
                var legacyRelease = new LegacyRelease
                {
                    PublicationId = Guid.NewGuid(),
                };

                await AssertHandlerOnlySucceedsWithPublicationRoles<ViewLegacyReleaseRequirement, LegacyRelease>(
                    legacyRelease.PublicationId,
                    legacyRelease,
                    contentDbContext => contentDbContext.Add(legacyRelease),
                    CreateHandler,
                    Owner);
            }

            private static ViewLegacyReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
            {
                return new ViewLegacyReleaseAuthorizationHandler(
                    new AuthorizationHandlerResourceRoleService(
                        Mock.Of<IUserReleaseRoleRepository>(Strict),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPublicationRepository>(Strict)));
            }
        }

        public class UpdateLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task UpdateLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, UpdateLegacyReleaseRequirement>(
                    CreateHandler,
                    new LegacyRelease(),
                    UpdateAllReleases
                );
            }

            [Fact]
            public async Task UpdateLegacyRelease_PublicationRoles()
            {
                var legacyRelease = new LegacyRelease
                {
                    PublicationId = Guid.NewGuid(),
                };

                await AssertHandlerOnlySucceedsWithPublicationRoles<UpdateLegacyReleaseRequirement, LegacyRelease>(
                    legacyRelease.PublicationId,
                    legacyRelease,
                    contentDbContext => contentDbContext.Add(legacyRelease),
                    CreateHandler,
                    Owner);
            }

            private static UpdateLegacyReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
            {
                return new UpdateLegacyReleaseAuthorizationHandler(
                    new AuthorizationHandlerResourceRoleService(
                        Mock.Of<IUserReleaseRoleRepository>(Strict),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPublicationRepository>(Strict)));
            }
        }

        public class DeleteLegacyReleaseAuthorizationHandlerTests
        {
            [Fact]
            public async Task DeleteLegacyRelease_Claims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<LegacyRelease, DeleteLegacyReleaseRequirement>(
                    CreateHandler,
                    new LegacyRelease(),
                    UpdateAllReleases
                );
            }

            [Fact]
            public async Task DeleteLegacyRelease_PublicationRoles()
            {
                var legacyRelease = new LegacyRelease
                {
                    PublicationId = Guid.NewGuid(),
                };

                await AssertHandlerOnlySucceedsWithPublicationRoles<DeleteLegacyReleaseRequirement, LegacyRelease>(
                    legacyRelease.PublicationId,
                    legacyRelease,
                    contentDbContext => contentDbContext.Add(legacyRelease),
                    CreateHandler,
                    Owner);
            }

            private static DeleteLegacyReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
            {
                return new DeleteLegacyReleaseAuthorizationHandler(
                    new AuthorizationHandlerResourceRoleService(
                        Mock.Of<IUserReleaseRoleRepository>(Strict),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPublicationRepository>(Strict)));
            }
        }
    }
}
