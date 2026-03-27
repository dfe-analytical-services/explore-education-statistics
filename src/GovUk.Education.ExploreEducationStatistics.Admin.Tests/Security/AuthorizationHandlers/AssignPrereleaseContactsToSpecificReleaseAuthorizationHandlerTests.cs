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

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ReleaseVersion _draftReleaseVersion;
    private readonly ReleaseVersion _approvedReleaseVersion;

    protected AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerTests()
    {
        _draftReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Draft);

        _approvedReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved);
    }

    public class ClaimsTests : AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task ReleaseVersionNotApproved_SucceedsOnlyForValidClaims()
        {
            // If the claims check fails, it will check the user's roles on the publication, but since we're testing claims here,
            // we want that to fail too, to ensure the claim is what's allowing access. So we let the IAuthorizationHandlerService default
            // to failing any role check, within the SetupHandler method.
            await AssertHandlerSucceedsWithCorrectClaims<
                AssignPrereleaseContactsToSpecificReleaseRequirement,
                ReleaseVersion
            >(
                handler: SetupHandler(),
                entity: _draftReleaseVersion,
                claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllReleases],
                userId: _userId
            );
        }

        [Fact]
        public async Task ReleaseVersionApproved_SucceedsOnlyForValidClaims()
        {
            // If the claims check fails, it will check the user's roles on the publication, but since we're testing claims here,
            // we want that to fail too, to ensure the claim is what's allowing access. So we let the IAuthorizationHandlerService default
            // to failing any role check, within the SetupHandler method.
            await AssertHandlerSucceedsWithCorrectClaims<
                AssignPrereleaseContactsToSpecificReleaseRequirement,
                ReleaseVersion
            >(
                handler: SetupHandler(),
                entity: _approvedReleaseVersion,
                claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllReleases],
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task ReleaseVersionNotApproved_SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<
                AssignPrereleaseContactsToSpecificReleaseRequirement,
                ReleaseVersion
            >(
                handlerSupplier: SetupHandler,
                entity: _draftReleaseVersion,
                publicationId: _draftReleaseVersion.Release.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task ReleaseVersionApproved_SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<
                AssignPrereleaseContactsToSpecificReleaseRequirement,
                ReleaseVersion
            >(
                handlerSupplier: SetupHandler,
                entity: _approvedReleaseVersion,
                publicationId: _approvedReleaseVersion.Release.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler SetupHandler(
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
