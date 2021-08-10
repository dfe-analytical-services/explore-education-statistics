#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.
    MethodologyStatusAuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ApproveSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        public class ApproveSpecificMethodologyAuthorizationHandlerClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowApprovingPubliclyAccessibleMethodology()
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
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow a publicly accessible Methodology to be approved
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonPubliclyAccessibleDraftMethodology()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Status = Draft
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPass = claim == SecurityClaimTypes.ApproveAllMethodologies;
                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonPubliclyAccessibleApprovedMethodology()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Status = Approved
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPass = claim == SecurityClaimTypes.ApproveAllMethodologies;
                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }
        }

        private static
            (ApproveSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IPublicationRepository>,
            Mock<IUserReleaseRoleRepository>
            )
            CreateHandlerAndDependencies(ContentDbContext? contentDbContext = null)
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

            var handler = new ApproveSpecificMethodologyAuthorizationHandler(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                methodologyRepository.Object,
                publicationRepository.Object,
                userReleaseRoleRepository.Object);

            return (
                handler,
                methodologyRepository,
                publicationRepository,
                userReleaseRoleRepository
            );
        }
    }
}
