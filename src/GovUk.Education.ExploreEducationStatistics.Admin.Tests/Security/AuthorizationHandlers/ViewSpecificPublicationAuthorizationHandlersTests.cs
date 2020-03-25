using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ViewSpecificPublicationAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificPublicationAuthorizationHandlersTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        private readonly Publication _publication = new Publication
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public void CanSeeAllPublicationsAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary Publication
            // (and no other claim allows this)
            //
            // Note that we're deliberately using the "All RELEASES" claim rather than having to have a separate 
            // "All PUBLICATIONS" claim, as they're effectively the same
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificPublicationRequirement>(
                new CanSeeAllPublicationsAuthorizationHandler(), AccessAllReleases);
        }
        
        [Fact]
        public async void HasRoleOnAnyChildReleaseAuthorizationHandler_NoReleasesOnThisPublicationForThisUser()
        {
            var releaseOnAnotherPublication = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };
            
            var releaseOnThisPublication = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publication.Id
            };
            
            var releaseRoleForDifferentPublication = new UserReleaseRole
            {
                UserId = _userId,
                Release = releaseOnAnotherPublication
            };
            
            var releaseRoleForDifferentUser = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                Release = releaseOnThisPublication
            };

            await AssertHasRoleOnAnyChildReleaseHandlesOk(
                false,
                releaseRoleForDifferentPublication,
                releaseRoleForDifferentUser);
        }
        
        [Fact]
        public async void HasRoleOnAnyChildReleaseAuthorizationHandler_HasRoleOnAReleaseOfThisPublication()
        {
            var releaseOnThisPublication = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publication.Id
            };

            var roleOnThisPublication = new UserReleaseRole
            {
                UserId = _userId,
                Release = releaseOnThisPublication
            };

            await AssertHasRoleOnAnyChildReleaseHandlesOk(true, roleOnThisPublication);
        }
        
        private async Task AssertHasRoleOnAnyChildReleaseHandlesOk(bool expectedToSucceed, params UserReleaseRole[] releaseRoles)
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.UserReleaseRoles.AddRange(releaseRoles);
                context.SaveChanges();

                var handler = new ViewSpecificPublicationAuthorizationHandler(context);
                
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ViewSpecificPublicationRequirement()},
                    CreateClaimsPrincipal(_userId), _publication);

                await handler.HandleAsync(authContext);
                
                Assert.Equal(expectedToSucceed, authContext.HasSucceeded);
            }
        }
    }
}