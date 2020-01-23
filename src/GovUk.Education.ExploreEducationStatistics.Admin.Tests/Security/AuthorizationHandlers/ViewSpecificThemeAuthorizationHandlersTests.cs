using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificThemeAuthorizationHandlersTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        private readonly Theme _theme = new Theme
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public void CanSeeAllThemesAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllTopics" claim can view an arbitrary Theme
            // (and no other claim allows this)
            //
            // Note that we're deliberately using the "All TOPICS" claim rather than having to have a separate 
            // "All THEMES" claim, as they're effectively the same
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificThemeRequirement>(
                new CanSeeAllThemesAuthorizationHandler(), AccessAllTopics);
        }
        
        [Fact]
        public async void HasRoleOnAnyChildPublicationAuthorizationHandler_NoReleasesOnThisThemeForThisUser()
        {
            var releaseOnAnotherTheme = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Topic = new Topic
                    {
                        ThemeId = Guid.NewGuid()
                    }
                }
            };
            
            var releaseOnThisTheme = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Topic = new Topic
                    {
                        ThemeId = _theme.Id
                    }
                }
            };
            
            var releaseRoleForDifferentTheme = new UserReleaseRole
            {
                UserId = _userId,
                Release = releaseOnAnotherTheme
            };
            
            var releaseRoleForDifferentUser = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                Release = releaseOnThisTheme
            };

            await AssertHasRoleOnAnyChildReleaseHandlesOk(
                false,
                releaseRoleForDifferentTheme,
                releaseRoleForDifferentUser);
        }
        
        [Fact]
        public async void HasRoleOnAnyChildReleaseAuthorizationHandler_HasRoleOnAReleaseOfThisTheme()
        {
            var releaseOnThisTheme = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Topic = new Topic
                    {
                        ThemeId = _theme.Id
                    }
                }
            };

            var roleOnThisTheme = new UserReleaseRole
            {
                UserId = _userId,
                Release = releaseOnThisTheme
            };

            await AssertHasRoleOnAnyChildReleaseHandlesOk(true, roleOnThisTheme);
        }
        
        private async Task AssertHasRoleOnAnyChildReleaseHandlesOk(bool expectedToSucceed, params UserReleaseRole[] releaseRoles)
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.UserReleaseRoles.AddRange(releaseRoles);
                context.SaveChanges();

                var handler = new ViewSpecificThemeAuthorizationHandler(context);
                
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ViewSpecificThemeRequirement()},
                    CreateClaimsPrincipal(_userId), _theme);

                await handler.HandleAsync(authContext);
                
                Assert.Equal(expectedToSucceed, authContext.HasSucceeded);
            }
        }
    }
}