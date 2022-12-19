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
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.MethodologyStatusAuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ApproveSpecificMethodologyAuthorizationHandlerTests
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
            public async Task NoClaimsAllowApprovingPubliclyAccessibleMethodology()
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
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>
                            (user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository);

                    // No claims should allow a publicly accessible Methodology to be approved
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonPubliclyAccessibleDraftMethodology()
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
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(methodologyVersion.Id))
                        .ReturnsAsync(false);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPassByClaimAlone = claim == ApproveAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(methodologyVersion.MethodologyId))
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
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>
                            (user, methodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository,
                        publicationRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonPubliclyAccessibleApprovedMethodology()
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
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(methodologyVersion.Id))
                        .ReturnsAsync(false);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPassByClaimAlone = claim == ApproveAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository
                            .Setup(s => s.GetOwningPublication(methodologyVersion.MethodologyId))
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
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>
                            (user, methodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository, 
                        methodologyVersionRepository,
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }
        
        // TODO DW - possibly should allow the Publication Owner to be able to do this too?
        public class PublicationRoleTests
        {
            [Fact]
            public async Task ApproversOnOwningPublicationCanApprove()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    // If the user has the Approver role on the owning Publication, they are allowed to approve it.
                    var expectedToPassByPublicationRole = publicationRole == PublicationRole.Approver;

                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(false);

                    methodologyRepository
                        .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(ListOf(publicationRole));

                    if (!expectedToPassByPublicationRole)
                    {
                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                            .ReturnsAsync((Release?) null);
                    }

                    var user = CreateClaimsPrincipal(UserId);

                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>(
                            user,
                            MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository);

                    Assert.Equal(expectedToPassByPublicationRole, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task ApproversOnOwningPublicationsLatestReleaseCanApprove()
            {
                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    // If the user has the Approver role on the latest Release of the owning Publication of this
                    // Methodology they are allowed to approve it
                    var expectedToPassByReleaseRole = releaseRole == Approver;

                    var latestReleaseForPublication = new Release
                    {
                        Id = Guid.NewGuid()
                    };

                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(false);

                    methodologyRepository
                        .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
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
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>(
                            user,
                            MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        methodologyVersionRepository,
                        userReleaseRoleRepository,
                        userPublicationRoleRepository,
                        publicationRepository);

                    Assert.Equal(expectedToPassByReleaseRole, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotApprove()
            {
                var latestReleaseForPublication = new Release
                {
                    Id = Guid.NewGuid()
                };

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
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
                    CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    publicationRepository);

                // A user with no role on the owning Publication's latest release is not allowed to approve it
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task NoLatestReleaseOnOwningPublicationSoCannotApprove()
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                    .ReturnsAsync((Release?) null);

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, MethodologyVersion>
                        (user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    publicationRepository);

                Assert.False(authContext.HasSucceeded);
            }
        }

        private static
            (ApproveSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IMethodologyVersionRepository>,
            Mock<IUserReleaseRoleRepository>,
            Mock<IUserPublicationRoleRepository>,
            Mock<IPublicationRepository>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);

            var handler = new ApproveSpecificMethodologyAuthorizationHandler(
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
                userReleaseRoleRepository,
                userPublicationRoleRepository,
                publicationRepository
            );
        }
    }
}
