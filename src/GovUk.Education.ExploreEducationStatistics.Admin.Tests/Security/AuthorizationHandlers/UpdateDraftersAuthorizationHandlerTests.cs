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

public abstract class UpdateDraftersAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly Publication _publication = _dataFixture.DefaultPublication();

    public class ClaimsTests : UpdateDraftersAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<UpdateDraftersRequirement, Publication>(
                handler: SetupHandler(),
                entity: _publication,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.ManageAnyUser]
            );
        }
    }

    public class PublicationRolesTests : UpdateDraftersAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<UpdateDraftersRequirement, Publication>(
                handlerSupplier: SetupHandler,
                entity: _publication,
                publicationId: _publication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private static UpdateDraftersAuthorizationHandler SetupHandler(
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
                    _publication.Id,
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
