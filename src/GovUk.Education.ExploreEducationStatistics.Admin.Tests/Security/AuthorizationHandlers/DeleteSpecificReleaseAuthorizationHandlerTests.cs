#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class DeleteSpecificReleaseAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly ReleaseVersion _nonAmendmentReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Draft)
        .WithVersion(0)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    private static readonly ReleaseVersion _approvedAmendmentReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
        .WithVersion(1)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    private static readonly ReleaseVersion _draftAmendmentReleaseVersion = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Draft)
        .WithVersion(1)
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    public class GlobalRolesTests : DeleteSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task NonAmendment_SucceedsOnlyForValidGlobalRoles()
        {
            // Assert that BAU users can delete the first version of a release
            await AssertHandlerSucceedsWithCorrectGlobalRoles<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _nonAmendmentReleaseVersion,
                userId: _userId,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]
            );
        }

        [Fact]
        public async Task ApprovedAmendment_FailsForAllGlobalRoles()
        {
            // Assert that no users can delete an amendment release version that is approved
            await AssertHandlerFailsForAllGlobalRoles<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _approvedAmendmentReleaseVersion,
                userId: _userId
            );
        }

        [Fact]
        public async Task DraftAmendment_FailsForAllGlobalRoles()
        {
            // Assert that no users can delete an amendment release version that is not yet approved
            await AssertHandlerFailsForAllGlobalRoles<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _draftAmendmentReleaseVersion,
                userId: _userId
            );
        }
    }

    public class ClaimsTests : DeleteSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task NonAmendment_FailsForAllClaims()
        {
            // Assert that no users can delete the first version of a release
            await AssertHandlerFailsForAllClaims<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _nonAmendmentReleaseVersion,
                userId: _userId
            );
        }

        [Fact]
        public async Task ApprovedAmendment_FailsForAllClaims()
        {
            // Assert that no users can delete an amendment release version that is approved
            await AssertHandlerFailsForAllClaims<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _approvedAmendmentReleaseVersion,
                userId: _userId
            );
        }

        [Fact]
        public async Task DraftAmendment_SucceedsOnlyForValidClaims()
        {
            // Assert that users with the "DeleteAllReleaseAmendments" claim can delete an amendment release version that is not yet approved
            await AssertHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handler: SetupHandler(),
                entity: _draftAmendmentReleaseVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.DeleteAllReleaseAmendments]
            );
        }
    }

    public class PublicationRolesTests : DeleteSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task NonAmendment_FailsWithoutCheckingRoles()
        {
            // Assert that no User Publication roles will allow deleting the first version of a release
            await AssertHandlerFailsWithoutCheckingRoles<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handlerSupplier: SetupHandler,
                entity: _nonAmendmentReleaseVersion
            );
        }

        [Fact]
        public async Task ApprovedAmendment_FailsWithoutCheckingRoles()
        {
            // Assert that no User Publication roles will allow deleting an amendment release version when it is Approved
            await AssertHandlerFailsWithoutCheckingRoles<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handlerSupplier: SetupHandler,
                entity: _approvedAmendmentReleaseVersion
            );
        }

        [Fact]
        public async Task DraftAmendment_SucceedsOnlyForValidPublicationRoles()
        {
            // Assert that users with the Publication Drafter or Approver role on an amendment release version can delete if it is not yet approved
            await AssertHandlerSucceedsForAnyValidPublicationRole<DeleteSpecificReleaseRequirement, ReleaseVersion>(
                handlerSupplier: SetupHandler,
                entity: _draftAmendmentReleaseVersion,
                publicationId: _draftAmendmentReleaseVersion.Release.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private static DeleteSpecificReleaseAuthorizationHandler SetupHandler(
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(authorizationHandlerService);
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
