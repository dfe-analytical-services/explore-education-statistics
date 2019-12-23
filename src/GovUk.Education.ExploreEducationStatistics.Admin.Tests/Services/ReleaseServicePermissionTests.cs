using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public void GetAsync()
        {
            AssertSecurityPolicyChecked(CanViewSpecificRelease, 
                releaseService => releaseService.GetAsync(_release.Id));
        }
        
        [Fact]
        public void GetReleaseSummaryAsync()
        {
            AssertSecurityPolicyChecked(CanViewSpecificRelease, 
                releaseService => releaseService.GetReleaseSummaryAsync(_release.Id));
        }
        
        [Fact]
        public void UpdateReleaseStatusAsync_Draft()
        {
            AssertSecurityPolicyChecked(CanMarkSpecificReleaseAsDraft, 
                releaseService => 
                    releaseService.UpdateReleaseStatusAsync(_release.Id, ReleaseStatus.Draft, ""));
        }
        
        [Fact]
        public void UpdateReleaseStatusAsync_SubmitForHigherLevelReview()
        {
            AssertSecurityPolicyChecked(CanSubmitSpecificReleaseToHigherReview, 
                releaseService => 
                    releaseService.UpdateReleaseStatusAsync(_release.Id, ReleaseStatus.HigherLevelReview, ""));
        }
        
        [Fact]
        public void UpdateReleaseStatusAsync_Approve()
        {
            AssertSecurityPolicyChecked(CanApproveSpecificRelease, 
                releaseService => 
                    releaseService.UpdateReleaseStatusAsync(_release.Id, ReleaseStatus.Approved, ""));
        }
        
        [Fact]
        public async void GetMyReleasesForReleaseStatusesAsync_CanViewAllReleases()
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository) = Mocks();

            var list = new List<ReleaseViewModel>
            {
                new ReleaseViewModel
                {
                    Id = Guid.NewGuid()
                }
            };
            
            userService
                .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(true);

            repository
                .Setup(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved))
                .ReturnsAsync(list);
            
            var releaseService = new ReleaseService(contentDbContext.Object, MapperForProfile<MappingProfiles>(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
            
            var result = await releaseService.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(CanViewAllReleases));
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved));
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyReleasesForReleaseStatusesAsync_CanViewRelatedReleases()
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository) = Mocks();

            var list = new List<ReleaseViewModel>
            {
                new ReleaseViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            var userId = Guid.NewGuid();
            
            userService
                .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            
            userService
                .Setup(s => s.GetUserId())
                .Returns(userId);

            repository
                .Setup(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(userId, ReleaseStatus.Approved))
                .ReturnsAsync(list);
            
            var releaseService = new ReleaseService(contentDbContext.Object, MapperForProfile<MappingProfiles>(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
            
            var result = await releaseService.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(userId, ReleaseStatus.Approved));
            repository.VerifyNoOtherCalls();
        }
        
        private async void AssertSecurityPolicyChecked<T>(SecurityPolicies policy, 
            Func<ReleaseService, Task<Either<ActionResult, T>>> protectedAction)
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository) = Mocks();

            userService
                .Setup(s => s.MatchesPolicy(_release, policy))
                .ReturnsAsync(false);
            
            var releaseService = new ReleaseService(contentDbContext.Object, MapperForProfile<MappingProfiles>(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);

            var result = await protectedAction.Invoke(releaseService);

            AssertForbidden(result);
            
            userService
                .Verify(s => s.MatchesPolicy(_release, policy));
            userService
                .VerifyNoOtherCalls();
        }

        private static void AssertForbidden<T>(Either<ActionResult,T> result)
        {
            Assert.NotNull(result);
            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<ForbidResult>(result.Left);
        }

        private (
            Mock<IUserService> UserService, 
            Mock<IPersistenceHelper<Release,Guid>> ReleaseHelper, 
            Mock<IPublishingService> PublishingService,
            Mock<ContentDbContext> ContentDbContext,
            Mock<IReleaseRepository>) Mocks()
        {
            var userService = new Mock<IUserService>();

            var releaseHelper = new Mock<IPersistenceHelper<Release, Guid>>();

            releaseHelper
                .Setup(s => s.CheckEntityExistsActionResult(_release.Id, null))
                .ReturnsAsync(new Either<ActionResult, Release>(_release));
            
            return (userService, releaseHelper, new Mock<IPublishingService>(), new Mock<ContentDbContext>(), 
                new Mock<IReleaseRepository>());
        }
    }
}