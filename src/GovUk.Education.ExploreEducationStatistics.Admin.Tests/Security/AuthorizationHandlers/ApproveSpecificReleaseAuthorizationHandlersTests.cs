using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ApproveSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void ApproveSpecificReleaseCanApproveAllReleasesAuthorizationHandler()
        {
            AssertHandlerSucceedsWithCorrectClaims(ApproveAllReleases);
        }
        
        [Fact]
        public void ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler()
        {
            AssertHandlerSucceedsWithCorrectReleaseRoles(ReleaseRole.Approver);
        }

        private void AssertHandlerSucceedsWithCorrectClaims(params SecurityClaimTypes[] succeedingClaims)
        {
            Enum.GetValues(typeof(SecurityClaimTypes)).Cast<SecurityClaimTypes>().ToList().ForEach(async claim =>
            {
                var handler = new ApproveSpecificReleaseCanApproveAllReleasesAuthorizationHandler();

                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(claim.ToString(), ""));
                var user = new ClaimsPrincipal(identity);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ApproveSpecificReleaseRequirement()},
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

        private void AssertHandlerSucceedsWithCorrectReleaseRoles(params ReleaseRole[] succeedingRoles)
        {
            Enum.GetValues(typeof(ReleaseRole)).Cast<ReleaseRole>().ToList().ForEach(async role =>
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
                
                var handler = new ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler(contentDbContext.Object);

                var authContext = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] {new ApproveSpecificReleaseRequirement()},
                    user, release);

                await handler.HandleAsync(authContext);

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