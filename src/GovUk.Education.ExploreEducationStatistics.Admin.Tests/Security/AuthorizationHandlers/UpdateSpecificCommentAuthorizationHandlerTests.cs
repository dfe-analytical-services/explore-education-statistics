using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificCommentAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanUpdateOwnCommentAuthorizationHandler_MatchingUser()
        {
            var user = CreateClaimsPrincipal(Guid.NewGuid());
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                CreatedById = user.GetUserId()
            };

            var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new UpdateSpecificCommentRequirement()}, user, comment);

            await new CanUpdateOwnCommentAuthorizationHandler().HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded, "Expected matching user to have caused the handler to fail");
        }

        [Fact]
        public async Task CanUpdateOwnCommentAuthorizationHandler_MismatchingUser()
        {
            var user = CreateClaimsPrincipal(Guid.NewGuid());
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                CreatedById = Guid.NewGuid()
            };

            var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new UpdateSpecificCommentRequirement()}, user, comment);

            await new CanUpdateOwnCommentAuthorizationHandler().HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded, "Expected different user to have caused the handler to fail");
        }
    }
}
