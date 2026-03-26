#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class CreateMethodologyForSpecificPublicationAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly Publication _publication = _dataFixture.DefaultPublication();

    private static readonly Publication _publicationArchived = _dataFixture
        .DefaultPublication()
        .WithSupersededBy(_dataFixture.DefaultPublication());

    private static readonly Publication _publicationWithOwnedMethodology = _dataFixture
        .DefaultPublication()
        .WithMethodologies(
            CollectionUtils.ListOf(new PublicationMethodology { Owner = true, MethodologyId = Guid.NewGuid() })
        );

    private static readonly Publication _publicationWithAdoptedMethodology = _dataFixture
        .DefaultPublication()
        .WithMethodologies(
            CollectionUtils.ListOf(new PublicationMethodology { Owner = false, MethodologyId = Guid.NewGuid() })
        );

    public class ClaimsTests : CreateMethodologyForSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task EmptyPublication_SucceedsOnlyForValidClaims()
        {
            await AssertSucceedsOnlyForValidClaims(_publication);
        }

        [Fact]
        public async Task PublicationAdoptedAnotherMethodologyButNotOwned_SucceedsOnlyForValidClaims()
        {
            await AssertSucceedsOnlyForValidClaims(_publicationWithAdoptedMethodology);
        }

        [Fact]
        public async Task ArchivedPublication_FailsForAllClaims()
        {
            await AssertFailsForAllClaims(_publicationArchived);
        }

        [Fact]
        public async Task PublicationOwnsAnotherMethodology_FailsForAllClaims()
        {
            await AssertFailsForAllClaims(_publicationWithOwnedMethodology);
        }

        private static async Task AssertSucceedsOnlyForValidClaims(Publication publication)
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerSucceedsWithCorrectClaims<
                    CreateMethodologyForSpecificPublicationRequirement,
                    Publication
                >(
                    handler: SetupHandler(context),
                    entity: publication,
                    claimsExpectedToSucceed: [SecurityClaimTypes.CreateAnyMethodology],
                    userId: _userId
                );
            }
        }

        private static async Task AssertFailsForAllClaims(Publication publication)
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerFailsForAllClaims<CreateMethodologyForSpecificPublicationRequirement, Publication>(
                    handler: SetupHandler(context),
                    entity: publication,
                    userId: _userId
                );
            }
        }
    }

    public class PublicationRolesTests : CreateMethodologyForSpecificPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task EmptyPublication_SucceedsOnlyForValidPublicationRoles()
        {
            await AssertSucceedsOnlyForValidPublicationRoles(_publication);
        }

        [Fact]
        public async Task PublicationAdoptedAnotherMethodologyButNotOwned_SucceedsOnlyForValidPublicationRoles()
        {
            await AssertSucceedsOnlyForValidPublicationRoles(_publicationWithAdoptedMethodology);
        }

        [Fact]
        public async Task ArchivedPublication_FailsWithoutCheckingRoles()
        {
            await AssertFailsWithoutCheckingRoles(_publicationArchived);
        }

        [Fact]
        public async Task PublicationOwnsAnotherMethodology_FailsWithoutCheckingRoles()
        {
            await AssertFailsWithoutCheckingRoles(_publicationWithOwnedMethodology);
        }

        private static async Task AssertSucceedsOnlyForValidPublicationRoles(Publication publication)
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(context, authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<
                    CreateMethodologyForSpecificPublicationRequirement,
                    Publication
                >(
                    handlerSupplier: handlerSuppler,
                    entity: publication,
                    publicationId: publication.Id,
                    publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
                );
            }
        }

        private static async Task AssertFailsWithoutCheckingRoles(Publication publication)
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(context, authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<
                    CreateMethodologyForSpecificPublicationRequirement,
                    Publication
                >(handlerSupplier: handlerSuppler, entity: publication);
            }
        }
    }

    private static CreateMethodologyForSpecificPublicationAuthorizationHandler SetupHandler(
        ContentDbContext? contentDbContext = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(contentDbContext, authorizationHandlerService);
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
