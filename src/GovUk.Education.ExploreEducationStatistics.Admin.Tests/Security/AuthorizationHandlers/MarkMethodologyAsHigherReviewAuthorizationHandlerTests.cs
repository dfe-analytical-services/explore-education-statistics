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
        public async Task NoClaimsAllowMarkingLatestPublishedMethodologyVersionHigherReview()
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
                        mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(true);

                var user = CreateClaimsPrincipal(UserId, claim);
                var authContext =
                    CreateAuthorizationHandlerContext
                        <MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                        (user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyVersionRepository);

                Assert.False(authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task UserWithCorrectClaimCanMarkNonLatestPublishedMethodologyVersionHigherReview()
        {
            await ForEachSecurityClaimAsync(async claim =>
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(false);

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

                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<ReleaseRole>());
                }

                var user = CreateClaimsPrincipal(UserId, claim);
                var authContext =
                    CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                        (user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository);

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
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(DraftMethodologyVersion))
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
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<ReleaseRole>());
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
                    userReleaseRoleRepository,
                    userPublicationRoleRepository);

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
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(ApprovedMethodologyVersion))
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
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                        .ReturnsAsync(new List<ReleaseRole>());
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
                    userReleaseRoleRepository,
                    userPublicationRoleRepository);

                Assert.Equal(expectedToPassByPublicationRole, authContext.HasSucceeded);
            });
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task ReleaseEditorAndApproverRolesOnAnyOwningPublicationReleaseCanMarkDraftMethodologyHigherReview()
        {
            await ForEachReleaseRoleAsync(async releaseRole =>
            {
                var expectedToPassByReleaseRole = ReleaseEditorAndApproverRoles.Contains(releaseRole);

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
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
                    userPublicationRoleRepository);

                Assert.Equal(expectedToPassByReleaseRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task ApproversOnAnyOwningPublicationReleaseCanMarkApprovedMethodologyHigherReview()
        {
            await ForEachReleaseRoleAsync(async releaseRole =>
            {
                var expectedToPassByReleaseRole = releaseRole == ReleaseRole.Approver;

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock =>
                        mock.IsLatestPublishedVersion(ApprovedMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(mock =>
                        mock.GetOwningPublication(ApprovedMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
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
                    userPublicationRoleRepository);

                Assert.Equal(expectedToPassByReleaseRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task NoReleaseRolesOnOwningPublicationReleasesSoCannotMarkMethodologyHigherReview()
        {
            var (
                handler,
                methodologyRepository,
                methodologyVersionRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository
                ) = CreateHandlerAndDependencies();

            methodologyVersionRepository.Setup(mock =>
                    mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                .ReturnsAsync(false);

            methodologyRepository.Setup(s =>
                    s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                .ReturnsAsync(OwningPublication);

            userPublicationRoleRepository
                .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                .ReturnsAsync(new List<PublicationRole>());

            userReleaseRoleRepository
                .Setup(mock =>
                    mock.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                .ReturnsAsync(new List<ReleaseRole>());

            var user = CreateClaimsPrincipal(UserId);

            var authContext =
                CreateAuthorizationHandlerContext<MarkMethodologyAsHigherLevelReviewRequirement, MethodologyVersion>
                    (user, DraftMethodologyVersion);

            await handler.HandleAsync(authContext);
            VerifyAllMocks(
                methodologyRepository,
                methodologyVersionRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository);

            Assert.False(authContext.HasSucceeded);
        }
    }

    private static (
        MarkMethodologyAsHigherLevelReviewAuthorizationHandler,
        Mock<IMethodologyRepository>,
        Mock<IMethodologyVersionRepository>,
        Mock<IUserReleaseRoleRepository>,
        Mock<IUserPublicationRoleRepository>
        )
        CreateHandlerAndDependencies()
    {
        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

        var handler = new MarkMethodologyAsHigherLevelReviewAuthorizationHandler(
            methodologyVersionRepository.Object,
            methodologyRepository.Object,
            new AuthorizationHandlerResourceRoleService(
                userReleaseRoleRepository.Object,
                userPublicationRoleRepository.Object)
        );

        return (
            handler,
            methodologyRepository,
            methodologyVersionRepository,
            userReleaseRoleRepository,
            userPublicationRoleRepository
        );
    }
}