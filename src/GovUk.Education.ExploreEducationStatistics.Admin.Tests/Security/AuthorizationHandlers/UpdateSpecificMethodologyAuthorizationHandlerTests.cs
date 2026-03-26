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
public abstract class UpdateSpecificMethodologyAuthorizationHandlerTests
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

    public class ClaimsTests : UpdateSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftMethodologyVersion_SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<UpdateSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _draftMethodologyVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllMethodologies]
            );
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_FailsForAllClaims()
        {
            await AssertHandlerFailsForAllClaims<UpdateSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _approvedMethodologyVersion,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : UpdateSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftMethodologyVersion_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                UpdateSpecificMethodologyRequirement,
                MethodologyVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _draftMethodologyVersion,
                publicationId: _owningPublication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task ApprovedMethodologyVersion_FailsWithoutCheckingRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerFailsWithoutCheckingRoles<UpdateSpecificMethodologyRequirement, MethodologyVersion>(
                handlerSupplier: handlerSuppler,
                entity: _approvedMethodologyVersion
            );
        }
    }

    private static UpdateSpecificMethodologyAuthorizationHandler SetupHandler(
        IMethodologyRepository? methodologyRepository = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        methodologyRepository ??= CreateDefaultMethodologyRepository();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(methodologyRepository, authorizationHandlerService);
    }

    private static IMethodologyRepository CreateDefaultMethodologyRepository()
    {
        var mock = new Mock<IMethodologyRepository>(MockBehavior.Strict);
        mock.Setup(s => s.GetOwningPublication(It.IsAny<Guid>())).ReturnsAsync(_owningPublication);

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

        return mock.Object;
    }
}
