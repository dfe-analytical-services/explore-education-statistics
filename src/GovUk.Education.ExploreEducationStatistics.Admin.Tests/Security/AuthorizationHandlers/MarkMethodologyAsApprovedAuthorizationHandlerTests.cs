#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MarkMethodologyAsApprovedAuthorizationHandlerTests
{
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly MethodologyVersion MethodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid()
        };

        private static readonly Publication OwningPublication = new() { Id = Guid.NewGuid() };

        private static readonly DataFixture DataFixture = new();

        public class ClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowApprovingLatestPublishedMethodologyVersion()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        _,
                        _) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(MethodologyVersion))
                        .ReturnsAsync(true);

                    var user = DataFixture
                        .AuthenticatedUser(userId: UserId)
                        .WithClaim(claim.ToString());

                    var authContext =
                        CreateAuthorizationHandlerContext<MarkMethodologyAsApprovedRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository);

                    // No claims should allow a publicly accessible Methodology to be approved
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonLatestPublishedDraftMethodologyVersion()
            {
                var methodologyVersion = new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    MethodologyId = Guid.NewGuid(),
                    Status = Draft
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(methodologyVersion))
                        .ReturnsAsync(false);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPassByClaimAlone = claim == ApproveAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(methodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());

                        userReleaseRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<ReleaseRole>());
                    }

                    var user = DataFixture
                        .AuthenticatedUser(userId: UserId)
                        .WithClaim(claim.ToString());

                    var authContext =
                        CreateAuthorizationHandlerContext<MarkMethodologyAsApprovedRequirement, MethodologyVersion>
                            (user, methodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleAndInviteManager,
                        userReleaseRoleAndInviteManager);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonLatestPublishedApprovedMethodologyVersion()
            {
                var methodologyVersion = new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    MethodologyId = Guid.NewGuid(),
                    Status = Approved
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(methodologyVersion))
                        .ReturnsAsync(false);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPassByClaimAlone = claim == ApproveAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository
                            .Setup(s => s.GetOwningPublication(methodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());

                        userReleaseRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<ReleaseRole>());
                    }

                    var user = DataFixture
                        .AuthenticatedUser(userId: UserId)
                        .WithClaim(claim.ToString());

                    var authContext =
                        CreateAuthorizationHandlerContext<MarkMethodologyAsApprovedRequirement, MethodologyVersion>
                            (user, methodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task ApproversOnOwningPublicationCanApprove()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    // If the user has the Approver role on the owning Publication, they are allowed to approve it.
                    var expectedToPassByPublicationRole = publicationRole == PublicationRole.Allower;

                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(MethodologyVersion))
                        .ReturnsAsync(false);

                    methodologyRepository
                        .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleAndInviteManager
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(publicationRole));

                    if (!expectedToPassByPublicationRole)
                    {
                        userReleaseRoleAndInviteManager
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<ReleaseRole>());
                    }

                    var user = DataFixture.AuthenticatedUser(userId: UserId);

                    var authContext =
                        CreateAuthorizationHandlerContext<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                            user,
                            MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager);

                    Assert.Equal(expectedToPassByPublicationRole, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task ApproversOnAnyOwningPublicationReleaseCanApprove()
            {
                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    // If the user has the Approver role on any Releases of the owning Publication of this
                    // Methodology, they are allowed to approve it.
                    var expectedToPassByReleaseRole = releaseRole == Approver;

                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock =>
                            mock.IsLatestPublishedVersion(MethodologyVersion))
                        .ReturnsAsync(false);

                    methodologyRepository
                        .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleAndInviteManager
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<PublicationRole>());

                    userReleaseRoleAndInviteManager
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(releaseRole));

                    var user = DataFixture.AuthenticatedUser(userId: UserId);

                    var authContext =
                        CreateAuthorizationHandlerContext<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                            user,
                            MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleAndInviteManager,
                        userPublicationRoleAndInviteManager);

                    Assert.Equal(expectedToPassByReleaseRole, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task NoReleaseRolesOnAnyOwningPublicationReleaseSoCannotApprove()
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleAndInviteManager,
                    userPublicationRoleAndInviteManager
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(MethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleAndInviteManager
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleAndInviteManager
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<MarkMethodologyAsApprovedRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleAndInviteManager,
                    userReleaseRoleAndInviteManager);

                Assert.False(authContext.HasSucceeded);
            }
        }

        private static
            (MarkMethodologyAsApprovedAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IMethodologyVersionRepository>,
            Mock<IUserReleaseRoleAndInviteManager>,
            Mock<IUserPublicationRoleAndInviteManager>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(Strict);
            var userPublicationRoleAndInviteManager = new Mock<IUserPublicationRoleAndInviteManager>(Strict);

            var handler = new MarkMethodologyAsApprovedAuthorizationHandler(
                methodologyVersionRepository.Object,
                methodologyRepository.Object,
                new AuthorizationHandlerService(
                    new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                    userReleaseRoleAndInviteManager.Object,
                    userPublicationRoleAndInviteManager.Object,
                    Mock.Of<IPreReleaseService>(Strict))
            );

            return (
                handler,
                methodologyRepository,
                methodologyVersionRepository,
                userReleaseRoleAndInviteManager,
                userPublicationRoleAndInviteManager
            );
        }
}
