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
    private readonly MethodologyVersion _methodologyVersion;
    private readonly Publication _owningPublication;

    protected MarkMethodologyAsApprovedAuthorizationHandlerTests()
    {
        _methodologyVersion = _dataFixture.DefaultMethodologyVersion();

        _owningPublication = _dataFixture.DefaultPublication();
    }

    public class ClaimsTests : MarkMethodologyAsApprovedAuthorizationHandlerTests
    {
        [Fact]
        public async Task NotLatestPublishedMethodologyVersion_SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _methodologyVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.ApproveAllMethodologies]
            );
        }

        [Fact]
        public async Task LatestPublishedMethodologyVersion_FailsForAllClaims()
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            methodologyVersionRepository.Setup(s => s.IsLatestPublishedVersion(_methodologyVersion)).ReturnsAsync(true);

            await AssertHandlerFailsForAllClaims<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                handler: SetupHandler(methodologyVersionRepository.Object),
                entity: _methodologyVersion,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : MarkMethodologyAsApprovedAuthorizationHandlerTests
    {
        [Fact]
        public async Task NotLatestPublishedMethodologyVersion_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                MarkMethodologyAsApprovedRequirement,
                MethodologyVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _methodologyVersion,
                publicationId: _owningPublication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task LatestPublishedMethodologyVersion_FailsWithoutCheckingRoles()
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            methodologyVersionRepository.Setup(s => s.IsLatestPublishedVersion(_methodologyVersion)).ReturnsAsync(true);

            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    authorizationHandlerService: authorizationHandlerService
                );

            await AssertHandlerFailsWithoutCheckingRoles<MarkMethodologyAsApprovedRequirement, MethodologyVersion>(
                handlerSupplier: handlerSuppler,
                entity: _methodologyVersion
            );
        }
    }

    private MarkMethodologyAsApprovedAuthorizationHandler SetupHandler(
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
        mock.Setup(s => s.IsLatestPublishedVersion(_methodologyVersion)).ReturnsAsync(false);

        return mock.Object;
    }

    private IMethodologyRepository CreateDefaultMethodologyRepository()
    {
        var mock = new Mock<IMethodologyRepository>(MockBehavior.Strict);
        mock.Setup(s => s.GetOwningPublication(_methodologyVersion.MethodologyId)).ReturnsAsync(_owningPublication);

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
