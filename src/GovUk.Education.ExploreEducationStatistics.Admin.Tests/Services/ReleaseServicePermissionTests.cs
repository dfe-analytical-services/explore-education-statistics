using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
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
        
        private async void AssertSecurityPolicyChecked<T>(SecurityPolicies policy, 
            Func<ReleaseService, Task<Either<ActionResult, T>>> protectedAction)
        {
            var (userService, releaseHelper, publishingService, contentDbContext) = Mocks();

            userService
                .Setup(s => s.MatchesPolicy(_release, policy))
                .Returns(Task.FromResult(false));
            
            var releaseService = new ReleaseService(contentDbContext.Object, MapperForProfile<MappingProfiles>(), 
                publishingService.Object, releaseHelper.Object, userService.Object);

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
            Mock<ContentDbContext> ContentDbContext) Mocks()
        {
            var userService = new Mock<IUserService>();

            var releaseHelper = new Mock<IPersistenceHelper<Release, Guid>>();

            releaseHelper
                .Setup(s => s.CheckEntityExistsActionResult(_release.Id, null))
                .ReturnsAsync(new Either<ActionResult, Release>(_release));
            
            return (userService, releaseHelper, new Mock<IPublishingService>(), new Mock<ContentDbContext>());
        }
    }
}