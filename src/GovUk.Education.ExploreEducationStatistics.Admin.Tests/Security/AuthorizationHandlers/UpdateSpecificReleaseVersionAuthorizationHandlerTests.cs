#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class UpdateSpecificReleaseVersionAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ReleaseVersion _draftReleaseVersion;
    private readonly ReleaseVersion _approvedReleaseVersion;

    protected UpdateSpecificReleaseVersionAuthorizationHandlerTests()
    {
        _draftReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        _approvedReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
    }

    public class ClaimsTests : UpdateSpecificReleaseVersionAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftReleaseVersion_SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseVersionRequirement, ReleaseVersion>(
                handler: BuildHandler(),
                entity: _draftReleaseVersion,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllReleases]
            );
        }

        [Fact]
        public async Task ApprovedReleaseVersion_FailsForAllClaims()
        {
            await AssertHandlerFailsForAllClaims<UpdateSpecificReleaseVersionRequirement, ReleaseVersion>(
                handler: BuildHandler(),
                entity: _approvedReleaseVersion,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : UpdateSpecificReleaseVersionAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftReleaseVersion_SucceedsOnlyForValidPublicationRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                BuildHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerSucceedsForAnyValidPublicationRole<
                UpdateSpecificReleaseVersionRequirement,
                ReleaseVersion
            >(
                handlerSupplier: handlerSuppler,
                entity: _draftReleaseVersion,
                publicationId: _draftReleaseVersion.Release.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task ApprovedReleaseVersion_FailsWithoutCheckingRoles()
        {
            var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                BuildHandler(authorizationHandlerService: authorizationHandlerService);

            await AssertHandlerFailsWithoutCheckingRoles<UpdateSpecificReleaseVersionRequirement, ReleaseVersion>(
                handlerSupplier: handlerSuppler,
                entity: _approvedReleaseVersion
            );
        }
    }

    private UpdateSpecificReleaseVersionAuthorizationHandler BuildHandler(
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
                    It.IsAny<Guid>(),
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
