using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using ReleaseStatusOverallStage = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;
using PublisherReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ApproveSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ApproveSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanApproveAllReleasesAuthorizationHandler()
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

            // Assert that any users with the "ApproveAllReleases" claim can approve an arbitrary Release
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new CanApproveAllReleasesAuthorizationHandler(releaseStatusRepository.Object), 
                ApproveAllReleases
            );
        }

        [Fact]
        public void CanApproveAllReleasesAuthorizationHandler_ReleaseUnpublished()
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
            
            // Assert that any users with the "ApproveAllReleases" claim can approve an arbitrary Release
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new CanApproveAllReleasesAuthorizationHandler(releaseStatusRepository.Object),
                release,
                ApproveAllReleases
            );
        }

        [Fact]
        public void CanApproveAllReleasesAuthorizationHandler_ReleaseStartedPublishing()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Approved,
            };

            var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

            releaseStatusRepository.Setup(s => s.GetAllByOverallStage(
                release.Id, 
                ReleaseStatusOverallStage.Started,
                ReleaseStatusOverallStage.Complete
            ))
                .ReturnsAsync(new List<PublisherReleaseStatus> {
                    new PublisherReleaseStatus()
                });
            
            // Assert that no users can approve a Release that has started publishing
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new CanApproveAllReleasesAuthorizationHandler(releaseStatusRepository.Object),
                release
            );
        }

        [Fact]
        public void CanApproveAllReleasesAuthorizationHandler_ReleasePublished()
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
            
            // Assert that no users can approve a published Release
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new CanApproveAllReleasesAuthorizationHandler(releaseStatusRepository.Object),
                release
            );
        }
        
        [Fact]
        public void HasApproverRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Approver" role on a Release can approve the Release
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ApproveSpecificReleaseRequirement>(
                contentDbContext => new HasApproverRoleOnReleaseAuthorizationHandler(contentDbContext), 
                ReleaseRole.Approver);
        }
        
        [Fact]
        public void HasApproverRoleOnReleaseAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Status = ReleaseStatus.Approved
            };
            
            // Assert that no User can approve the Release if it is already Approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ApproveSpecificReleaseRequirement>(
                contentDbContext => new HasApproverRoleOnReleaseAuthorizationHandler(contentDbContext),
                release);
        }
    }
}