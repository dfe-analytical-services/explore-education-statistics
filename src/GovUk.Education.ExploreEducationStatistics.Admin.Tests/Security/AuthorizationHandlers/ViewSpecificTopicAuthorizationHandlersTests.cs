using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ViewSpecificTopicAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificTopicAuthorizationHandlersTests
    {
        [Fact]
        public void CanViewAllTopics()
        {
            // Assert that any users with the "AccessAllTopics" claim can view an arbitrary Theme
            // (and no other claim allows this)
            //
            // Note that we're deliberately using the "All TOPICS" claim rather than having to have a separate 
            // "All THEMES" claim, as they're effectively the same
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificTopicRequirement>(
                new CanViewAllTopics(), AccessAllTopics);
        }
        
        [Fact]
        public async void HasRoleOnAnyChildRelease_NoReleasesForThisTopicAndUser()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                var userId = Guid.NewGuid();
                var topic = new Topic
                {
                    ThemeId = Guid.NewGuid()
                };

                var releaseRoleForDifferentTopic = new UserReleaseRole
                {
                    UserId = userId,
                    Release = new Release
                    {
                        Publication = new Publication
                        {
                            Topic = new Topic
                            {
                                ThemeId = Guid.NewGuid()
                            }
                        }
                    }
                };
                var releaseRoleForDifferentUser = new UserReleaseRole
                {
                    UserId = Guid.NewGuid(),
                    Release = new Release
                    {
                        Publication = new Publication
                        {
                            Topic = topic
                        }
                    }
                };

                context.UserReleaseRoles.AddRange(releaseRoleForDifferentTopic, releaseRoleForDifferentUser);
                context.SaveChanges();

                var handler = new ViewSpecificTopicAuthorizationHandler(context);
                
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { new ViewSpecificTopicRequirement() },
                    CreateClaimsPrincipal(userId),
                    topic
                );

                await handler.HandleAsync(authContext);
                
                Assert.False(authContext.HasSucceeded);
            }
        }
        
        [Fact]
        public async void HasRoleOnAnyChildRelease_HasRoleOnAReleaseOfThisTopic()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                var userId = Guid.NewGuid();
                var topic = new Topic
                {
                    ThemeId = Guid.NewGuid()
                };

                var releaseRoleOnThisTopic = new UserReleaseRole
                {
                    UserId = userId,
                    Release = new Release
                    {
                        Publication = new Publication
                        {
                            Topic = topic
                        }
                    }
                };

                context.UserReleaseRoles.Add(releaseRoleOnThisTopic);
                context.SaveChanges();

                var handler = new ViewSpecificTopicAuthorizationHandler(context);
                
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { new ViewSpecificTopicRequirement() },
                    CreateClaimsPrincipal(userId),
                    topic
                );

                await handler.HandleAsync(authContext);
                
                Assert.True(authContext.HasSucceeded);
            }
        }
    }
}