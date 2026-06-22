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
public abstract class MarkMethodologyAsApprovedAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly MethodologyVersion _draftMethodologyVersion;
    private readonly MethodologyVersion _higherReviewMethodologyVersion;
    private readonly MethodologyVersion _approvedMethodologyVersion;
    private readonly Publication _owningPublication;

    private IReadOnlyList<MethodologyVersion> AllTypesOfMethodologyVersion =>
        [_draftMethodologyVersion, _higherReviewMethodologyVersion, _approvedMethodologyVersion];

    protected MarkMethodologyAsApprovedAuthorizationHandlerTests()
    {
        _draftMethodologyVersion = _dataFixture
            .DefaultMethodologyVersion()
            .WithApprovalStatus(MethodologyApprovalStatus.Draft);

        _higherReviewMethodologyVersion = _dataFixture
            .DefaultMethodologyVersion()
            .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview);

        _approvedMethodologyVersion = _dataFixture
            .DefaultMethodologyVersion()
            .WithApprovalStatus(MethodologyApprovalStatus.Approved)
            .WithPublished(DateTime.UtcNow);

        _owningPublication = _dataFixture.DefaultPublication();
    }

    public class ClaimsTests : MarkMethodologyAsApprovedAuthorizationHandlerTests
    {
        [Fact]
        public async Task NotLatestPublishedMethodologyVersion_SucceedsOnlyForValidClaims()
        {
            // Should succeed for valid claims regardless of the methodology version's approval status, as long as it's not the latest published version
            foreach (var methodologyVersion in AllTypesOfMethodologyVersion)
            {
                await AssertHandlerSucceedsWithCorrectClaims<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                    handler: BuildHandler(),
                    entity: methodologyVersion,
                    userId: _userId,
                    claimsExpectedToSucceed: [SecurityClaimTypes.ApproveAllMethodologies]
                );
            }
        }

        [Fact]
        public async Task LatestPublishedMethodologyVersion_FailsForAllClaims()
        {
            // Should fail for all claims regardless of the methodology version's approval status if it is the latest published version
            foreach (var methodologyVersion in AllTypesOfMethodologyVersion)
            {
                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
                methodologyVersionRepository
                    .Setup(s => s.IsLatestPublishedVersion(methodologyVersion))
                    .ReturnsAsync(true);

                await AssertHandlerFailsForAllClaims<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                    handler: BuildHandler(methodologyVersionRepository.Object),
                    entity: methodologyVersion,
                    userId: _userId
                );
            }
        }
    }

    public class PublicationRolesTests : MarkMethodologyAsApprovedAuthorizationHandlerTests
    {
        [Fact]
        public async Task NotLatestPublishedMethodologyVersion_SucceedsOnlyForValidPublicationRoles()
        {
            // Should succeed for valid publication roles regardless of the methodology version's approval status, as long as it's not the latest published version
            foreach (var methodologyVersion in AllTypesOfMethodologyVersion)
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(authorizationHandlerService: authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<
                    MarkMethodologyAsApprovedRequirement,
                    MethodologyVersion
                >(
                    handlerSupplier: handlerSuppler,
                    entity: methodologyVersion,
                    publicationId: _owningPublication.Id,
                    publicationRolesExpectedToSucceed: [PublicationRole.Approver]
                );
            }
        }

        [Fact]
        public async Task LatestPublishedMethodologyVersion_FailsWithoutCheckingRoles()
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            methodologyVersionRepository
                .Setup(s => s.IsLatestPublishedVersion(_approvedMethodologyVersion))
                .ReturnsAsync(true);

            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                BuildHandler(
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    authorizationHandlerService: authorizationHandlerService
                );

            await AssertHandlerFailsWithoutCheckingRoles<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                handlerSupplier: handlerSuppler,
                entity: _approvedMethodologyVersion
            );
        }
    }

    private MarkMethodologyAsApprovedAuthorizationHandler BuildHandler(
        IMethodologyVersionRepository? methodologyVersionRepository = null,
        IMethodologyRepository? methodologyRepository = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        methodologyVersionRepository ??= CreateDefaultMethodologyVersionRepository();
        methodologyRepository ??= CreateDefaultMethodologyRepository();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(methodologyVersionRepository, methodologyRepository, authorizationHandlerService);
    }

    private IMethodologyVersionRepository CreateDefaultMethodologyVersionRepository()
    {
        var mock = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        mock.Setup(s => s.IsLatestPublishedVersion(It.IsAny<MethodologyVersion>())).ReturnsAsync(false);

        return mock.Object;
    }

    private IMethodologyRepository CreateDefaultMethodologyRepository()
    {
        var mock = new Mock<IMethodologyRepository>(MockBehavior.Strict);
        mock.Setup(s => s.GetOwningPublication(It.IsAny<Guid>())).ReturnsAsync(_owningPublication);

        return mock.Object;
    }

    private IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s =>
                s.UserHasAnyPublicationRoleOnPublication(
                    _userId,
                    _owningPublication.Id,
                    CollectionUtils.SetOf(PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
