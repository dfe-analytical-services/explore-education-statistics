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
public abstract class ManagePublicationReleaseSeriesAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Publication _publication;

    protected ManagePublicationReleaseSeriesAuthorizationHandlerTests()
    {
        _publication = _dataFixture.DefaultPublication();
    }

    public class ClaimsTests : ManagePublicationReleaseSeriesAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<ManagePublicationReleaseSeriesRequirement, Publication>(
                handler: BuildHandler(),
                entity: _publication,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllPublications]
            );
        }
    }

    public class PublicationRolesTests : ManagePublicationReleaseSeriesAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<
                ManagePublicationReleaseSeriesRequirement,
                Publication
            >(
                handlerSupplier: BuildHandler,
                entity: _publication,
                publicationId: _publication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private ManagePublicationReleaseSeriesAuthorizationHandler BuildHandler(
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
