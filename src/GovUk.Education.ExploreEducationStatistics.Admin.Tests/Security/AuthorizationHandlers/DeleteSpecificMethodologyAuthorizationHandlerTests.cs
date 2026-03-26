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
public abstract class DeleteSpecificMethodologyAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly MethodologyVersion _draftFirstVersion = _dataFixture
        .DefaultMethodologyVersion()
        .WithApprovalStatus(MethodologyApprovalStatus.Draft);

    private static readonly MethodologyVersion _approvedFirstVersion = _dataFixture
        .DefaultMethodologyVersion()
        .WithApprovalStatus(MethodologyApprovalStatus.Approved);

    private static readonly MethodologyVersion _draftAmendmentVersion = _dataFixture
        .DefaultMethodologyVersion()
        .WithApprovalStatus(MethodologyApprovalStatus.Draft)
        .WithVersion(1)
        .WithPreviousVersionId(Guid.NewGuid());

    private static readonly MethodologyVersion _approvedAmendmentVersion = _dataFixture
        .DefaultMethodologyVersion()
        .WithApprovalStatus(MethodologyApprovalStatus.Approved)
        .WithVersion(1)
        .WithPreviousVersionId(Guid.NewGuid());

    private static readonly Publication _owningPublication = new() { Id = Guid.NewGuid() };

    public class ClaimsTests : DeleteSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftFirstVersion_SucceedsOnlyForValidClaims()
        {
            // If the claims check fails, it will check the user's roles on the publication, but since we're testing claims here,
            // we want that to fail too, to ensure the claim is what's allowing access. So we let the IAuthorizationHandlerService default
            // to failing any role check, within the SetupHandler method.
            await AssertHandlerSucceedsWithCorrectClaims<DeleteSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _draftFirstVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.DeleteAllMethodologies]
            );
        }

        [Fact]
        public async Task ApprovedFirstVersion_FailsForAllClaims()
        {
            await AssertHandlerFailsForAllClaims<DeleteSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _approvedFirstVersion,
                userId: _userId
            );
        }

        [Fact]
        public async Task DraftAmendment_SucceedsOnlyForValidClaims()
        {
            // If the claims check fails, it will check the user's roles on the publication, but since we're testing claims here,
            // we want that to fail too, to ensure the claim is what's allowing access. So we let the IAuthorizationHandlerService default
            // to failing any role check, within the SetupHandler method.
            await AssertHandlerSucceedsWithCorrectClaims<DeleteSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _draftAmendmentVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.DeleteAllMethodologies]
            );
        }

        [Fact]
        public async Task ApprovedAmendment_FailsForAllClaims()
        {
            await AssertHandlerFailsForAllClaims<DeleteSpecificMethodologyRequirement, MethodologyVersion>(
                handler: SetupHandler(),
                entity: _approvedAmendmentVersion,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : DeleteSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftFirstVersion_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                DeleteSpecificMethodologyRequirement,
                MethodologyVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _draftFirstVersion,
                publicationId: _owningPublication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task ApprovedFirstVersion_FailsWithoutCheckingRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerFailsWithoutCheckingRoles<DeleteSpecificMethodologyRequirement, MethodologyVersion>(
                handlerSupplier: handlerSuppler,
                entity: _approvedFirstVersion
            );
        }

        [Fact]
        public async Task DraftAmendment_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                DeleteSpecificMethodologyRequirement,
                MethodologyVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _draftAmendmentVersion,
                publicationId: _owningPublication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task ApprovedAmendment_FailsWithoutCheckingRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                SetupHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerFailsWithoutCheckingRoles<DeleteSpecificMethodologyRequirement, MethodologyVersion>(
                handlerSupplier: handlerSuppler,
                entity: _approvedAmendmentVersion
            );
        }
    }

    private static DeleteSpecificMethodologyAuthorizationHandler SetupHandler(
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
