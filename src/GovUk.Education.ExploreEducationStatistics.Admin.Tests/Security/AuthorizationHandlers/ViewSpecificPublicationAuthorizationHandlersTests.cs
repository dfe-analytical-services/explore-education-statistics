using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ViewSpecificPublicationAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.PublicationAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

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
        public async Task CanSeeAllPublicationsAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary Publication
            // (and no other claim allows this)
            //
            // Note that we're deliberately using the "All RELEASES" claim rather than having to have a separate 
            // "All PUBLICATIONS" claim, as they're effectively the same
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificPublicationRequirement>(
                new CanSeeAllPublicationsAuthorizationHandler(),
                AccessAllReleases);
        }

        [Fact]
        public async Task HasOwnerOrApproverRoleOnPublicationAuthorizationHandler_Succeeds()
        {
            await AssertPublicationHandlerSucceedsWithPublicationRoles<
                ViewSpecificPublicationRequirement>(contentDbContext =>
                new HasOwnerOrApproverRoleOnPublicationAuthorizationHandler(
                    new AuthorizationHandlerResourceRoleService(
                        Mock.Of<IUserReleaseRoleRepository>(Strict),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPublicationRepository>(Strict))),
                Owner, Approver);
        }

        [Fact]
        public async Task HasRoleOnAnyChildReleaseAuthorizationHandler_NoReleasesOnThisPublicationForThisUser()
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
        public async Task HasRoleOnAnyChildReleaseAuthorizationHandler_HasRoleOnAReleaseOfThisPublication()
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
            await using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.UserReleaseRoles.AddRange(releaseRoles);
                await context.SaveChangesAsync();

                var handler = new ViewSpecificPublicationAuthorizationHandler(
                    context,
                    new AuthorizationHandlerResourceRoleService(
                        Mock.Of<IUserReleaseRoleRepository>(Strict),
                        new UserPublicationRoleRepository(context),
                        Mock.Of<IPublicationRepository>(Strict)));
                
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ViewSpecificPublicationRequirement()},
                    CreateClaimsPrincipal(_userId), _publication);

                await handler.HandleAsync(authContext);
                
                Assert.Equal(expectedToSucceed, authContext.HasSucceeded);
            }
        }
    }
}
