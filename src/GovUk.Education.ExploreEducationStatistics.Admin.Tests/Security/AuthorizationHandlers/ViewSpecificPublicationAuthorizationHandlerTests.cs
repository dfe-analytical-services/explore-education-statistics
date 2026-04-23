#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class ViewSpecificPublicationAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Publication _publication;

    protected ViewSpecificPublicationAuthorizationHandlerTests()
    {
        _publication = _dataFixture.DefaultPublication();
    }

    public class ClaimsTests : ViewSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificPublicationRequirement, Publication>(
                handler: BuildHandler(),
                entity: _publication,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.AccessAllPublications]
            );
        }
    }

    public class RolesTests : ViewSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsIfUserHasAnyRoleOnPublication<ViewSpecificPublicationRequirement, Publication>(
                handlerSupplier: BuildHandler,
                entity: _publication,
                publicationId: _publication.Id
            );
        }
    }

    private ViewSpecificPublicationAuthorizationHandler BuildHandler(
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(authorizationHandlerService);
    }

    private IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s => s.UserHasAnyRoleOnPublication(_userId, _publication.Id)).ReturnsAsync(false);

        return mock.Object;
    }
}
