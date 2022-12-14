#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

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
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        _,
                        publicationRepository
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

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());

                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                            .ReturnsAsync((Release?)null);

                        // TODO DW - absorb this in
                        userReleaseRoleRepository
                            .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository, publicationRepository);

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
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    var expectedToPassByRole = ListOf(PublicationRole.Owner, PublicationRole.Approver).Contains(publicationRole);
                    
                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(publicationRole));

                    if (!expectedToPassByRole)
                    {
                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                            .ReturnsAsync((Release?)null);
                    }

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    VerifyAllMocks(
                        methodologyRepository, 
                        userPublicationRoleRepository, 
                        publicationRepository);

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
                var expectedReleaseRolesToPass = ListOf(ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.Lead);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var expectedToPassByReleaseRole = expectedReleaseRolesToPass.Contains(releaseRole);

                    var latestReleaseForPublication = new Release
                    {
                        Id = Guid.NewGuid()
                    };
                    
                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        _,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    if (!expectedToPassByReleaseRole)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(new List<Guid>{ OwningPublication.Id });

                        // TODO convert these
                        userReleaseRoleRepository
                            .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<PublicationRole>());

                    publicationRepository
                        .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                        .ReturnsAsync(latestReleaseForPublication);

                    userReleaseRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndRelease(UserId, latestReleaseForPublication.Id))
                        .ReturnsAsync(ListOf(releaseRole));

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    VerifyAllMocks(
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        publicationRepository);

                    // As the user has a role on the latest Release of the owning Publication of this Methodology
                    // they are allowed to view it.
                    Assert.Equal(expectedToPassByReleaseRole, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task PrereleaseViewersOnOwningPublicationsLatestReleaseCanViewMethodology()
            {
                var latestReleaseForPublication = new Release
                {
                    Id = Guid.NewGuid()
                };
                
                var preReleaseForConnectedPublication = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = Guid.NewGuid()
                };
                
                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseService,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                    .ReturnsAsync(latestReleaseForPublication);

                // TODO DW - test user having prerelease on owning publication's latest release
                // TODO DW - test user not being prerelease on any releases
                // TODO DW - test no latest releases on any connected publications
                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndRelease(UserId, latestReleaseForPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                methodologyRepository.Setup(s =>
                        s.GetAllPublicationIds(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(new List<Guid> { preReleaseForConnectedPublication.PublicationId });

                userReleaseRoleRepository
                    .Setup(s => s.IsUserPrereleaseViewerOnLatestPreReleaseRelease(UserId, preReleaseForConnectedPublication.PublicationId))
                    .ReturnsAsync(true);
                
                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(preReleaseForConnectedPublication.PublicationId))
                    .ReturnsAsync(preReleaseForConnectedPublication);
                
                preReleaseService
                    .Setup(s => s.GetPreReleaseWindowStatus(
                        It.Is<Release>(r => r.Id == preReleaseForConnectedPublication.Id),
                        It.IsAny<DateTime>()))
                    .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Within });

                var user = CreateClaimsPrincipal(UserId);
                
                var authContext =
                    CreateAuthorizationHandlerContext<ViewSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                
                VerifyAllMocks(
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    preReleaseService,
                    publicationRepository);

                // As the user has a role on the latest Release of the owning Publication of this Methodology
                // they are allowed to view it.
                Assert.True(authContext.HasSucceeded);
            }

            // TODO DW - is this a legit test case?  cos we check connected publication too
            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotViewMethodology()
            {
                var latestReleaseForPublication = new Release
                {
                    Id = Guid.NewGuid()
                };
                    
                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    _,
                    publicationRepository
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

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                    .ReturnsAsync(latestReleaseForPublication);

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndRelease(UserId, latestReleaseForPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

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
                    userReleaseRoleRepository,
                    publicationRepository);

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
            Mock<IPublicationRepository>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            var preReleaseService = new Mock<IPreReleaseService>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);

            var handler = new ViewSpecificMethodologyAuthorizationHandler(
                methodologyRepository.Object,
                userReleaseRoleRepository.Object,
                preReleaseService.Object,
                publicationRepository.Object,
                new AuthorizationHandlerResourceRoleService(
                    userReleaseRoleRepository.Object,
                    userPublicationRoleRepository.Object,
                    publicationRepository.Object)
                );

            return (
                handler,
                methodologyRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository,
                preReleaseService,
                publicationRepository
            );
        }
    }
}
