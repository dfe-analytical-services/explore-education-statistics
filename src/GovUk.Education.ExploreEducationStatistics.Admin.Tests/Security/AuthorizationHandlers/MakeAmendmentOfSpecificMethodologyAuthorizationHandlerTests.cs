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
public abstract class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly MethodologyVersion _methodologyVersion;
    private readonly Publication _owningPublication;

    protected MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests()
    {
        _methodologyVersion = _dataFixture
            .DefaultMethodologyVersion()
            .WithPublishingStrategy(MethodologyPublishingStrategy.Immediately);

        _owningPublication = _dataFixture.DefaultPublication();
    }

    public class ClaimsTests : MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task LatestPublishedMethodologyVersion_SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<
                MakeAmendmentOfSpecificMethodologyRequirement,
                MethodologyVersion
            >(
                handler: SetupHandler(),
                entity: _methodologyVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.MakeAmendmentsOfAllMethodologies]
            );
        }

        [Fact]
        public async Task NotLatestPublishedMethodologyVersion_FailsForAllClaims()
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            methodologyVersionRepository
                .Setup(mock => mock.IsLatestPublishedVersion(_methodologyVersion))
                .ReturnsAsync(false);

            await AssertHandlerFailsForAllClaims<MakeAmendmentOfSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(methodologyVersionRepository.Object),
                entity: _methodologyVersion,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task LatestPublishedMethodologyVersion_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                MakeAmendmentOfSpecificMethodologyRequirement,
                MethodologyVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _methodologyVersion,
                publicationId: _owningPublication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task NotLatestPublishedMethodologyVersion_FailsWithoutCheckingRoles()
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            methodologyVersionRepository
                .Setup(mock => mock.IsLatestPublishedVersion(_methodologyVersion))
                .ReturnsAsync(false);

            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    authorizationHandlerService: authorizationHandlerService
                );

            await AssertHandlerFailsWithoutCheckingRoles<
                MakeAmendmentOfSpecificMethodologyRequirement,
                MethodologyVersion
            >(handlerSupplier: handlerSuppler, entity: _methodologyVersion);
        }
    }

    private MakeAmendmentOfSpecificMethodologyAuthorizationHandler SetupHandler(
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
        mock.Setup(s => s.IsLatestPublishedVersion(_methodologyVersion)).ReturnsAsync(true);

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
                    It.IsAny<Guid>(),
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
