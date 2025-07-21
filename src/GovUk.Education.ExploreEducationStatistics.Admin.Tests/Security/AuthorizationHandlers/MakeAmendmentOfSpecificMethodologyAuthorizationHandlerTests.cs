#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests
{
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly MethodologyVersion MethodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = Immediately,
            MethodologyId = Guid.NewGuid()
        };

        private static readonly Publication OwningPublication = new() { Id = Guid.NewGuid() };

        private static readonly DataFixture DataFixture = new();

        public class ClaimTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanCreateAmendmentOfLatestPublishedMethodologyVersion()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleAndInviteManager
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(MethodologyVersion))
                        .ReturnsAsync(true);

                    // Only the MakeAmendmentsOfAllMethodologies claim alongside the publicly-accessible Methodology
                    // should allow creating an Amendment
                    var expectedToPassByClaimAlone = claim == MakeAmendmentsOfAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(mock =>
                                mock.GetOwningPublication(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());
                    }

                    var user = DataFixture
                        .AuthenticatedUser(userId: UserId)
                        .WithClaim(claim.ToString());

                    var authContext = CreateAuthContext(user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                        userPublicationRoleAndInviteManager);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotCreateAmendmentIfNotLatestPublishedMethodologyVersion()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        userPublicationRoleAndInviteManager
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(MethodologyVersion))
                        .ReturnsAsync(false);

                    var user = DataFixture
                        .AuthenticatedUser(userId: UserId)
                        .WithClaim(claim.ToString());

                    var authContext = CreateAuthContext(user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository, userPublicationRoleAndInviteManager);

                    // Verify that no Claims can allow a user to create an Amendment of a non-publicly-accessible
                    // Methodology.
                    Assert.False(authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task
                UserWithLinkedPublicationOwnerRoleCanCreateAmendmentOfLatestPublishedMethodologyVersionOwnedByPublication()
            {
                await GetEnums<PublicationRole>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async role =>
                    {
                        var (
                            handler,
                            methodologyRepository,
                            methodologyVersionRepository,
                            userPublicationRoleAndInviteManager
                            ) = CreateHandlerAndDependencies();

                        methodologyVersionRepository.Setup(mock =>
                                mock.IsLatestPublishedVersion(MethodologyVersion))
                            .ReturnsAsync(true);

                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(ListOf(role));

                        var user = DataFixture.AuthenticatedUser(userId: UserId);
                        var authContext = CreateAuthContext(user, MethodologyVersion);

                        await handler.HandleAsync(authContext);
                        VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                            userPublicationRoleAndInviteManager);

                        // Verify that the user can create an Amendment, as they have a Publication Owner role on a Publication
                        // that uses this Methodology.
                        Assert.Equal(role == Owner, authContext.HasSucceeded);
                    });
            }

            [Fact]
            public async Task
                UserWithoutLinkedPublicationOwnerRoleCannotCreateAmendmentOfLatestPublishedMethodologyVersion()
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleAndInviteManager
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(MethodologyVersion))
                    .ReturnsAsync(true);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleAndInviteManager
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                var user = DataFixture.AuthenticatedUser(userId: UserId);
                var authContext = CreateAuthContext(user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, methodologyVersionRepository, userPublicationRoleAndInviteManager);

                // Verify that the user cannot create an Amendment, as they don't have a Publication Owner role on a
                // Publication that uses this Methodology.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task
                UserWithLinkedPublicationOwnerRoleCannotCreateAmendmentIfNotLatestPublishedMethodologyVersion()
            {
                var (
                    handler,
                    _,
                    methodologyVersionRepository,
                    userPublicationRoleAndInviteManager) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(MethodologyVersion))
                    .ReturnsAsync(false);

                var user = DataFixture.AuthenticatedUser(userId: UserId);
                var authContext = CreateAuthContext(user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyVersionRepository, userPublicationRoleAndInviteManager);

                // Verify that the user cannot create an Amendment, as even though they have the Publication Owner role
                // on one of the Publications that use this Methodology, this Methodology is not yet publicly
                // accessible.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user,
            MethodologyVersion methodologyVersion)
        {
            return CreateAuthorizationHandlerContext<MakeAmendmentOfSpecificMethodologyRequirement, MethodologyVersion>
                (user, methodologyVersion);
        }

        private static (MakeAmendmentOfSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IMethodologyVersionRepository>,
            Mock<IUserPublicationRoleAndInviteManager>)
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var userPublicationRoleAndInviteManager = new Mock<IUserPublicationRoleAndInviteManager>(Strict);

            var handler = new MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
                methodologyVersionRepository.Object,
                methodologyRepository.Object,
                new AuthorizationHandlerService(
                    new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                    Mock.Of<IUserReleaseRoleAndInviteManager>(Strict),
                    userPublicationRoleAndInviteManager.Object,
                    Mock.Of<IPreReleaseService>(Strict)));

            return (handler, methodologyRepository, methodologyVersionRepository, userPublicationRoleAndInviteManager);
        }
}
