#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class ManagePublicApiDataSetPreviewTokensAuthorizationHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _publicationId = Guid.NewGuid();

    public class GlobalRolesTests : ManagePublicApiDataSetPreviewTokensAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidGlobalRoles()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ManagePublicApiDataSetPreviewTokensRequirement, Guid>(
                handler: BuildHandler(),
                entity: _publicationId,
                userId: _userId,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]
            );
        }
    }

    public class PublicationRolesTests : ManagePublicApiDataSetPreviewTokensAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<ManagePublicApiDataSetPreviewTokensRequirement, Guid>(
                handlerSupplier: BuildHandler,
                entity: _publicationId,
                publicationId: _publicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }
    }

    private ManagePublicApiDataSetPreviewTokensAuthorizationHandler BuildHandler(
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
