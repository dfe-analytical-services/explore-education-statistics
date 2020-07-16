using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using ReleaseStatusOverallStage = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;
using PublisherReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.MarkSpecificReleaseAsDraftAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftAuthorizationHandlersTests
    {
        [Fact]
        public void CanMarkAllReleasesAsDraftAuthorizationHandler()
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

            // Assert that any users with the "MarkAllReleasesAsDraft" claim can mark an arbitrary Release as Draft
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<MarkSpecificReleaseAsDraftRequirement>(
                new CanMarkAllReleasesAsDraftAuthorizationHandler(releaseStatusRepository.Object),
                MarkAllReleasesAsDraft
            );
        }

        [Fact]
        public void CanMarkAllReleasesAsDraftAuthorizationHandler_ReleaseNotPublished()
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
                .ReturnsAsync(new List<PublisherReleaseStatus> {});

            
            // Assert that any users with the "MarkAllReleasesAsDraft" claim can mark an arbitrary Release as Draft
            AssertReleaseHandlerSucceedsWithCorrectClaims<MarkSpecificReleaseAsDraftRequirement>(
                new CanMarkAllReleasesAsDraftAuthorizationHandler(releaseStatusRepository.Object),
                release,
                MarkAllReleasesAsDraft
            );
        }

        [Fact]
        public void CanMarkAllReleasesAsDraftAuthorizationHandler_ReleaseStartedPublishing()
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
                .ReturnsAsync(new List<PublisherReleaseStatus> {
                    new PublisherReleaseStatus()
                });

            // Assert that no users can mark a Release as Draft once it has started publishing
            AssertReleaseHandlerSucceedsWithCorrectClaims<MarkSpecificReleaseAsDraftRequirement>(
                new CanMarkAllReleasesAsDraftAuthorizationHandler(releaseStatusRepository.Object),
                release
            );
        }

        [Fact]
        public void CanMarkAllReleasesAsDraftAuthorizationHandler_ReleasePublished()
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

            // Assert that no users can mark a published Release as Draft
            AssertReleaseHandlerSucceedsWithCorrectClaims<MarkSpecificReleaseAsDraftRequirement>(
                new CanMarkAllReleasesAsDraftAuthorizationHandler(releaseStatusRepository.Object),
                release
            );
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can mark the Release
            // as Draft
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkSpecificReleaseAsDraftRequirement>(
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
            
            // Assert that no User can mark the Release as Draft if it is already Approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkSpecificReleaseAsDraftRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                release);
        }
    }
}