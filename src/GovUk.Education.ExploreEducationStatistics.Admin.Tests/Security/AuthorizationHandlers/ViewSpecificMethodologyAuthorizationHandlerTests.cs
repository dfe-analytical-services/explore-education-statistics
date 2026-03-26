#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class ViewSpecificMethodologyAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly MethodologyVersion _draftMethodologyVersion = _dataFixture
        .DefaultMethodologyVersion()
        .WithApprovalStatus(MethodologyApprovalStatus.Draft);

    private static readonly MethodologyVersion _approvedMethodologyVersion = _dataFixture
        .DefaultMethodologyVersion()
        .WithApprovalStatus(MethodologyApprovalStatus.Approved);

    private static readonly Publication _owningPublication = _dataFixture.DefaultPublication();

    private static readonly ReleaseVersion _liveReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
        .WithPublished(DateTimeOffset.UtcNow)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_owningPublication));

    private static readonly ReleaseVersion _draftReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Draft)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_owningPublication));

    private static readonly ReleaseVersion _approvedReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_owningPublication));

    public class ClaimsTests : ViewSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _draftMethodologyVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.AccessAllMethodologies]
            );
        }
    }

    public class PublicationRolesTests : ViewSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _draftMethodologyVersion,
                publicationId: _owningPublication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    public class InvalidClaimsAndRolesTests
    {
        [Fact]
        public async Task UnapprovedMethodologyVersion_Fails()
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _draftMethodologyVersion);

            var handler = SetupHandler();

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_PublicationHasNoReleases_Fails()
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _approvedMethodologyVersion);

            var handler = SetupHandler();

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_PublicationsLatestReleaseVersionIsLive_Fails()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.GetLatestReleaseVersion(_owningPublication.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_liveReleaseVersion);

            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _approvedMethodologyVersion);

            var handler = SetupHandler(releaseVersionRepository: releaseVersionRepository.Object);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_PublicationsLatestReleaseVersionIsUnapproved_Fails()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.GetLatestReleaseVersion(_owningPublication.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_draftReleaseVersion);

            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _approvedMethodologyVersion);

            var handler = SetupHandler(releaseVersionRepository: releaseVersionRepository.Object);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }

        [Theory]
        [InlineData(PreReleaseAccess.NoneSet)]
        [InlineData(PreReleaseAccess.Before)]
        [InlineData(PreReleaseAccess.After)]
        public async Task ApprovedMethodologyVersion_AnyPublicationsLatestReleaseVersionIsApproved_UserHasPrereleaseRole_IsNotWithinPrereleaseWindow_Fails(
            PreReleaseAccess preReleaseAccessWindowStatus
        )
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.GetLatestReleaseVersion(_owningPublication.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_approvedReleaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _owningPublication.Id,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(false);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _approvedReleaseVersion.Id))
                .ReturnsAsync(true);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            preReleaseService
                .Setup(s => s.GetPreReleaseWindowStatus(_approvedReleaseVersion, It.IsAny<DateTimeOffset>()))
                .Returns(new PreReleaseWindowStatus() { Access = preReleaseAccessWindowStatus });

            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _approvedMethodologyVersion);

            var handler = SetupHandler(
                releaseVersionRepository: releaseVersionRepository.Object,
                authorizationHandlerService: authorizationHandlerService.Object,
                preReleaseService: preReleaseService.Object
            );

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_AnyPublicationsLatestReleaseVersionIsApproved_UserDoesNotHavePrereleaseRole_IsWithinPrereleaseWindow_Fails()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.GetLatestReleaseVersion(_owningPublication.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_approvedReleaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _owningPublication.Id,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(false);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _approvedReleaseVersion.Id))
                .ReturnsAsync(false);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            preReleaseService
                .Setup(s => s.GetPreReleaseWindowStatus(_approvedReleaseVersion, It.IsAny<DateTimeOffset>()))
                .Returns(new PreReleaseWindowStatus() { Access = PreReleaseAccess.Within });

            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _approvedMethodologyVersion);

            var handler = SetupHandler(
                releaseVersionRepository: releaseVersionRepository.Object,
                authorizationHandlerService: authorizationHandlerService.Object,
                preReleaseService: preReleaseService.Object
            );

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_AnyPublicationsLatestReleaseVersionIsApproved_UserHasPrereleaseRole_IsWithinPrereleaseWindow_Passes()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.GetLatestReleaseVersion(_owningPublication.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_approvedReleaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _owningPublication.Id,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(false);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _approvedReleaseVersion.Id))
                .ReturnsAsync(true);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            preReleaseService
                .Setup(s => s.GetPreReleaseWindowStatus(_approvedReleaseVersion, It.IsAny<DateTimeOffset>()))
                .Returns(new PreReleaseWindowStatus() { Access = PreReleaseAccess.Within });

            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, _approvedMethodologyVersion);

            var handler = SetupHandler(
                releaseVersionRepository: releaseVersionRepository.Object,
                authorizationHandlerService: authorizationHandlerService.Object,
                preReleaseService: preReleaseService.Object
            );

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }
    }

    private static ViewSpecificMethodologyAuthorizationHandler SetupHandler(
        IMethodologyRepository? methodologyRepository = null,
        IPreReleaseService? preReleaseService = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        methodologyRepository ??= CreateDefaultMethodologyRepository();
        preReleaseService ??= CreateDefaultPreReleaseService();
        releaseVersionRepository ??= CreateDefaultReleaseVersionRepository();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(methodologyRepository, preReleaseService, releaseVersionRepository, authorizationHandlerService);
    }

    private static IMethodologyRepository CreateDefaultMethodologyRepository()
    {
        var mock = new Mock<IMethodologyRepository>(MockBehavior.Strict);
        mock.Setup(s => s.GetOwningPublication(It.IsAny<Guid>())).ReturnsAsync(_owningPublication);
        mock.Setup(s => s.GetAllPublicationIds(It.IsAny<Guid>())).ReturnsAsync([_owningPublication.Id]);

        return mock.Object;
    }

    private static IPreReleaseService CreateDefaultPreReleaseService()
    {
        var mock = new Mock<IPreReleaseService>(MockBehavior.Strict);
        mock.Setup(s => s.GetPreReleaseWindowStatus(It.IsAny<ReleaseVersion>(), It.IsAny<DateTimeOffset>()))
            .Returns(new PreReleaseWindowStatus() { Access = PreReleaseAccess.NoneSet });

        return mock.Object;
    }

    private static IReleaseVersionRepository CreateDefaultReleaseVersionRepository()
    {
        var mock = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
        mock.Setup(s => s.GetLatestReleaseVersion(_owningPublication.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReleaseVersion?)null);

        return mock.Object;
    }

    private static IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s =>
                s.UserHasAnyPublicationRoleOnPublication(
                    _userId,
                    _owningPublication.Id,
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);
        mock.Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, It.IsAny<Guid>())).ReturnsAsync(false);

        return mock.Object;
    }
}
