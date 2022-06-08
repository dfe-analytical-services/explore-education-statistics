#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AdoptMethodologyForSpecificPublicationAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly Publication Publication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimsTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanAdoptAnyMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                    var handler = SetupHandler(userPublicationRoleRepository.Object);

                    // Only the AdoptAnyMethodology claim should allow adopting a methodology for a publication.
                    var expectedToPassByClaimAlone = claim == AdoptAnyMethodology;

                    if (!expectedToPassByClaimAlone)
                    {
                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                            .ReturnsAsync(new List<PublicationRole>());
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<AdoptMethodologyForSpecificPublicationRequirement,
                                Publication>
                            (user, Publication);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(userPublicationRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnerCanAdoptAnyMethodology()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                    var handler = SetupHandler(userPublicationRoleRepository.Object);

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUserAndPublication(UserId, Publication.Id))
                        .ReturnsAsync(CollectionUtils.ListOf(publicationRole));

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<AdoptMethodologyForSpecificPublicationRequirement,
                            Publication>(user, Publication);

                    await handler.HandleAsync(authContext);

                    VerifyAllMocks(userPublicationRoleRepository);

                    // As the user has Publication Owner role on the Publication they are allowed to adopt any methodology
                    Assert.Equal(publicationRole == Owner, authContext.HasSucceeded);
                });
            }
        }

        private static AdoptMethodologyForSpecificPublicationAuthorizationHandler SetupHandler(
            IUserPublicationRoleRepository? userPublicationRoleRepository = null
        )
        {
            return new AdoptMethodologyForSpecificPublicationAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
                    Mock.Of<IPublicationRepository>(Strict)));
        }
    }
}
