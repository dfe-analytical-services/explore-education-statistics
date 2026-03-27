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
public abstract class CreateReleaseForSpecificPublicationAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Publication _publication;
    private readonly Publication _archivedPublication;

    protected CreateReleaseForSpecificPublicationAuthorizationHandlerTests()
    {
        _publication = _dataFixture.DefaultPublication();

        _archivedPublication = _dataFixture.DefaultPublication().WithSupersededBy(_dataFixture.DefaultPublication());
    }

    public class ClaimsTests : CreateReleaseForSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            // If the claims check fails, it will check the user's roles on the publication, but since we're testing claims here,
            // we want that to fail too, to ensure the claim is what's allowing access. So we let the IAuthorizationHandlerService default
            // to failing any role check, within the SetupHandler method.
            await AssertHandlerSucceedsWithCorrectClaims<CreateReleaseForSpecificPublicationRequirement, Publication>(
                handler: SetupHandler(),
                entity: _publication,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.CreateAnyRelease]
            );
        }

        [Fact]
        public async Task ArchivedPublication_FailsForAllClaims()
        {
            await AssertHandlerFailsForAllClaims<CreateReleaseForSpecificPublicationRequirement, Publication>(
                handler: SetupHandler(),
                entity: _archivedPublication,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : CreateReleaseForSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<
                CreateReleaseForSpecificPublicationRequirement,
                Publication
            >(
                handlerSupplier: SetupHandler,
                entity: _publication,
                publicationId: _publication.Id,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task ArchivedPublication_FailsWithoutCheckingRoles()
        {
            await AssertHandlerFailsWithoutCheckingRoles<CreateReleaseForSpecificPublicationRequirement, Publication>(
                handlerSupplier: SetupHandler,
                entity: _archivedPublication
            );
        }
    }

    private CreateReleaseForSpecificPublicationAuthorizationHandler SetupHandler(
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
