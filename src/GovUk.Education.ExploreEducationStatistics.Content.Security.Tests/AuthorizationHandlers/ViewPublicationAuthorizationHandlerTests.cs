#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlerContextFactory;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Tests.AuthorizationHandlers
{
    public class ViewPublicationAuthorizationHandlerTests
    {
        [Fact]
        public async Task HasPublishedRelease()
        {
            var publication = new Publication
            {
                LatestPublishedReleaseId = Guid.NewGuid()
            };

            var handler = new ViewPublicationAuthorizationHandler();
            var authContext = CreateAnonymousAuthContext<ViewPublicationRequirement, Publication>(publication);
            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }

        [Fact]
        public async Task HasNoPublishedRelease()
        {
            var publication = new Publication();

            var handler = new ViewPublicationAuthorizationHandler();
            var authContext = CreateAnonymousAuthContext<ViewPublicationRequirement, Publication>(publication);
            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }
}
