using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServicePermissionTests
    {
        private static readonly Publication Publication = new Publication
        {
            Id = Guid.NewGuid()
        };

        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = Publication.Id,
            Published = DateTime.Now,
            TimePeriodCoverage = TimeIdentifier.April
        };
        
        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public void GetReleaseForIdAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.GetReleaseForIdAsync(_release.Id),  
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
                    PublicationId = Publication.Id,
                }), 
                Publication, 
                CanCreateReleaseForSpecificPublication);
        }
        
        [Fact]
        public void EditReleaseSummaryAsync()
        {
            AssertSecurityPoliciesChecked(
                service => service.EditReleaseSummaryAsync(_release.Id, new UpdateReleaseSummaryRequest()), 
                _release, 
                CanUpdateSpecificRelease);
        }
        
        [Fact]
        public void GetLatestReleaseAsync()
        {
            AssertSecurityPoliciesChecked(
                service => service.GetLatestReleaseAsync(Publication.Id), 
                Publication, 
                CanViewSpecificPublication);
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
            var (userService, releaseHelper, publishingService, contentDbContext, repository, 
                subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService) = Mocks();

            var list = new List<ReleaseViewModel>
            {
                new ReleaseViewModel
                {
                    Id = Guid.NewGuid()
                }
            };
            
            userService
                .Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            
            userService
                .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(true);

            repository
                .Setup(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved))
                .ReturnsAsync(list);
            
            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object,
                subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object, footnoteService.Object);
            
            var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            Assert.Equal(list, result.Right);
            
            userService.Verify(s => s.MatchesPolicy(CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(CanViewAllReleases));
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved));
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyReleasesForReleaseStatusesAsync_CanViewRelatedReleases()
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository, 
                subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService) = Mocks();

            var list = new List<ReleaseViewModel>
            {
                new ReleaseViewModel
                {
                    Id = Guid.NewGuid()
                }
            };
            
            userService
                .Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            
            userService
                .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            
            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            repository
                .Setup(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(_userId, ReleaseStatus.Approved))
                .ReturnsAsync(list);
            
            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object,
                subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object, footnoteService.Object);
            
            var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            
            Assert.Equal(list, result.Right);
            
            userService.Verify(s => s.MatchesPolicy(CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(_userId, ReleaseStatus.Approved));
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyReleasesForReleaseStatusesAsync_NoAccessToSystem()
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository, 
                subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService) = Mocks();

            userService
                .Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(false);
            
            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object,
                subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object, footnoteService.Object);
            
            var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            
            Assert.IsAssignableFrom<ForbidResult>(result.Left);
            
            userService.Verify(s => s.MatchesPolicy(CanAccessSystem));
            userService.VerifyNoOtherCalls();

            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void DeleteDataFilesAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.DeleteDataFilesAsync(_release.Id, "", ""),  
                _release,
                CanUpdateSpecificRelease);
        }
        
        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<ReleaseService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity, params SecurityPolicies[] policies)
            where TEntity : class
        {
            var (userService, releaseHelper, publishingService, contentDbContext, repository, 
                subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService) = Mocks();

            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object,
                subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object, footnoteService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, userService, service, policies);
        }
        
        private (
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IPublishingService>,
            Mock<ContentDbContext>,
            Mock<IReleaseRepository>,
            Mock<ISubjectService>,
            Mock<ITableStorageService>,
            Mock<IFileStorageService>,
            Mock<IImportStatusService>,
            Mock<IFootnoteService>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(persistenceHelper, _release.Id, _release);
            MockUtils.SetupCall(persistenceHelper, Publication.Id, Publication);

            return (
                new Mock<IUserService>(), 
                persistenceHelper, 
                new Mock<IPublishingService>(), 
                new Mock<ContentDbContext>(), 
                new Mock<IReleaseRepository>(),
                new Mock<ISubjectService>(),
                new Mock<ITableStorageService>(),
                new Mock<IFileStorageService>(),
                new Mock<IImportStatusService>(),
                new Mock<IFootnoteService>());
        }
    }
}