#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class ViewSpecificPublicationReleaseTeamAccessAuthorizationHandlersTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Publication Publication = new() { Id = Guid.NewGuid() };

    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task ViewSpecificPublicationReleaseTeamAccess_SucceedsWithAccessAllPublicationsClaim()
    {
        // Assert that any users with the "AccessAllPublications" claim can view Team Access of Releases on this
        // Publication
        await ForEachSecurityClaimAsync(async claim =>
        {
            var expectedToPassByClaimAlone = claim == AccessAllPublications;

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            if (!expectedToPassByClaimAlone)
            {
                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                    .ReturnsAsync(new List<PublicationRole>());
            }

            var handler = CreateHandler(userPublicationRoleRepository.Object);

            var user = _fixture
                .AuthenticatedUser(userId: UserId)
                .WithClaim(claim.ToString());

            var authContext =
                CreateAuthorizationHandlerContext<ViewSpecificPublicationReleaseTeamAccessRequirement, Publication>
                    (user, Publication);

            await handler.HandleAsync(authContext);

            VerifyAllMocks(userPublicationRoleRepository);

            Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
        });
    }

    [Fact]
    public async Task ViewSpecificPublicationReleaseTeamAccess_SucceedsWithPublicationRoles()
    {
        await ForEachPublicationRoleAsync(async role =>
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            userPublicationRoleRepository
                .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                .ReturnsAsync(ListOf(role));

            var handler = CreateHandler(userPublicationRoleRepository.Object);

            var user = _fixture.AuthenticatedUser(userId: UserId);

            var authContext =
                CreateAuthorizationHandlerContext<ViewSpecificPublicationReleaseTeamAccessRequirement, Publication>
                    (user, Publication);

            await handler.HandleAsync(authContext);

            VerifyAllMocks(userPublicationRoleRepository);

            Assert.Equal(
                ListOf(PublicationRole.Owner, PublicationRole.Allower).Contains(role),
                authContext.HasSucceeded);
        });
    }

    private static ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler CreateHandler(
        IUserPublicationRoleRepository? userPublicationRoleRepository = null)
    {
        return new ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler(
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                Mock.Of<IUserReleaseRoleRepository>(Strict),
                userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
                Mock.Of<IPreReleaseService>(Strict)));
    }
}
