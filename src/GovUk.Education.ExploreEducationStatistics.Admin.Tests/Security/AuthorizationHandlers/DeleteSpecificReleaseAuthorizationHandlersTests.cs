using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.DeleteSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class DeleteSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanDeleteAllReleasesAuthorizationHandler_NotAmendment()
        {
            // Assert that no users can delete a non-amendment release
            AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement>(
                new CanDeleteAllReleaseAmendmentsAuthorizationHandler(), new Release
                {
                    Status = ReleaseStatus.Draft
                });
        }
        
        [Fact]
        public void CanDeleteAllReleasesAuthorizationHandler_AmendmentButApproved()
        {
            // Assert that no users can delete an amendment release that is approved
            AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement>(
                new CanDeleteAllReleaseAmendmentsAuthorizationHandler(), new Release
                {
                    Status = ReleaseStatus.Approved,
                    Version = 1
                });
        }
        
        [Fact]
        public void CanDeleteAllReleasesAuthorizationHandler_UnapprovedAmendment()
        {
            // Assert that users with the "DeleteAllReleaseAmendments" can delete an amendment release that is not
            // yet approved
            AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement>(
                new CanDeleteAllReleaseAmendmentsAuthorizationHandler(), new Release
                {
                    Status = ReleaseStatus.Draft,
                    Version = 1
                }, DeleteAllReleaseAmendments);
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAmendmentAuthorizationHandler_NotAmendment()
        {
            // Assert that no User Release roles will allow a Release to be deleted that is not an Amendment
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAmendmentAuthorizationHandler(contentDbContext), 
                new Release
                {
                    Status = ReleaseStatus.Draft
                });
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAmendmentAuthorizationHandler_AmendmentButApproved()
        {
            // Assert that no User Release roles will allow an Amendment to be deleted when it is Approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAmendmentAuthorizationHandler(contentDbContext), new Release
                {
                    Version = 1,
                    Status = ReleaseStatus.Approved
                });
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAmendmentAuthorizationHandler_UnapprovedAmendment()
        {
            // Assert that users with an editor role on the release amendment can delete it if it is not yet approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAmendmentAuthorizationHandler(contentDbContext),
                new Release
                {
                    Status = ReleaseStatus.Draft,
                    Version = 1
                },
                ReleaseRole.Contributor, ReleaseRole.Approver, ReleaseRole.Lead);
        }
    }
}