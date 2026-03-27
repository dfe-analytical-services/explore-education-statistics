#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ReleaseVersion _releaseVersion;

    protected ViewSpecificPreReleaseSummaryAuthorizationHandlersTests()
    {
        _releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
    }

    public class ClaimsTests : ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificPreReleaseSummaryRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _releaseVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.AccessAllReleases]
            );
        }
    }

    public class PublicationRolesTests : ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
    {
        [Fact]
        public async Task SucceedsForValidPublicationRoles()
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificPreReleaseSummaryRequirement,
                ReleaseVersion
            >(user, _releaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _releaseVersion.Release.PublicationId,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(true);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _releaseVersion.Id))
                .ReturnsAsync(false);

            var handler = SetupHandler(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }

        [Fact]
        public async Task FailsForInvalidPublicationRoles()
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificPreReleaseSummaryRequirement,
                ReleaseVersion
            >(user, _releaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _releaseVersion.Release.PublicationId,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(false);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _releaseVersion.Id))
                .ReturnsAsync(false);

            var handler = SetupHandler(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }

    public class PrereleaseRolesTests : ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
    {
        [Fact]
        public async Task HasPreReleaseRole_Succeeds()
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificPreReleaseSummaryRequirement,
                ReleaseVersion
            >(user, _releaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _releaseVersion.Release.PublicationId,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(false);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _releaseVersion.Id))
                .ReturnsAsync(true);

            var handler = SetupHandler(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }

        [Fact]
        public async Task DoesNotHavePreReleaseRole_Fails()
        {
            ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

            var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                ViewSpecificPreReleaseSummaryRequirement,
                ReleaseVersion
            >(user, _releaseVersion);

            var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
            authorizationHandlerService
                .Setup(s =>
                    s.UserHasAnyPublicationRoleOnPublication(
                        _userId,
                        _releaseVersion.Release.PublicationId,
                        CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                    )
                )
                .ReturnsAsync(false);
            authorizationHandlerService
                .Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _releaseVersion.Id))
                .ReturnsAsync(false);

            var handler = SetupHandler(authorizationHandlerService.Object);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }

    private ViewSpecificPreReleaseSummaryAuthorizationHandler SetupHandler(
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(authorizationHandlerService);
    }

    private IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s =>
                s.UserHasAnyPublicationRoleOnPublication(
                    _userId,
                    _releaseVersion.Release.PublicationId,
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);
        mock.Setup(s => s.UserHasPrereleaseRoleOnReleaseVersion(_userId, _releaseVersion.Id)).ReturnsAsync(false);

        return mock.Object;
    }
}
