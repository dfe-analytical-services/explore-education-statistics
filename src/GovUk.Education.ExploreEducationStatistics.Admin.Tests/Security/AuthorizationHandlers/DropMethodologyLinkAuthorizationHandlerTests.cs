#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class DropMethodologyLinkAuthorizationHandlerTests
{
    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly PublicationMethodology _owningLink = new()
    {
        Owner = true,
        PublicationId = Guid.NewGuid(),
        MethodologyId = Guid.NewGuid(),
    };

    private static readonly PublicationMethodology _nonOwningLink = new()
    {
        Owner = false,
        PublicationId = Guid.NewGuid(),
        MethodologyId = Guid.NewGuid(),
    };

    public class ClaimsTests : DropMethodologyLinkAuthorizationHandlerTests
    {
        [Fact]
        public async Task NonOwningLink_SucceedsOnlyForValidClaims()
        {
            await AssertHandlerSucceedsWithCorrectClaims<DropMethodologyLinkRequirement, PublicationMethodology>(
                handler: SetupHandler(),
                entity: _nonOwningLink,
                userId: _userId,
                claimsExpectedToSucceed: [SecurityClaimTypes.AdoptAnyMethodology]
            );
        }

        [Fact]
        public async Task OwningLink_FailsForAllClaims()
        {
            // No claims should allow dropping the link from a methodology to the owning publication
            await AssertHandlerFailsForAllClaims<DropMethodologyLinkRequirement, PublicationMethodology>(
                handler: SetupHandler(),
                entity: _owningLink,
                userId: _userId
            );
        }
    }

    public class PublicationRolesTests : DropMethodologyLinkAuthorizationHandlerTests
    {
        [Fact]
        public async Task NonOwningLink_SucceedsOnlyForValidPublicationRoles()
        {
            await AssertHandlerSucceedsForAnyValidPublicationRole<
                DropMethodologyLinkRequirement,
                PublicationMethodology
            >(
                handlerSupplier: SetupHandler,
                entity: _nonOwningLink,
                publicationId: _nonOwningLink.PublicationId,
                publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
            );
        }

        [Fact]
        public async Task OwningLink_FailsWithoutCheckingRoles()
        {
            // No publication roles should allow dropping the link from a methodology to the owning publication
            await AssertHandlerFailsWithoutCheckingRoles<DropMethodologyLinkRequirement, PublicationMethodology>(
                handlerSupplier: SetupHandler,
                entity: _owningLink
            );
        }
    }

    private static DropMethodologyLinkAuthorizationHandler SetupHandler(
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
