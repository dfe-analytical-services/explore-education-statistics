using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Draft
            };
            
            AssertHandlerSucceedsWithCorrectClaims(
                new UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler(), release, UpdateAllReleases);
        }
        
        [Fact]
        public void UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Draft
            };
            
            AssertHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(contentDbContext),
                release,
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
    }
}