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

public abstract class UpdateSpecificReleaseAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Release _release;

    protected UpdateSpecificReleaseAuthorizationHandlerTests()
    {
        _release = _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication());
    }

    public class ClaimsTests : UpdateSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement, Release>(
                handler: SetupHandler(),
                entity: _release,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllReleases]
            );
        }
    }

    public class PublicationRolesTests : UpdateSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<UpdateSpecificReleaseRequirement, Release>(
                handlerSupplier: SetupHandler,
                entity: _release,
                publicationId: _release.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private UpdateSpecificReleaseAuthorizationHandler SetupHandler(
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
                    _release.PublicationId,
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
