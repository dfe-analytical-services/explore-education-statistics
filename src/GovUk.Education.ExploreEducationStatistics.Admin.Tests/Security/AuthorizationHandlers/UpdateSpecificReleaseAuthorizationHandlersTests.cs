using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using Xunit.Sdk;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "UpdateAllReleases" claim can update an arbitrary Release if it is not
            // in Approved state
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaimsWithCorrectReleaseStatuses(
                ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview);
        }
        
        [Fact]
        public void UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can update a Release
            // if it is not in Approved state
            // (and no other role or Release Status)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRolesWithCorrectReleaseStatuses(
                ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview);
        }

        private void AssertReleaseHandlerSucceedsWithCorrectClaimsWithCorrectReleaseStatuses(params ReleaseStatus[] succeedingStatuses)
        {
            GetEnumValues<ReleaseStatus>().ForEach(status =>
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Status = status
                };

                if (succeedingStatuses.Contains(status))
                {
                    AssertReleaseHandlerSucceedsWithCorrectClaims(new UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler(), release, UpdateAllReleases);
                }
                else
                {
                    AssertReleaseHandlerSucceedsWithCorrectClaims(new UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler(), release);
                }
            });
        }

        private void AssertReleaseHandlerSucceedsWithCorrectReleaseRolesWithCorrectReleaseStatuses(params ReleaseStatus[] succeedingStatuses)
        {
            GetEnumValues<ReleaseStatus>().ForEach(status =>
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Status = status
                };

                if (succeedingStatuses.Contains(status))
                {
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles(
                        contentDbContext => new UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(contentDbContext),
                        release,
                        ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
                }
                else
                {
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles(
                        contentDbContext => new UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(contentDbContext),
                        release);
                }
            });
        }
    }
}