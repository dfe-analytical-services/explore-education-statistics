#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UpdateSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly MethodologyVersion MethodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid()
        };

        private static readonly Publication OwningPublication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowUpdatingApprovedMethodology()
            {
                var methodologyVersion = new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Status = Approved
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        _,
                        _,
                        _) = CreateHandlerAndDependencies();

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                            (user, methodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    VerifyAllMocks(methodologyVersionRepository);

                    // No claims should allow an Approved Methodology to be updated
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task NoClaimsAllowUpdatingPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        _,
                        _,
                        _) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    VerifyAllMocks(methodologyVersionRepository);

                    // No claims should allow a publicly accessible Methodology to be updated
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanUpdateNonPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository,
                        _,
                        publicationRepository) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(false);

                    // Only the UpdateAllMethodologies claim should allow a non publicly accessible Methodology
                    // to be updated.
                    var expectedToPassByClaimAlone = claim == UpdateAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());

                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                            .ReturnsAsync((Release?)null);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    VerifyAllMocks(
                        methodologyRepository, 
                        methodologyVersionRepository,
                        userPublicationRoleRepository, 
                        publicationRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnersAndApproversCanUpdateMethodology()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository,
                        _,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(false);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(publicationRole));

                    var expectedToPassByRole = ListOf(PublicationRole.Owner, PublicationRole.Approver).Contains(publicationRole);

                    if (!expectedToPassByRole)
                    {
                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                            .ReturnsAsync((Release?)null);
                    }

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    VerifyAllMocks(
                        methodologyRepository, 
                        methodologyVersionRepository,
                        userPublicationRoleRepository, 
                        publicationRepository);

                    // As the user has Publication Owner role on the owning Publication of this Methodology, they are
                    // allowed to update it.
                    Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task EditorsOrApproversOnOwningPublicationsLatestReleaseCanUpdateMethodology()
            {
                var expectedReleaseRolesToPass = ListOf(ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.Lead);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var latestReleaseForPublication = new Release
                    {
                        Id = Guid.NewGuid()
                    };
                    
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(false);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

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
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    
                    // as they are strict
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        publicationRepository);

                    // As the user has a role on the latest Release of the owning Publication of this Methodology
                    // they are allowed to update it.
                    Assert.Equal(expectedReleaseRolesToPass.Contains(releaseRole), authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotUpdateMethodology()
            {
                var latestReleaseForPublication = new Release
                {
                    Id = Guid.NewGuid()
                };
                    
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                    .ReturnsAsync(latestReleaseForPublication);

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndRelease(UserId, latestReleaseForPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    publicationRepository);

                // A user with no roles on the owning Publication of this Methodology is not allowed to update it.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task NoLatestReleaseForPublicationSoUsersCannotUpdateMethodology()
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    _,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                    .ReturnsAsync((Release?) null);

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    publicationRepository);

                // A user with no roles on the owning Publication of this Methodology is not allowed to update it.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static (
            UpdateSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IMethodologyVersionRepository>,
            Mock<IUserPublicationRoleRepository>,
            Mock<IUserReleaseRoleRepository>,
            Mock<IPublicationRepository>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);

            var handler = new UpdateSpecificMethodologyAuthorizationHandler(
                methodologyVersionRepository.Object,
                methodologyRepository.Object,
                new AuthorizationHandlerResourceRoleService(
                    userReleaseRoleRepository.Object,
                    userPublicationRoleRepository.Object,
                    publicationRepository.Object)
                );

            return (
                handler,
                methodologyRepository,
                methodologyVersionRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository,
                publicationRepository
            );
        }
    }
}
