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

        private readonly Publication _publication = new Publication
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public void GetAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                service.GetAsync(_release.Id), 
                _release,
                CanViewSpecificRelease);
        }
        
        [Fact]
        public void GetReleaseSummaryAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                service.GetReleaseSummaryAsync(_release.Id),  
                _release,
                CanViewSpecificRelease);
        }
        
        [Fact]
        public void CreateReleaseAsync()
        {
            AssertSecurityPoliciesChecked(
                service => service.CreateReleaseAsync(new CreateReleaseViewModel
                {
                    PublicationId = _publication.Id,
                }), 
                _publication, 
                CanCreateReleaseForSpecificPublication);
        }
        
        [Fact]
        public void UpdateReleaseStatusAsync_Draft()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateReleaseStatusAsync(_release.Id, ReleaseStatus.Draft, ""),  
                _release,
                CanMarkSpecificReleaseAsDraft);
        }
        
        [Fact]
        public void UpdateReleaseStatusAsync_SubmitForHigherLevelReview()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateReleaseStatusAsync(_release.Id, ReleaseStatus.HigherLevelReview, ""),  
                _release,
                CanSubmitSpecificReleaseToHigherReview);
        }
        
        [Fact]
        public void UpdateReleaseStatusAsync_Approve()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateReleaseStatusAsync(_release.Id, ReleaseStatus.Approved, ""),  
                _release,
                CanApproveSpecificRelease);
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
            
            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
            
            var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
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
            
            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
            
            var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(userId, ReleaseStatus.Approved));
            repository.VerifyNoOtherCalls();
        }
        
        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<ReleaseService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity, params SecurityPolicies[] policies)
            where TEntity : class
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository) = Mocks();

            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, userService, service, policies);
        }
        
        private (
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IPublishingService>,
            Mock<ContentDbContext>,
            Mock<IReleaseRepository>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(persistenceHelper, _release.Id, _release);
            MockUtils.SetupCall(persistenceHelper, _publication.Id, _publication);
            
            return (
                new Mock<IUserService>(), 
                persistenceHelper, 
                new Mock<IPublishingService>(), 
                new Mock<ContentDbContext>(), 
                new Mock<IReleaseRepository>());
        }
    }
}