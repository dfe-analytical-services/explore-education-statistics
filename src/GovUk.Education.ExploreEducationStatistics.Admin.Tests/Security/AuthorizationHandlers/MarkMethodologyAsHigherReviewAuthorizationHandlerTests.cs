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
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MarkMethodologyAsHigherReviewAuthorizationHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly MethodologyVersion DraftMethodologyVersion = new()
    {
        Id = Guid.NewGuid(),
        MethodologyId = Guid.NewGuid(),
        Status = MethodologyApprovalStatus.Draft,
    };

    private static readonly MethodologyVersion ApprovedMethodologyVersion = new()
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
        public async Task NoClaimsAllowMarkingPubliclyAccessibleMethodologyHigherReview()
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

                methodologyVersionRepository.Setup(mock => 
                        mock.IsPubliclyAccessible(DraftMethodologyVersion))
                    .ReturnsAsync(true);

                var user = CreateClaimsPrincipal(UserId, claim);
                var authContext =
                    CreateAuthorizationHandlerContext
                        <MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                        (user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyVersionRepository);

                // No claims should allow a publicly accessible Methodology to be marked as draft
                Assert.False(authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task UserWithCorrectClaimCanMarkNonPubliclyAccessibleMethodologyHigherReview()
        {
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

                methodologyVersionRepository.Setup(mock => 
                        mock.IsPubliclyAccessible(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                // Only the SubmitAllMethodologiesToHigherReview claim should allow a non publicly accessible
                // Methodology to be marked as draft
                var expectedToPassByClaimAlone = claim == SubmitAllMethodologiesToHigherReview;

                if (!expectedToPassByClaimAlone)
                {
                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository
                        .Setup(mock => 
                            mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<PublicationRole>());

                    publicationRepository
                        .Setup(mock => 
                            mock.GetLatestReleaseForPublication(OwningPublication.Id))
                        .ReturnsAsync((Release?)null);
                }

                var user = CreateClaimsPrincipal(UserId, claim);
                var authContext =
                    CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                        (user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                    userReleaseRoleRepository);

                Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
            });
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task ApproversAndOwnersOnOwningPublicationCanMarkDraftMethodologyHigherReview()
        {
            await ForEachPublicationRoleAsync(async publicationRole =>
            {
                var expectedToPassByPublicationRole =
                    publicationRole is Approver or Owner;

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    _,
                    userPublicationRoleRepository,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsPubliclyAccessible(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(mock =>
                        mock.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock => 
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(ListOf(publicationRole));

                if (!expectedToPassByPublicationRole)
                {
                    publicationRepository
                        .Setup(mock => 
                            mock.GetLatestReleaseForPublication(OwningPublication.Id))
                        .ReturnsAsync((Release?)null);
                }

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext
                        <MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                        (user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    publicationRepository);

                Assert.Equal(expectedToPassByPublicationRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task ApproversOnOwningPublicationCanMarkApprovedMethodologyHigherReview()
        {
            await ForEachPublicationRoleAsync(async publicationRole =>
            {
                var expectedToPassByPublicationRole =
                    publicationRole is Approver;

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    _,
                    userPublicationRoleRepository,
                    publicationRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsPubliclyAccessible(ApprovedMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(mock =>
                        mock.GetOwningPublication(ApprovedMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(ListOf(publicationRole));

                if (!expectedToPassByPublicationRole)
                {
                    publicationRepository
                        .Setup(mock =>
                            mock.GetLatestReleaseForPublication(OwningPublication.Id))
                        .ReturnsAsync((Release?)null);
                }

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext
                        <MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                        (user, ApprovedMethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository,
                    publicationRepository);

                Assert.Equal(expectedToPassByPublicationRole, authContext.HasSucceeded);
            });
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task ReleaseEditorAndApproverRolesOnOwningPublicationsLatestReleaseCanMarkDraftMethodologyHigherReview()
        {
            await ForEachReleaseRoleAsync(async releaseRole =>
            {
                var expectedToPassByReleaseRole = ReleaseEditorAndApproverRoles.Contains(releaseRole);

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

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
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
                    CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement,
                        MethodologyVersion>(user,
                        DraftMethodologyVersion);

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
        public async Task ApproversOnOwningPublicationsLatestReleaseCanMarkApprovedMethodologyHigherReview()
        {
            await ForEachReleaseRoleAsync(async releaseRole =>
            {
                var expectedToPassByReleaseRole = releaseRole == ReleaseRole.Approver;

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

                methodologyVersionRepository.Setup(mock =>
                        mock.IsPubliclyAccessible(ApprovedMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(mock =>
                        mock.GetOwningPublication(ApprovedMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                publicationRepository
                    .Setup(mock =>
                        mock.GetLatestReleaseForPublication(OwningPublication.Id))
                    .ReturnsAsync(latestReleaseForPublication);

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndRelease(UserId, latestReleaseForPublication.Id))
                    .ReturnsAsync(ListOf(releaseRole));

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement,
                        MethodologyVersion>(user,
                        ApprovedMethodologyVersion);

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
        public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotMarkMethodologyHigherReview()
        {
            var (
                handler,
                methodologyRepository,
                methodologyVersionRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository,
                publicationRepository
                ) = CreateHandlerAndDependencies();

            methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(DraftMethodologyVersion))
                .ReturnsAsync(false);

            methodologyRepository.Setup(s =>
                    s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                .ReturnsAsync(OwningPublication);

            userPublicationRoleRepository
                .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                .ReturnsAsync(new List<PublicationRole>());

            publicationRepository
                .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                .ReturnsAsync((Release?)null);

            var user = CreateClaimsPrincipal(UserId);

            var authContext =
                CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                    (user, DraftMethodologyVersion);

            await handler.HandleAsync(authContext);
            VerifyAllMocks(methodologyRepository, methodologyVersionRepository, userReleaseRoleRepository);

            // A user with no role on the owning Publication of this Methodology is not allowed to mark it as draft
            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task NoLatestReleaseForOwningPublicationSoCannotMarkMethodologyHigherReview()
        {
            var (
                handler,
                methodologyRepository,
                methodologyVersionRepository,
                _,
                userPublicationRoleRepository,
                publicationRepository
                ) = CreateHandlerAndDependencies();

            methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(DraftMethodologyVersion))
                .ReturnsAsync(false);

            methodologyRepository.Setup(s =>
                    s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                .ReturnsAsync(OwningPublication);

            userPublicationRoleRepository
                .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                .ReturnsAsync(new List<PublicationRole>());

            publicationRepository
                .Setup(s => s.GetLatestReleaseForPublication(OwningPublication.Id))
                .ReturnsAsync((Release?)null);

            var user = CreateClaimsPrincipal(UserId);

            var authContext =
                CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                    (user, DraftMethodologyVersion);

            await handler.HandleAsync(authContext);
            VerifyAllMocks(
                methodologyRepository,
                methodologyVersionRepository,
                userPublicationRoleRepository,
                publicationRepository);

            // A user with no role on the owning Publication of this Methodology is not allowed to mark it as draft
            Assert.False(authContext.HasSucceeded);
        }
    }

    private static (
        MarkMethodologyAsHigherLevelReviewAuthorizationHandler,
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

        var handler = new MarkMethodologyAsHigherLevelReviewAuthorizationHandler(
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
