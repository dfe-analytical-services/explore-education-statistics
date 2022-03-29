#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

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
            Status = MethodologyStatus.Approved,
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
                        contentDbContext,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        preReleaseService
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
                            .ReturnsAsync(new List<Guid>{ OwningPublication.Id });

                        userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                            UserId, OwningPublication, false);

                        userReleaseRoleRepository
                            .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);

                        userReleaseRoleRepository
                            .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);

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
                        contentDbContext,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        preReleaseService
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                        UserId, OwningPublication, publicationRole == Owner);

                    var expectedToPassByRole = publicationRole == Owner;

                    if (!expectedToPassByRole)
                    {
                        userReleaseRoleRepository
                            .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);

                    // As the user has Publication Owner role on the owning Publication of this Methodology, they are
                    // allowed to view it.
                    Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task EditorsOrApproversOnOwningPublicationsLatestReleaseCanViewMethodology()
            {
                var expectedReleaseRolesToPass = ListOf(Approver, Contributor, Lead);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var (
                        handler,
                        contentDbContext,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        preReleaseService
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                        methodologyRepository.Setup(s =>
                                s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(new List<Guid>{ OwningPublication.Id });

                    userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                        UserId, OwningPublication, false);
                    
                    userReleaseRoleRepository
                        .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                        .ReturnsAsync(expectedReleaseRolesToPass.Contains(releaseRole));

                    userReleaseRoleRepository
                        .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, OwningPublication.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository);

                    // As the user has a role on the latest Release of the owning Publication of this Methodology
                    // they are allowed to view it.
                    Assert.Equal(expectedReleaseRolesToPass.Contains(releaseRole), authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task PrereleaseViewersOnOwningPublicationsLatestReleaseCanViewMethodology()
            {
                var expectedReleaseRolesToPass = ListOf(PrereleaseViewer);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
                    var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
                    var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
                    var preReleaseService = new Mock<IPreReleaseService>(Strict);

                    var release = new Release
                    {
                        ReleaseName = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublicationId = OwningPublication.Id,
                    };

                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddRangeAsync(
                            OwningPublication,
                            release);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var handler = new ViewSpecificMethodologyAuthorizationHandler(
                            contentDbContext,
                            methodologyRepository.Object,
                            userPublicationRoleRepository.Object,
                            userReleaseRoleRepository.Object,
                            preReleaseService.Object
                        );

                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        methodologyRepository.Setup(s =>
                                s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(new List<Guid> { OwningPublication.Id });

                        userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                            UserId, OwningPublication, false);

                        userReleaseRoleRepository
                            .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);

                        userReleaseRoleRepository
                            .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(expectedReleaseRolesToPass.Contains(releaseRole));

                        if (releaseRole == PrereleaseViewer)
                        {
                            preReleaseService
                                .Setup(s => s.GetPreReleaseWindowStatus(It.IsAny<Release>(), It.IsAny<DateTime>()))
                                .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Within });
                        }

                        var user = CreateClaimsPrincipal(UserId);
                        var authContext =
                            CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                                (user, MethodologyVersion);

                        await handler.HandleAsync(authContext);
                        VerifyAllMocks(
                            methodologyRepository,
                            userPublicationRoleRepository,
                            userReleaseRoleRepository,
                            preReleaseService);

                        // As the user has a role on the latest Release of the owning Publication of this Methodology
                        // they are allowed to view it.
                        Assert.Equal(expectedReleaseRolesToPass.Contains(releaseRole), authContext.HasSucceeded);
                    }
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotViewMethodology()
            {
                var (
                    handler,
                    contentDbContext,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseServiceUser
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { OwningPublication.Id });

                userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                    UserId, OwningPublication, false);
                
                userReleaseRoleRepository
                    .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                    .ReturnsAsync(false);

                userReleaseRoleRepository
                    .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, OwningPublication.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository);

                // A user with no roles on the owning Publication of this Methodology is not allowed to view it.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static (
            ViewSpecificMethodologyAuthorizationHandler,
            Mock<ContentDbContext>,
            Mock<IMethodologyRepository>,
            Mock<IUserPublicationRoleRepository>,
            Mock<IUserReleaseRoleRepository>,
            Mock<IPreReleaseService>
            )
            CreateHandlerAndDependencies()
        {
            var contentDbContext = new Mock<ContentDbContext>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            var preReleaseService = new Mock<IPreReleaseService>(Strict);

            var handler = new ViewSpecificMethodologyAuthorizationHandler(
                contentDbContext.Object,
                methodologyRepository.Object,
                userPublicationRoleRepository.Object,
                userReleaseRoleRepository.Object,
                preReleaseService.Object
            );

            return (
                handler,
                contentDbContext,
                methodologyRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository,
                preReleaseService
            );
        }
    }
}
