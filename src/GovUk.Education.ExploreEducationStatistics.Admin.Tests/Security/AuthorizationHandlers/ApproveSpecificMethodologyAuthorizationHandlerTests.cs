using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.MethodologyStatusAuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ApproveSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        public class ApproveSpecificMethodologyAuthorizationHandlerClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowApprovingMethodologyWhichIsApproved()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Status = Approved
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var handler = new ApproveSpecificMethodologyAuthorizationHandler();

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);

                    // No claims should allow approving a Methodology which is already approved
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveDraftMethodology()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Status = Draft
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var handler = new ApproveSpecificMethodologyAuthorizationHandler();

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);

                    // Only the ApproveAllMethodologies claim should allow approving a draft Methodology
                    var expectedToPass = claim == SecurityClaimTypes.ApproveAllMethodologies;
                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }
        }
    }
}
