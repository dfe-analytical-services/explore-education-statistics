#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ManageExternalMethodologyForSpecificPublicationAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly Publication Publication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanManageExternalMethodologyForAnyPublication()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies();

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, Publication);

                    var expectedToPassByClaimAlone = claim == CreateAnyMethodology;

                    if (!expectedToPassByClaimAlone)
                    {
                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                            .ReturnsAsync(new List<PublicationRole>());
                    }

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(userPublicationRoleRepository);

                    // Verify that the presence of the "CreateAnyMethodology" Claim will pass the handler test, without
                    // the need for a specific Publication to be provided
                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task UserCanManageExternalMethodologyForPublicationWithPublicationOwnerRole()
            {
                await using var context = InMemoryApplicationDbContext();
                context.Attach(Publication);

                var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies();

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, Publication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                    .ReturnsAsync(CollectionUtils.ListOf(Owner));

                await handler.HandleAsync(authContext);
                VerifyAllMocks(userPublicationRoleRepository);

                // Verify that the user can create a Methodology for this Publication by virtue of having a Publication
                // Owner role on the Publication
                Assert.True(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserCannotManageExternalMethodologyForPublicationWithoutPublicationOwnerRole()
            {
                var (handler, userPublicationRoleRepository) = CreateHandlerAndDependencies();

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, Publication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                await handler.HandleAsync(authContext);
                VerifyAllMocks(userPublicationRoleRepository);

                // Verify that the user can't create a Methodology for this Publication because they don't have 
                // Publication Owner role on it
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user, Publication publication)
        {
            return CreateAuthorizationHandlerContext<ManageExternalMethodologyForSpecificPublicationRequirement,
                    Publication>
                (user, publication);
        }

        private static (ManageExternalMethodologyForSpecificPublicationAuthorizationHandler,
            Mock<IUserPublicationRoleRepository>)
            CreateHandlerAndDependencies()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            var handler = new ManageExternalMethodologyForSpecificPublicationAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    userPublicationRoleRepository.Object,
                    Mock.Of<IPublicationRepository>(Strict)));

            return (handler, userPublicationRoleRepository);
        }
    }
}
