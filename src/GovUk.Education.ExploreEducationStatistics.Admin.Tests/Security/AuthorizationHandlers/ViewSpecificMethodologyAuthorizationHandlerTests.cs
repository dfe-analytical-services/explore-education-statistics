#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IReleaseRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ViewSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly MethodologyVersion MethodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid(),
            Status = MethodologyApprovalStatus.Approved,
        };

        private static readonly Publication OwningPublication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimsTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanViewAnyMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        _,
                        releaseRepository
                        ) = CreateHandlerAndDependencies();

                    // Only the AccessAllMethodologies claim should allow a Methodology to be viewed.
                    var expectedToPassByClaimAlone = claim == AccessAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        methodologyRepository.Setup(s =>
                                s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(new List<Guid> { OwningPublication.Id });

                        releaseRepository.Setup(s =>
                                s.GetLatestReleaseVersion(OwningPublication.Id, default))
                            .ReturnsAsync((ReleaseVersion?) null);

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());

                        userReleaseRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<ReleaseRole>());
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        releaseRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnerCanViewMethodology()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        _,
                        _,
                        releaseRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    var expectedToPassByRole = ListOf(PublicationRole.Owner, PublicationRole.Approver)
                        .Contains(publicationRole);

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(publicationRole));

                    if (!expectedToPassByRole)
                    {
                        releaseRepository
                            .Setup(s => s.GetLatestReleaseVersion(OwningPublication.Id, default))
                            .ReturnsAsync((ReleaseVersion?) null);
                    }

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(
                        methodologyRepository,
                        releaseRepository,
                        userPublicationRoleRepository);

                    // As the user has Publication Owner role on the owning Publication of this Methodology, they are
                    // allowed to view it.
                    Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task EditorsOrApproversOnAnyOwningPublicationReleaseCanViewMethodology()
            {
                var expectedReleaseRolesToPass =
                    ListOf(ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.Lead);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var expectedToPassByReleaseRole = expectedReleaseRolesToPass.Contains(releaseRole);

                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        _,
                        releaseRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    if (!expectedToPassByReleaseRole)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(new List<Guid> { OwningPublication.Id });

                        releaseRepository
                            .Setup(s => s.GetLatestReleaseVersion(OwningPublication.Id, default))
                            .ReturnsAsync((ReleaseVersion?) null);
                    }

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<PublicationRole>());

                    userReleaseRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(releaseRole));

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(
                        methodologyRepository,
                        releaseRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository);

                    // As the user has a role on any Release of the owning Publication of this Methodology,
                    // they are allowed to view it.
                    Assert.Equal(expectedToPassByReleaseRole, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task PrereleaseViewersOnAnyPublicationsLatestReleaseCanViewMethodology()
            {
                // Setup an approved but unpublished release version that can be in prerelease
                var preReleaseForConnectedPublication = new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    PublicationId = Guid.NewGuid(),
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    Published = null
                };

                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseService,
                    releaseRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { preReleaseForConnectedPublication.PublicationId });

                releaseRepository
                    .Setup(s => s.GetLatestReleaseVersion(preReleaseForConnectedPublication.PublicationId, default))
                    .ReturnsAsync(preReleaseForConnectedPublication);

                userReleaseRoleRepository
                    .Setup(s => s.HasUserReleaseRole(UserId,
                        preReleaseForConnectedPublication.Id,
                        ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(true);

                // Set up the release version to be within the prerelease window
                preReleaseService
                    .Setup(s => s.GetPreReleaseWindowStatus(
                        It.Is<ReleaseVersion>(rv => rv.Id == preReleaseForConnectedPublication.Id),
                        It.IsAny<DateTime>()))
                    .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Within });

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    releaseRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseService);

                // As the user has the PrereleaseViewer role on a most recent release by time series for a publication
                // using the methodology version, and the release is approved but unpublished and within the prerelease
                // window, they are allowed to view it.
                Assert.True(authContext.HasSucceeded);
            }

            [Fact]
            public async Task
                PrereleaseViewersOnAnyPublicationsLatestReleaseCannotViewMethodology_AllReleasesAreOutsideWindow()
            {
                // Setup an approved but unpublished release version that can be in prerelease
                var preReleaseForConnectedPublication = new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    PublicationId = Guid.NewGuid(),
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    Published = null
                };

                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseService,
                    releaseRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { preReleaseForConnectedPublication.PublicationId });

                releaseRepository
                    .Setup(s => s.GetLatestReleaseVersion(preReleaseForConnectedPublication.PublicationId, default))
                    .ReturnsAsync(preReleaseForConnectedPublication);

                userReleaseRoleRepository
                    .Setup(s => s.HasUserReleaseRole(UserId,
                        preReleaseForConnectedPublication.Id,
                        ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(true);

                // Set up the release version to be outside the prerelease window
                preReleaseService
                    .Setup(s => s.GetPreReleaseWindowStatus(
                        It.Is<ReleaseVersion>(rv => rv.Id == preReleaseForConnectedPublication.Id),
                        It.IsAny<DateTime>()))
                    .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Before });

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    releaseRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseService);

                // As none of the publications using the methodology version have a most recent release by time series
                // which is approved but unpublished, and within the prerelease window, the user is not allowed to view
                // it regardless of whether they have the PrereleaseViewer role on any release.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task
                PrereleaseViewersOnAnyPublicationsLatestReleaseCannotViewMethodology_AllReleasesAreDraft()
            {
                // Setup a draft release version that cannot be in prerelease
                var latestReleaseVersion = new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    PublicationId = Guid.NewGuid(),
                    ApprovalStatus = ReleaseApprovalStatus.Draft
                };

                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    _,
                    releaseRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { latestReleaseVersion.PublicationId });

                releaseRepository
                    .Setup(s => s.GetLatestReleaseVersion(latestReleaseVersion.PublicationId, default))
                    .ReturnsAsync(latestReleaseVersion);

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    releaseRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository);

                // As none of the publications using the methodology version have a most recent release by time series
                // which is approved but unpublished, the user is not allowed to view it regardless of whether they have
                // the PrereleaseViewer role on any release.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task
                PrereleaseViewersOnAnyPublicationsLatestReleaseCannotViewMethodology_AllReleasesArePublished()
            {
                // Setup a published release version that cannot be in prerelease
                var latestReleaseVersion = new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    PublicationId = Guid.NewGuid(),
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    Published = DateTime.UtcNow
                };

                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    _,
                    releaseRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { latestReleaseVersion.PublicationId });

                releaseRepository
                    .Setup(s => s.GetLatestReleaseVersion(latestReleaseVersion.PublicationId, default))
                    .ReturnsAsync(latestReleaseVersion);

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    releaseRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository);

                // As none of the publications using the methodology version have a most recent release by time series
                // which is approved but unpublished, the user is not allowed to view it regardless of whether they have
                // the PrereleaseViewer role on any release.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task PrereleaseViewersOnAnyPublicationsLatestReleaseCannotViewMethodology_NoLatestReleases()
            {
                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    _,
                    releaseRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { OwningPublication.Id });

                releaseRepository
                    .Setup(s => s.GetLatestReleaseVersion(OwningPublication.Id, default))
                    .ReturnsAsync((ReleaseVersion?) null);

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    releaseRepository);

                // As there are no latest Releases for the Owning Publication, the user cannot be a Prerelease Viewer
                // on it.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UsersWithNoRolesOnAnyOwningPublicationReleaseCannotViewMethodology()
            {
                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    _,
                    releaseRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { OwningPublication.Id });

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                releaseRepository
                    .Setup(s => s.GetLatestReleaseVersion(OwningPublication.Id, default))
                    .ReturnsAsync((ReleaseVersion?) null);

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    releaseRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository);

                // A user with no roles on the owning Publication of this Methodology is not allowed to view it.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static (
            ViewSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IUserPublicationRoleRepository>,
            Mock<IUserReleaseRoleRepository>,
            Mock<IPreReleaseService>,
            Mock<IReleaseRepository>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            var preReleaseService = new Mock<IPreReleaseService>(Strict);
            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            var handler = new ViewSpecificMethodologyAuthorizationHandler(
                methodologyRepository.Object,
                userReleaseRoleRepository.Object,
                preReleaseService.Object,
                releaseRepository.Object,
                new AuthorizationHandlerService(
                    new ReleaseRepository(InMemoryApplicationDbContext()),
                    userReleaseRoleRepository.Object,
                    userPublicationRoleRepository.Object,
                    Mock.Of<IPreReleaseService>(Strict))
            );

            return (
                handler,
                methodologyRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository,
                preReleaseService,
                releaseRepository
            );
        }
    }
}
