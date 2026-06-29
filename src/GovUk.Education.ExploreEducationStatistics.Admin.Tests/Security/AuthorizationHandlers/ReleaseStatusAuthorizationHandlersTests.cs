#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class ReleaseStatusAuthorizationHandlersTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ReleaseVersion _draftReleaseVersion;
    private readonly ReleaseVersion _approvedReleaseVersion;
    private readonly ReleaseVersion _publishedReleaseVersion;

    protected ReleaseStatusAuthorizationHandlersTests()
    {
        _draftReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        _approvedReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        _publishedReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithPublished(DateTimeOffset.UtcNow)
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
    }

    public class MarkReleaseAsDraftAuthorizationHandlerTests : ReleaseStatusAuthorizationHandlersTests
    {
        public class ClaimsTests : MarkReleaseAsDraftAuthorizationHandlerTests
        {
            [Fact]
            public async Task ReleaseVersionPublished_FailsForAllClaims()
            {
                await AssertHandlerFailsForAllClaims<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handler: BuildHandler(),
                    entity: _publishedReleaseVersion,
                    userId: _userId
                );
            }

            [Fact]
            public async Task ReleaseVersionPublishing_FailsForAllClaims()
            {
                var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(
                    MockBehavior.Strict
                );
                releasePublishingStatusRepository
                    .Setup(s =>
                        s.GetAllByOverallStage(
                            _draftReleaseVersion.Id,
                            new ReleasePublishingStatusOverallStage[]
                            {
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete,
                            }
                        )
                    )
                    .ReturnsAsync([new ReleasePublishingStatus()]);

                await AssertHandlerFailsForAllClaims<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handler: BuildHandler(releasePublishingStatusRepository.Object),
                    entity: _draftReleaseVersion,
                    userId: _userId
                );
            }

            [Fact]
            public async Task UnpublishedReleaseVersion_SucceedsOnlyForValidClaims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handler: BuildHandler(),
                    entity: _draftReleaseVersion,
                    userId: _userId,
                    claimsExpectedToSucceed: [SecurityClaimTypes.MarkAllReleasesAsDraft]
                );
            }
        }

        public class PublicationRolesTests : MarkReleaseAsDraftAuthorizationHandlerTests
        {
            [Fact]
            public async Task ReleaseVersionPublished_FailsWithoutCheckingRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _publishedReleaseVersion
                );
            }

            [Fact]
            public async Task ReleaseVersionPublishing_FailsWithoutCheckingRoles()
            {
                var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(
                    MockBehavior.Strict
                );
                releasePublishingStatusRepository
                    .Setup(s =>
                        s.GetAllByOverallStage(
                            _draftReleaseVersion.Id,
                            new ReleasePublishingStatusOverallStage[]
                            {
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete,
                            }
                        )
                    )
                    .ReturnsAsync([new ReleasePublishingStatus()]);

                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(
                        releasePublishingStatusRepository: releasePublishingStatusRepository.Object,
                        authorizationHandlerService: authorizationHandlerService
                    );

                await AssertHandlerFailsWithoutCheckingRoles<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _draftReleaseVersion
                );
            }

            [Fact]
            public async Task UnpublishedDraftReleaseVersion_SucceedsOnlyForValidPublicationRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _draftReleaseVersion,
                    publicationId: _draftReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
                );
            }

            [Fact]
            public async Task UnpublishedApprovedReleaseVersion_SucceedsOnlyForValidPublicationRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<MarkReleaseAsDraftRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _approvedReleaseVersion,
                    publicationId: _approvedReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Approver]
                );
            }
        }

        private MarkReleaseAsDraftAuthorizationHandler BuildHandler(
            IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
            IAuthorizationHandlerService? authorizationHandlerService = null
        )
        {
            releasePublishingStatusRepository ??= CreateDefaultReleasePublishingStatusRepository();
            authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

            return new(releasePublishingStatusRepository, authorizationHandlerService);
        }
    }

    public class MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests : ReleaseStatusAuthorizationHandlersTests
    {
        public class ClaimsTests : MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests
        {
            [Fact]
            public async Task ReleaseVersionPublished_FailsForAllClaims()
            {
                await AssertHandlerFailsForAllClaims<MarkReleaseAsHigherLevelReviewRequirement, ReleaseVersion>(
                    handler: SetupHandler(),
                    entity: _publishedReleaseVersion,
                    userId: _userId
                );
            }

            [Fact]
            public async Task ReleaseVersionPublishing_FailsForAllClaims()
            {
                var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(
                    MockBehavior.Strict
                );
                releasePublishingStatusRepository
                    .Setup(s =>
                        s.GetAllByOverallStage(
                            _draftReleaseVersion.Id,
                            new ReleasePublishingStatusOverallStage[]
                            {
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete,
                            }
                        )
                    )
                    .ReturnsAsync([new ReleasePublishingStatus()]);

                await AssertHandlerFailsForAllClaims<MarkReleaseAsHigherLevelReviewRequirement, ReleaseVersion>(
                    handler: SetupHandler(releasePublishingStatusRepository.Object),
                    entity: _draftReleaseVersion,
                    userId: _userId
                );
            }

            [Fact]
            public async Task UnpublishedReleaseVersion_SucceedsOnlyForValidClaims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<MarkReleaseAsHigherLevelReviewRequirement, ReleaseVersion>(
                    handler: SetupHandler(),
                    entity: _draftReleaseVersion,
                    userId: _userId,
                    claimsExpectedToSucceed: [SecurityClaimTypes.SubmitAllReleasesToHigherReview]
                );
            }
        }

        public class PublicationRolesTests : MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests
        {
            [Fact]
            public async Task ReleaseVersionPublished_FailsWithoutCheckingRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<MarkReleaseAsHigherLevelReviewRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _publishedReleaseVersion
                );
            }

            [Fact]
            public async Task ReleaseVersionPublishing_FailsWithoutCheckingRoles()
            {
                var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(
                    MockBehavior.Strict
                );
                releasePublishingStatusRepository
                    .Setup(s =>
                        s.GetAllByOverallStage(
                            _draftReleaseVersion.Id,
                            new ReleasePublishingStatusOverallStage[]
                            {
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete,
                            }
                        )
                    )
                    .ReturnsAsync([new ReleasePublishingStatus()]);

                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(
                        releasePublishingStatusRepository: releasePublishingStatusRepository.Object,
                        authorizationHandlerService: authorizationHandlerService
                    );

                await AssertHandlerFailsWithoutCheckingRoles<MarkReleaseAsHigherLevelReviewRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _draftReleaseVersion
                );
            }

            [Fact]
            public async Task UnpublishedDraftReleaseVersion_SucceedsOnlyForValidPublicationRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<
                    MarkReleaseAsHigherLevelReviewRequirement,
                    ReleaseVersion
                >(
                    handlerSupplier: handlerSuppler,
                    entity: _draftReleaseVersion,
                    publicationId: _draftReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
                );
            }

            [Fact]
            public async Task UnpublishedApprovedReleaseVersion_SucceedsOnlyForValidPublicationRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<
                    MarkReleaseAsHigherLevelReviewRequirement,
                    ReleaseVersion
                >(
                    handlerSupplier: handlerSuppler,
                    entity: _approvedReleaseVersion,
                    publicationId: _approvedReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Approver]
                );
            }
        }

        private MarkReleaseAsHigherLevelReviewAuthorizationHandler SetupHandler(
            IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
            IAuthorizationHandlerService? authorizationHandlerService = null
        )
        {
            releasePublishingStatusRepository ??= CreateDefaultReleasePublishingStatusRepository();
            authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

            return new(releasePublishingStatusRepository, authorizationHandlerService);
        }
    }

    public class MarkReleaseAsApprovedAuthorizationHandlerTests : ReleaseStatusAuthorizationHandlersTests
    {
        public class ClaimsTests : MarkReleaseAsApprovedAuthorizationHandlerTests
        {
            [Fact]
            public async Task ReleaseVersionPublished_FailsForAllClaims()
            {
                await AssertHandlerFailsForAllClaims<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handler: SetupHandler(),
                    entity: _publishedReleaseVersion,
                    userId: _userId
                );
            }

            [Fact]
            public async Task ReleaseVersionPublishing_FailsForAllClaims()
            {
                var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(
                    MockBehavior.Strict
                );
                releasePublishingStatusRepository
                    .Setup(s =>
                        s.GetAllByOverallStage(
                            _draftReleaseVersion.Id,
                            new ReleasePublishingStatusOverallStage[]
                            {
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete,
                            }
                        )
                    )
                    .ReturnsAsync([new ReleasePublishingStatus()]);

                await AssertHandlerFailsForAllClaims<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handler: SetupHandler(releasePublishingStatusRepository.Object),
                    entity: _draftReleaseVersion,
                    userId: _userId
                );
            }

            [Fact]
            public async Task UnpublishedReleaseVersion_SucceedsOnlyForValidClaims()
            {
                await AssertHandlerSucceedsWithCorrectClaims<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handler: SetupHandler(),
                    entity: _draftReleaseVersion,
                    userId: _userId,
                    claimsExpectedToSucceed: [SecurityClaimTypes.ApproveAllReleases]
                );
            }
        }

        public class PublicationRolesTests : MarkReleaseAsApprovedAuthorizationHandlerTests
        {
            [Fact]
            public async Task ReleaseVersionPublished_FailsWithoutCheckingRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _publishedReleaseVersion
                );
            }

            [Fact]
            public async Task ReleaseVersionPublishing_FailsWithoutCheckingRoles()
            {
                var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(
                    MockBehavior.Strict
                );
                releasePublishingStatusRepository
                    .Setup(s =>
                        s.GetAllByOverallStage(
                            _draftReleaseVersion.Id,
                            new ReleasePublishingStatusOverallStage[]
                            {
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete,
                            }
                        )
                    )
                    .ReturnsAsync([new ReleasePublishingStatus()]);

                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(
                        releasePublishingStatusRepository: releasePublishingStatusRepository.Object,
                        authorizationHandlerService: authorizationHandlerService
                    );

                await AssertHandlerFailsWithoutCheckingRoles<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _draftReleaseVersion
                );
            }

            [Fact]
            public async Task UnpublishedDraftReleaseVersion_SucceedsOnlyForValidPublicationRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _draftReleaseVersion,
                    publicationId: _draftReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Approver]
                );
            }

            [Fact]
            public async Task UnpublishedApprovedReleaseVersion_SucceedsOnlyForValidPublicationRoles()
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<MarkReleaseAsApprovedRequirement, ReleaseVersion>(
                    handlerSupplier: handlerSuppler,
                    entity: _approvedReleaseVersion,
                    publicationId: _approvedReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Approver]
                );
            }
        }

        private MarkReleaseAsApprovedAuthorizationHandler SetupHandler(
            IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
            IAuthorizationHandlerService? authorizationHandlerService = null
        )
        {
            releasePublishingStatusRepository ??= CreateDefaultReleasePublishingStatusRepository();
            authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

            return new(releasePublishingStatusRepository, authorizationHandlerService);
        }
    }

    private IReleasePublishingStatusRepository CreateDefaultReleasePublishingStatusRepository()
    {
        var mock = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
        mock.Setup(s =>
                s.GetAllByOverallStage(
                    It.IsAny<Guid>(),
                    new ReleasePublishingStatusOverallStage[]
                    {
                        ReleasePublishingStatusOverallStage.Started,
                        ReleasePublishingStatusOverallStage.Complete,
                    }
                )
            )
            .ReturnsAsync([]);

        return mock.Object;
    }

    private IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s =>
                s.UserHasAnyPublicationRoleOnPublication(
                    _userId,
                    It.IsAny<Guid>(),
                    It.IsAny<HashSet<PublicationRole>>()
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
