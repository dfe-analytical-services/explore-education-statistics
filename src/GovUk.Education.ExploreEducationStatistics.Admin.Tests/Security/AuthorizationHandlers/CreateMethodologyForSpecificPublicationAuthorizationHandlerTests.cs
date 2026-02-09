#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateMethodologyForSpecificPublicationAuthorizationHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Publication Publication = new() { Id = Guid.NewGuid(), Methodologies = [] };

    private static readonly Publication PublicationArchived = new()
    {
        Id = Guid.NewGuid(),
        SupersededById = Guid.NewGuid(),
    };

    private static readonly Publication PublicationWithOwnedMethodology = new()
    {
        Id = Guid.NewGuid(),
        Methodologies = ListOf(new PublicationMethodology { Owner = true, MethodologyId = Guid.NewGuid() }),
    };

    private static readonly Publication PublicationWithAdoptedMethodology = new()
    {
        Id = Guid.NewGuid(),
        Methodologies = ListOf(new PublicationMethodology { Owner = false, MethodologyId = Guid.NewGuid() }),
    };

    private static readonly DataFixture DataFixture = new();

    public class ClaimTests
    {
        [Fact]
        public async Task UserWithCorrectClaimCanCreateMethodologyForAnyPublication()
        {
            await AssertUserWithCorrectClaimCanCreateMethodology(Publication);
        }

        [Fact]
        public async Task UserWithCorrectClaimCanCreateMethodologyForAnyPublication_AdoptedAnotherMethodologyButNotOwned()
        {
            await AssertUserWithCorrectClaimCanCreateMethodology(PublicationWithAdoptedMethodology);
        }

        [Fact]
        public async Task UserWithCorrectClaimCannotCreateMethodologyForArchivedPublication()
        {
            await AssertUserWithCorrectClaimCannotCreateMethodology(PublicationArchived);
        }

        [Fact]
        public async Task UserWithCorrectClaimCannotCreateMethodologyForAnyPublication_OwnsAnotherMethodology()
        {
            await AssertUserWithCorrectClaimCannotCreateMethodology(PublicationWithOwnedMethodology);
        }

        private static async Task AssertUserWithCorrectClaimCanCreateMethodology(Publication publication)
        {
            await ForEachSecurityClaimAsync(async claim =>
            {
                await using var context = InMemoryApplicationDbContext();
                context.Attach(publication);

                var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies(context);

                ClaimsPrincipal user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

                var authContext = CreateAuthContext(user, publication);

                var expectedToPassByClaimAlone = claim == CreateAnyMethodology;

                if (!expectedToPassByClaimAlone)
                {
                    userPublicationRoleRepository
                        .Setup(mock =>
                            mock.UserHasAnyRoleOnPublication(
                                UserId,
                                publication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                PublicationRole.Owner
                            )
                        )
                        .ReturnsAsync(false);
                }

                await handler.HandleAsync(authContext);
                VerifyAllMocks(userPublicationRoleRepository);

                // Verify that the presence of the "CreateAnyMethodology" Claim will pass the handler test, without
                // the need for a specific Publication to be provided
                Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
            });
        }

        private static async Task AssertUserWithCorrectClaimCannotCreateMethodology(Publication publication)
        {
            await ForEachSecurityClaimAsync(async claim =>
            {
                await using var context = InMemoryApplicationDbContext();
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();

                var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies(context);

                var user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

                var authContext = CreateAuthContext(user, publication);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(userPublicationRoleRepository);

                Assert.False(authContext.HasSucceeded);
            });
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task UserCanCreateMethodologyForPublicationWithPublicationOwnerRole()
        {
            await AssertPublicationOwnerCanCreateMethodology(Publication);
        }

        [Fact]
        public async Task UserCanCreateMethodologyForPublicationWithPublicationOwnerRole_AdoptedAnotherMethodologyButNotOwned()
        {
            await AssertPublicationOwnerCanCreateMethodology(PublicationWithAdoptedMethodology);
        }

        [Fact]
        public async Task UserCannotCreateMethodologyForArchivedPublication()
        {
            await AssertPublicationOwnerCannotCreateMethodology(PublicationArchived);
        }

        [Fact]
        public async Task UserCannotCreateMethodologyForPublicationWithoutPublicationOwnerRole()
        {
            await using var context = InMemoryApplicationDbContext();
            await context.Publications.AddAsync(Publication);
            await context.SaveChangesAsync();

            var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies(context);

            ClaimsPrincipal user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthContext(user, Publication);

            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        UserId,
                        Publication.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        PublicationRole.Owner
                    )
                )
                .ReturnsAsync(false);

            await handler.HandleAsync(authContext);
            VerifyAllMocks(userPublicationRoleRepository);

            // Verify that the user can't create a Methodology for this Publication because they don't have
            // Publication Owner role on it
            Assert.False(authContext.HasSucceeded);
        }

        [Fact]
        public async Task UserCannotCreateMethodologyForPublication_OwnsAnotherMethodology()
        {
            await AssertPublicationOwnerCannotCreateMethodology(PublicationWithOwnedMethodology);
        }

        private static async Task AssertPublicationOwnerCanCreateMethodology(Publication publication)
        {
            await using var context = InMemoryApplicationDbContext();
            context.Attach(publication);

            var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies(context);

            ClaimsPrincipal user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthContext(user, publication);

            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        UserId,
                        publication.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        PublicationRole.Owner
                    )
                )
                .ReturnsAsync(true);

            await handler.HandleAsync(authContext);
            VerifyAllMocks(userPublicationRoleRepository);

            // Verify that the user can create a Methodology for this Publication by virtue of having a Publication
            // Owner role on the Publication
            Assert.True(authContext.HasSucceeded);
        }

        private static async Task AssertPublicationOwnerCannotCreateMethodology(Publication publication)
        {
            await using var context = InMemoryApplicationDbContext();
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();

            var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies(context);

            var user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthContext(user, publication);

            await handler.HandleAsync(authContext);

            // There should be no interactions to check the users' publication roles to determine
            // whether or not they can create a methodology
            VerifyAllMocks(userPublicationRoleRepository);

            // Verify that the user cannot create a methodology for this publication
            Assert.False(authContext.HasSucceeded);
        }
    }

    private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user, Publication publication)
    {
        return CreateAuthorizationHandlerContext<CreateMethodologyForSpecificPublicationRequirement, Publication>(
            user,
            publication
        );
    }

    private static (
        CreateMethodologyForSpecificPublicationAuthorizationHandler,
        Mock<IUserPublicationRoleRepository>
    ) CreateHandlerAndDependencies(ContentDbContext contentDbContext)
    {
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

        var handler = new CreateMethodologyForSpecificPublicationAuthorizationHandler(
            contentDbContext,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(contentDbContext),
                Mock.Of<IUserReleaseRoleRepository>(Strict),
                userPublicationRoleRepository.Object,
                Mock.Of<IPreReleaseService>(Strict)
            )
        );

        return (handler, userPublicationRoleRepository);
    }
}
