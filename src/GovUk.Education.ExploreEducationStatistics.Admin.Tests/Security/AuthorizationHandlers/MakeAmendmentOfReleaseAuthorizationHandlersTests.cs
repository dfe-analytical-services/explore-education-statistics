#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly ReleaseVersion _draftReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Draft)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    private static readonly ReleaseVersion _publishedReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
        .WithPublished(DateTimeOffset.UtcNow)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    public class ClaimsTests : MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task UnpublishedVersion_FailsForAllClaims()
        {
            await AssertHandlerFailsForAllClaims<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _draftReleaseVersion,
                userId: _userId
            );
        }

        [Fact]
        public async Task PublishedAndNotLatestVersion_FailsForAllClaims()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestReleaseVersion(_publishedReleaseVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await AssertHandlerFailsForAllClaims<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(releaseVersionRepository.Object),
                entity: _publishedReleaseVersion,
                userId: _userId
            );
        }

        [Fact]
        public async Task PublishedAndLatestVersion_SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>(
                // IReleaseVersionRespository.IsLatestReleaseVersion has a default setup of returning 'true'
                handler: SetupHandler(),
                entity: _publishedReleaseVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.MakeAmendmentsOfAllReleases]
            );
        }
    }

    public class PublicationRolesTests : MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task UnpublishedVersion_FailsWithoutCheckingRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerFailsWithoutCheckingRoles<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>(
                handlerSupplier: handlerSuppler,
                entity: _draftReleaseVersion
            );
        }

        [Fact]
        public async Task PublishedAndNotLatestVersion_FailsWithoutCheckingRoles()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestReleaseVersion(_publishedReleaseVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(releaseVersionRepository.Object, authorizationHandlerService);

            await AssertHandlerFailsWithoutCheckingRoles<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>(
                handlerSupplier: handlerSuppler,
                entity: _publishedReleaseVersion
            );
        }

        [Fact]
        public async Task PublishedAndLatestVersion_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                MakeAmendmentOfSpecificReleaseRequirement,
                ReleaseVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _publishedReleaseVersion,
                publicationId: _publishedReleaseVersion.Release.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private static MakeAmendmentOfSpecificReleaseAuthorizationHandler SetupHandler(
        IReleaseVersionRepository? releaseVersionRepository = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        releaseVersionRepository ??= CreateDefaultReleaseVersionRepository();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(releaseVersionRepository, authorizationHandlerService);
    }

    private static IReleaseVersionRepository CreateDefaultReleaseVersionRepository()
    {
        var mock = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
        mock.Setup(s => s.IsLatestReleaseVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        return mock.Object;
    }

    private static IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s =>
                s.UserHasAnyPublicationRoleOnPublication(
                    _userId,
                    It.IsAny<Guid>(),
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
