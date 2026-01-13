#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
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

    private static readonly Publication OwningPublication = new() { Id = Guid.NewGuid() };

    private static readonly DataFixture DataFixture = new();

    public class ClaimsTests
    {
        [Fact]
        public async Task NoClaimsAllowMarkingLatestPublishedMethodologyVersionHigherReview()
        {
            await ForEachSecurityClaimAsync(async claim =>
            {
                var (handler, _, methodologyVersionRepository, _, _) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(mock => mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(true);

                var user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

                var authContext = CreateAuthorizationHandlerContext<
                    MarkMethodologyAsHigherLevelReviewRequirement,
                    MethodologyVersion
                >(user, DraftMethodologyVersion);

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

                methodologyVersionRepository
                    .Setup(mock => mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                var expectedToPassByClaimAlone = claim == SubmitAllMethodologiesToHigherReview;

                if (!expectedToPassByClaimAlone)
                {
                    methodologyRepository
                        .Setup(s => s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository
                        .Setup(mock =>
                            mock.UserHasAnyRoleOnPublication(
                                UserId,
                                OwningPublication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                new[] { PublicationRole.Owner, PublicationRole.Allower }
                            )
                        )
                        .ReturnsAsync(false);

                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasAnyRoleOnPublication(
                                UserId,
                                OwningPublication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                ReleaseEditorAndApproverRoles.ToArray()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

                var authContext = CreateAuthorizationHandlerContext<
                    MarkMethodologyAsHigherLevelReviewRequirement,
                    MethodologyVersion
                >(user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                );

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
                var isAllowerOrOwnerRole = publicationRole is PublicationRole.Allower or PublicationRole.Owner;

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(mock => mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(mock => mock.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            new[] { PublicationRole.Owner, PublicationRole.Allower }
                        )
                    )
                    .ReturnsAsync(isAllowerOrOwnerRole);

                if (!isAllowerOrOwnerRole)
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasAnyRoleOnPublication(
                                UserId,
                                OwningPublication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                ReleaseEditorAndApproverRoles.ToArray()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthorizationHandlerContext<
                    MarkMethodologyAsHigherLevelReviewRequirement,
                    MethodologyVersion
                >(user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                );

                Assert.Equal(isAllowerOrOwnerRole, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task ApproversOnOwningPublicationCanMarkApprovedMethodologyHigherReview()
        {
            await ForEachPublicationRoleAsync(async publicationRole =>
            {
                var isAllower = publicationRole is PublicationRole.Allower;

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(mock => mock.IsLatestPublishedVersion(ApprovedMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(mock => mock.GetOwningPublication(ApprovedMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            PublicationRole.Allower
                        )
                    )
                    .ReturnsAsync(isAllower);

                if (!isAllower)
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasAnyRoleOnPublication(
                                UserId,
                                OwningPublication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                ReleaseRole.Approver
                            )
                        )
                        .ReturnsAsync(false);
                }

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthorizationHandlerContext<
                    MarkMethodologyAsHigherLevelReviewRequirement,
                    MethodologyVersion
                >(user, ApprovedMethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                );

                Assert.Equal(isAllower, authContext.HasSucceeded);
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
                var isEditorOrApprover = ReleaseEditorAndApproverRoles.Contains(releaseRole);

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(mock => mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(s => s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            new[] { PublicationRole.Owner, PublicationRole.Allower }
                        )
                    )
                    .ReturnsAsync(false);

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            ReleaseEditorAndApproverRoles.ToArray()
                        )
                    )
                    .ReturnsAsync(isEditorOrApprover);

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthorizationHandlerContext<
                    MarkMethodologyAsHigherLevelReviewRequirement,
                    MethodologyVersion
                >(user, DraftMethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                );

                Assert.Equal(isEditorOrApprover, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task ApproversOnAnyOwningPublicationReleaseCanMarkApprovedMethodologyHigherReview()
        {
            await ForEachReleaseRoleAsync(async releaseRole =>
            {
                var isApprover = releaseRole == ReleaseRole.Approver;

                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(mock => mock.IsLatestPublishedVersion(ApprovedMethodologyVersion))
                    .ReturnsAsync(false);

                methodologyRepository
                    .Setup(mock => mock.GetOwningPublication(ApprovedMethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            PublicationRole.Allower
                        )
                    )
                    .ReturnsAsync(false);

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            ReleaseRole.Approver
                        )
                    )
                    .ReturnsAsync(isApprover);

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthorizationHandlerContext<
                    MarkMethodologyAsHigherLevelReviewRequirement,
                    MethodologyVersion
                >(user, ApprovedMethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(
                    methodologyRepository,
                    methodologyVersionRepository,
                    userReleaseRoleRepository,
                    userPublicationRoleRepository
                );

                Assert.Equal(isApprover, authContext.HasSucceeded);
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

            methodologyVersionRepository
                .Setup(mock => mock.IsLatestPublishedVersion(DraftMethodologyVersion))
                .ReturnsAsync(false);

            methodologyRepository
                .Setup(s => s.GetOwningPublication(DraftMethodologyVersion.MethodologyId))
                .ReturnsAsync(OwningPublication);

            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        UserId,
                        OwningPublication.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        new[] { PublicationRole.Owner, PublicationRole.Allower }
                    )
                )
                .ReturnsAsync(false);

            userReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        UserId,
                        OwningPublication.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        ReleaseEditorAndApproverRoles.ToArray()
                    )
                )
                .ReturnsAsync(false);

            var user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthorizationHandlerContext<
                MarkMethodologyAsHigherLevelReviewRequirement,
                MethodologyVersion
            >(user, DraftMethodologyVersion);

            await handler.HandleAsync(authContext);
            VerifyAllMocks(
                methodologyRepository,
                methodologyVersionRepository,
                userReleaseRoleRepository,
                userPublicationRoleRepository
            );

            Assert.False(authContext.HasSucceeded);
        }
    }

    private static (
        MarkMethodologyAsHigherLevelReviewAuthorizationHandler,
        Mock<IMethodologyRepository>,
        Mock<IMethodologyVersionRepository>,
        Mock<IUserReleaseRoleRepository>,
        Mock<IUserPublicationRoleRepository>
    ) CreateHandlerAndDependencies()
    {
        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

        var handler = new MarkMethodologyAsHigherLevelReviewAuthorizationHandler(
            methodologyVersionRepository.Object,
            methodologyRepository.Object,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleRepository.Object,
                userPublicationRoleRepository.Object,
                Mock.Of<IPreReleaseService>(Strict)
            )
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
