using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Moq;
using Xunit;
using ReleaseStatusOverallStage = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;
using PublisherReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.SubmitSpecificReleaseToHigherReviewAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class SubmitSpecificReleaseToHigherReviewAuthorizationHandlersTests
    {
        [Fact]
        public void CanSubmitAllReleasesToHigherReviewAuthorizationHandler()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

            releaseStatusRepository.Setup(s => s.GetAllByOverallStage(
                release.Id, 
                ReleaseStatusOverallStage.Started, 
                ReleaseStatusOverallStage.Complete
            ))
                .ReturnsAsync(new List<PublisherReleaseStatus> {});

            // Assert that any users with the "SubmitAllReleasesToHigherReview" claim can submit an arbitrary Release to higher review
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(releaseStatusRepository.Object), 
                SubmitAllReleasesToHigherReview
            );
        }
        
        [Fact]
        public void CanSubmitAllReleasesToHigherReviewAuthorizationHandler_ReleaseUnpublished()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Approved
            };

            var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

            releaseStatusRepository.Setup(s => s.GetAllByOverallStage(
                release.Id, 
                ReleaseStatusOverallStage.Started, 
                ReleaseStatusOverallStage.Complete
            ))
                .ReturnsAsync(new List<PublisherReleaseStatus> {});
            
            // Assert that any users with the "SubmitAllReleasesToHigherReview" claim can submit an arbitrary Release to higher review
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(releaseStatusRepository.Object),
                release,
                SubmitAllReleasesToHigherReview
            );
        }

        [Fact]
        public void CanSubmitAllReleasesToHigherReviewAuthorizationHandler_ReleaseStartedPublishing()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Approved,
                Published = DateTime.Now
            };

            var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

            releaseStatusRepository.Setup(s => s.GetAllByOverallStage(
                release.Id, 
                ReleaseStatusOverallStage.Started, 
                ReleaseStatusOverallStage.Complete
            ))
                .ReturnsAsync(new List<PublisherReleaseStatus> {});
            
            // Assert that no users can submit a Release to higher review once it has started publishing
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(releaseStatusRepository.Object),
                release
            );
        }

        [Fact]
        public void CanSubmitAllReleasesToHigherReviewAuthorizationHandler_ReleasePublished()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Approved,
                Published = DateTime.Now
            };

            var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

            releaseStatusRepository.Setup(s => s.GetAllByOverallStage(
                release.Id, 
                ReleaseStatusOverallStage.Started, 
                ReleaseStatusOverallStage.Complete
            ))
                .ReturnsAsync(new List<PublisherReleaseStatus> {});
            
            // Assert that no users can submit a Release to higher review once it has published
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(releaseStatusRepository.Object),
                release
            );
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can submit the Release
            // to higher review
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<SubmitSpecificReleaseToHigherReviewRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Status = ReleaseStatus.Approved
            };
            
            // Assert that no User can submit the Release to higher review if it is already Approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<SubmitSpecificReleaseToHigherReviewRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                release);
        }
    }
}