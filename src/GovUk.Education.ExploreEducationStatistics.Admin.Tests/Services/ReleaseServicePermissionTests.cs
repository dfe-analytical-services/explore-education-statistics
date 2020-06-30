using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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
        public void UpdateRelease()
        {
            AssertSecurityPoliciesChecked(
                service => service.UpdateRelease(_release.Id, new UpdateReleaseRequest()), 
                _release, 
                CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateRelease_Draft()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateRelease(_release.Id, new UpdateReleaseRequest() {
                    Status = ReleaseStatus.Draft
                }),  
                _release,
                CanUpdateSpecificRelease,
                CanMarkSpecificReleaseAsDraft);
        }
        
        [Fact]
        public void UpdateRelease_SubmitForHigherLevelReview()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateRelease(_release.Id, new UpdateReleaseRequest() {
                    Status = ReleaseStatus.HigherLevelReview
                }),  
                _release,
                CanUpdateSpecificRelease,
                CanSubmitSpecificReleaseToHigherReview);
        }
        
        [Fact]
        public void UpdateRelease_Approve()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateRelease(_release.Id, new UpdateReleaseRequest() {
                    Status = ReleaseStatus.Approved
                }),  
                _release,
                CanUpdateSpecificRelease,
                CanApproveSpecificRelease);
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
        public void CreateReleaseAmendmentAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.CreateReleaseAmendmentAsync(_release.Id),  
                _release,
                CanMakeAmendmentOfSpecificRelease);
        }

        [Fact]
        public void PublishReleaseAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.PublishReleaseAsync(_release.Id),
                _release,
                CanPublishSpecificRelease);
        }

        [Fact]
        public void PublishReleaseContentAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.PublishReleaseContentAsync(_release.Id),
                _release,
                CanPublishSpecificRelease);
        }

        [Fact]
        public void DeleteReleaseAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.DeleteReleaseAsync(_release.Id),  
                _release,
                CanDeleteSpecificRelease);
        }

        [Fact]
        public async void GetMyReleasesForReleaseStatusesAsync_CanViewAllReleases()
        {
            var mocks = Mocks();
            var repository = mocks.ReleaseRepository;
            var userService = mocks.UserService;

            var list = new List<MyReleaseViewModel>
            {
                new MyReleaseViewModel
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

            var service = BuildReleaseService(mocks);

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
            var mocks = Mocks();
            var repository = mocks.ReleaseRepository;
            var userService = mocks.UserService;

            var list = new List<MyReleaseViewModel>
            {
                new MyReleaseViewModel
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

            var service = BuildReleaseService(mocks);
            
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
            var mocks = Mocks();
            var repository = mocks.ReleaseRepository;
            var userService = mocks.UserService;

            mocks.UserService
                .Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(false);

            var service = BuildReleaseService(mocks);
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
                    service.RemoveDataFileReleaseLinkAsync(_release.Id, "", ""),  
                _release,
                CanUpdateSpecificRelease);
        }

        private static ReleaseService BuildReleaseService((Mock<IUserService> userService,
            Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper,
            Mock<IPublishingService> publishingService,
            Mock<ContentDbContext> contentDbContext,
            Mock<IReleaseRepository> releaseRepository,
            Mock<ISubjectService> subjectService,
            Mock<ITableStorageService> tableStorageService,
            Mock<IFileStorageService> fileStorageService,
            Mock<IImportStatusService> importStatusService,
            Mock<IFootnoteService> footnoteService,
            Mock<StatisticsDbContext> statisticsDbContext,
            Mock<IDataBlockService> dataBlockService,
            Mock<IReleaseSubjectService> releaseSubjectService) mocks)
        {
            var (userService, persistenceHelper, publishingService, contentDbContext, releaseRepository, subjectService,
                tableStorageService, fileStorageService, importStatusService, footnoteService, statisticsDbContext,
                dataBlockService, releaseSubjectService) = mocks;

            return new ReleaseService(
                contentDbContext.Object, AdminMapper(), publishingService.Object, persistenceHelper.Object,
                userService.Object, releaseRepository.Object, subjectService.Object, tableStorageService.Object,
                fileStorageService.Object, importStatusService.Object, footnoteService.Object,
                statisticsDbContext.Object, dataBlockService.Object, releaseSubjectService.Object, new SequentialGuidGenerator());
        }

        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<ReleaseService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity, params SecurityPolicies[] policies)
            where TEntity : class
        {
            var mocks = Mocks();
            var service = BuildReleaseService(mocks);
            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, mocks.UserService, service, policies);
        }

        private (Mock<IUserService> UserService,
            Mock<IPersistenceHelper<ContentDbContext>> PersistenceHelper,
            Mock<IPublishingService> PublishingService,
            Mock<ContentDbContext> ContentDbContext,
            Mock<IReleaseRepository> ReleaseRepository,
            Mock<ISubjectService> SubjectService,
            Mock<ITableStorageService> TableStorageService,
            Mock<IFileStorageService> FileStorageService,
            Mock<IImportStatusService> ImportStatusService,
            Mock<IFootnoteService> FootnoteService,
            Mock<StatisticsDbContext> StatisticsDbContext,
            Mock<IDataBlockService> DataBlockService,
            Mock<IReleaseSubjectService> ReleaseSubjectService) Mocks()
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
                new Mock<IFootnoteService>(),
                new Mock<StatisticsDbContext>(),
                new Mock<IDataBlockService>(),
                new Mock<IReleaseSubjectService>());
        }
    }
}