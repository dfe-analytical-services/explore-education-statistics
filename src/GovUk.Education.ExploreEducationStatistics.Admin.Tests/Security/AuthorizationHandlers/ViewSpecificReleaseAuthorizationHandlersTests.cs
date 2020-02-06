using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Configuration.UserSecrets;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanSeeAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary Release
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificReleaseRequirement>(
                new CanSeeAllReleasesAuthorizationHandler(), AccessAllReleases);
        }
        
        [Fact]
        public void HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has any unrestricted viewer role on a Release can view the Release
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificReleaseRequirement>(
                contentDbContext => new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Viewer, ReleaseRole.Lead, ReleaseRole.Contributor, ReleaseRole.Approver);
        }
        
        [Fact]
        public void HasPreReleaseRoleWithinAccessWindowAuthorizationHandler()
        {
            var release = new Release();
            
            var preReleaseService = new Mock<IPreReleaseService>();

            preReleaseService
                .Setup(s => s.GetPreReleaseWindowStatus(release, It.IsAny<DateTime>()))
                .Returns(new PreReleaseWindowStatus
                {
                    PreReleaseAccess = PreReleaseAccess.Within
                });
            
            // Assert that a User who specifically has the Pre Release role will cause this handler to pass 
            // IF the Pre Release window is open
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificReleaseRequirement>(
                contentDbContext => new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                    contentDbContext, preReleaseService.Object),
                release,
                ReleaseRole.PrereleaseViewer);
        }
        
        [Fact]
        public void HasPreReleaseRoleWithinAccessWindowAuthorizationHandler_PreReleaseWindowNotOpen()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };
            
            var preReleaseService = new Mock<IPreReleaseService>();

            var userId = Guid.NewGuid();
            
            var failureScenario = new ReleaseHandlerTestScenario
            {
                Release = release,
                User = CreateClaimsPrincipal(userId),
                UserReleaseRoles = new List<UserReleaseRole>
                {
                    new UserReleaseRole
                    {
                        ReleaseId = release.Id,
                        UserId = userId,
                        Role = ReleaseRole.PrereleaseViewer
                    }
                },
                ExpectedToPass = false,
                UnexpectedPassMessage = "Expected the test to fail because the Pre Release window is not open at the " +
                                        "current time"
            };
                
            GetEnumValues<PreReleaseAccess>()
                .Where(value => value != PreReleaseAccess.Within)
                .ToList()
                .ForEach(access =>
                {
                    preReleaseService
                        .Setup(s => s.GetPreReleaseWindowStatus(release, It.IsAny<DateTime>()))
                        .Returns(new PreReleaseWindowStatus
                        {
                            PreReleaseAccess = access
                        });
            
                    // Assert that a User who specifically has the Pre Release role will cause this handler to fail 
                    // IF the Pre Release window is NOT open
                    AssertReleaseHandlerHandlesScenarioSuccessfully<ViewSpecificReleaseRequirement>(
                        contentDbContext => new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                            contentDbContext, preReleaseService.Object),
                        failureScenario);           
                });
        }
    }
}