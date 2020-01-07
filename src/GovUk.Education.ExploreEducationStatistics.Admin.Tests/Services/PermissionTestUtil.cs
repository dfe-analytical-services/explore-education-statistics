using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class PermissionTestUtil
    {
        public static async void AssertSecurityPoliciesChecked<TProtectedResource, TReturn, TService>(
            Func<TService, Task<Either<ActionResult, TReturn>>> protectedAction, 
            TProtectedResource resource, 
            Mock<IUserService> userService,
            TService service,
            params SecurityPolicies[] policies)
        {
            policies.ToList().ForEach(policy => 
                userService
                    .Setup(s => s.MatchesPolicy(resource, policy))
                    .ReturnsAsync(policy != policies.Last()));
            
            var result = await protectedAction.Invoke(service);

            AssertForbidden(result);
            
            policies.ToList().ForEach(policy =>
                userService.Verify(s => s.MatchesPolicy(resource, policy)));
            
            userService.VerifyNoOtherCalls();
        }

        private static void AssertForbidden<T>(Either<ActionResult,T> result)
        {
            Assert.NotNull(result);
            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<ForbidResult>(result.Left);
        }

        public static void AssertHandlerSucceedsWithCorrectClaims<TRequirement>(
            AuthorizationHandler<TRequirement> handler, 
            params SecurityClaimTypes[] succeedingClaims) 
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<SecurityClaimTypes>().ForEach(async claim =>
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(claim.ToString(), ""));
                var user = new ClaimsPrincipal(identity);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()},
                    user, new Release());

                await handler.HandleAsync(authContext);

                if (succeedingClaims.Contains(claim))
                {
                    Assert.True(authContext.HasSucceeded, "Expected claim " + claim + " to have made the handler succeed"); 
                }
                else
                {
                    Assert.False(authContext.HasSucceeded, "Expected claim " + claim + " to have made the handler fail"); 
                }
            });
        }
        
        public static void AssertHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
            Func<ContentDbContext, AuthorizationHandler<TRequirement, Release>> handlerSupplier,
            params ReleaseRole[] succeedingRoles) 
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<ReleaseRole>().ForEach(async role =>
            {
                var release = new Release
                {
                    Id = Guid.NewGuid()
                };

                var userId = Guid.NewGuid();
                
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                var user = new ClaimsPrincipal(identity);

                // add a new UserReleaseRole for the current User and ReleaseRole
                var rolesList = new List<UserReleaseRole>
                {
                    new UserReleaseRole
                    {
                        ReleaseId = release.Id,
                        UserId = userId,
                        Role = role
                    }
                };
                
                // add some roles to unrelated Users to ensure that only the current User is being
                // taken into consideration
                succeedingRoles.ToList().ForEach(roleExpectedToSucceed =>
                {
                    rolesList.Add(new UserReleaseRole
                    {
                        ReleaseId = release.Id,
                        UserId = Guid.NewGuid(),
                        Role = roleExpectedToSucceed
                    });
                });
                
                // add some roles to unrelated Releases to ensure that only the Release under test is being
                // taken into consideration
                succeedingRoles.ToList().ForEach(roleExpectedToSucceed =>
                {
                    rolesList.Add(new UserReleaseRole
                    {
                        ReleaseId = Guid.NewGuid(),
                        UserId = userId,
                        Role = roleExpectedToSucceed
                    });
                });
               
                var contentDbContext = new Mock<ContentDbContext>();

                contentDbContext
                    .Setup(s => s.UserReleaseRoles)
                    .ReturnsDbSet(rolesList);
                
                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()},
                    user, release);

                await handlerSupplier(contentDbContext.Object).HandleAsync(authContext);

                if (succeedingRoles.Contains(role))
                {
                    Assert.True(authContext.HasSucceeded, "Expected role " + role + " to have made the handler succeed"); 
                }
                else
                {
                    Assert.False(authContext.HasSucceeded, "Expected role " + role + " to have made the handler fail"); 
                }
            });
        }
    }
}