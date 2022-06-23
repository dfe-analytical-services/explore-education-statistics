#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DropMethodologyLinkAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly PublicationMethodology OwningLink = new()
        {
            Owner = true,
            PublicationId = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid()
        };

        private static readonly PublicationMethodology NonOwningLink = new()
        {
            Owner = false,
            PublicationId = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid()
        };

        public class ClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowDroppingOwningLinks()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                    var handler = SetupHandler(userPublicationRoleRepository.Object);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<DropMethodologyLinkRequirement, PublicationMethodology>(
                            user,
                            OwningLink);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(userPublicationRoleRepository);

                    // No claims should allow dropping the link from a methodology to the owning publication
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanDropLinks()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                    var handler = SetupHandler(userPublicationRoleRepository.Object);

                    // Only the AdoptAnyMethodology claim should allow dropping methodology links to publications
                    var expectedToPassByClaimAlone = claim == AdoptAnyMethodology;

                    if (!expectedToPassByClaimAlone)
                    {
                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, NonOwningLink.PublicationId))
                            .ReturnsAsync(new List<PublicationRole>());
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<DropMethodologyLinkRequirement, PublicationMethodology>(
                            user,
                            NonOwningLink);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(userPublicationRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task NoPublicationRolesAllowDroppingOwningLinks()
            {
                var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                var handler = SetupHandler(userPublicationRoleRepository.Object);

                // Deliberately set no expectations for checking user has any publication owner roles

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<DropMethodologyLinkRequirement, PublicationMethodology>(
                        user,
                        OwningLink);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(userPublicationRoleRepository);

                // No publication roles should allow dropping the link from a methodology to the owning publication
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task PublicationOwnerCanDropLinks()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    // If the user has Publication Owner role on the publication they are allowed to drop methodology links
                    var expectedToPassByPublicationRole = publicationRole == Owner;

                    var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                    var handler = SetupHandler(userPublicationRoleRepository.Object);

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, NonOwningLink.PublicationId))
                        .ReturnsAsync(ListOf(publicationRole));

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<DropMethodologyLinkRequirement, PublicationMethodology>(
                            user,
                            NonOwningLink);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(userPublicationRoleRepository);

                    Assert.Equal(expectedToPassByPublicationRole, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsCannotDropLinks()
            {
                var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                var handler = SetupHandler(userPublicationRoleRepository.Object);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, NonOwningLink.PublicationId))
                    .ReturnsAsync(new List<PublicationRole>());

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<DropMethodologyLinkRequirement, PublicationMethodology>(
                        user,
                        NonOwningLink);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(userPublicationRoleRepository);

                // A user with no role on the owning publication is not allowed to drop methodology links
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static DropMethodologyLinkAuthorizationHandler SetupHandler(
            IUserPublicationRoleRepository? userPublicationRoleRepository = null
        )
        {
            return new(
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
                    Mock.Of<IPublicationRepository>(Strict)));
        }
    }
}
