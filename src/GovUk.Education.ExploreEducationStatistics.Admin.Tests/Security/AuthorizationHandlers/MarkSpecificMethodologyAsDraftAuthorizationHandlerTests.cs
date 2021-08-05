#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.
    MethodologyStatusAuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MarkSpecificMethodologyAsDraftAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        public class MarkSpecificMethodologyAsDraftAuthorizationHandlerClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowMarkingPubliclyAccessibleMethodologyAsDraft()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid()
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<MarkSpecificMethodologyAsDraftRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow a publicly accessible Methodology to be marked as draft
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanMarkNonPubliclyAccessibleMethodologyAsDraft()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid()
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<MarkSpecificMethodologyAsDraftRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // Only the MarkAllMethodologiesDraft claim should allow a non publicly accessible Methodology to be updated
                    var expectedToPass = claim == MarkAllMethodologiesDraft;
                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }
        }

        
            
        private static (
            MarkSpecificMethodologyAsDraftAuthorizationHandler, 
            Mock<IMethodologyRepository>,
            Mock<IPublicationRepository>,
            Mock<IUserReleaseRoleRepository>
            )
            CreateHandlerAndDependencies(ContentDbContext? contentDbContext = null)
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

            var handler = new MarkSpecificMethodologyAsDraftAuthorizationHandler(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                methodologyRepository.Object,
                publicationRepository.Object,
                userReleaseRoleRepository.Object
            );

            return (
                handler,
                methodologyRepository,
                publicationRepository, 
                userReleaseRoleRepository
            );
        }
    }
}
