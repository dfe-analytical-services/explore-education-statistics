using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.MakeAmendmentOfSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanMakeAmendmentOfAllReleasesAuthorizationHandler()
        {
            // Assert that no users can amend a non-Live Release
            AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                new CanMakeAmendmentOfAllReleasesAuthorizationHandler());
        }
        
        [Fact]
        public void MakeAmendmentOfSpecificReleaseCanMakeAmendmentOfAllReleasesAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Published = DateTime.UtcNow
            };
            
            // Assert that any users with the "MakeAmendmentOfAllReleases" claim can approve an arbitrary Release
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                new CanMakeAmendmentOfAllReleasesAuthorizationHandler(),
                release, MakeAmendmentsOfAllReleases);
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler_ReleaseNotYetLive()
        {
            // Assert that no User Release roles will allow an Amendment to be made when the Release is not yet Live
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext));
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler_ReleaseLive()
        {
            var release = new Release
            {
                Published = DateTime.UtcNow
            };
            
            // Assert that users with an editor role on the release can amend it if it is Live
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                release,
                ReleaseRole.Contributor, ReleaseRole.Approver, ReleaseRole.Lead);
        }
    }
}