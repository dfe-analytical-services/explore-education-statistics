using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
        public void GetReleasesForPublicationAsync_NoReleasesOnThisPublicationForThisUser()
        {
            var (userService, releaseHelper, publishingService, _, repository) = Mocks();

            var releaseOnAnotherPublication = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };
            
            var releaseRoleForDifferentPublication = new UserReleaseRole
            {
                UserId = _userId,
                Release = releaseOnAnotherPublication
            };
            
            var releaseRoleForDifferentUser = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                Release = releaseOnAnotherPublication
            };

            using (var context = DbUtils.InMemoryApplicationDbContext("Find"))
            {
                context.UserReleaseRoles.AddRange(
                    releaseRoleForDifferentPublication,
                    releaseRoleForDifferentUser);
                context.SaveChanges();

                userService
                    .Setup(s => s.GetUserId())
                    .Returns(_userId);
                    
                var service = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);

                var result = service.GetLatestReleaseAsync(_release.PublicationId);
                Assert.IsAssignableFrom<ForbidResult>(result.Result.Left);
            }
        }

        [Fact]
        public void GetLatestReleaseAsync_OnAReleaseForThisPublication()
        {
            var (userService, releaseHelper, publishingService, _, repository) = Mocks();

            var releaseOnThisPublication = new UserReleaseRole
            {
                UserId = _userId,
                Release = _release
            };

            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.UserReleaseRoles.Add(releaseOnThisPublication);
                context.SaveChanges();
                
                userService
                    .Setup(s => s.GetUserId())
                    .Returns(_userId);

                var service = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);

                var result = service.GetLatestReleaseAsync(_release.PublicationId).Result.Right;
                Assert.IsAssignableFrom<TitleAndIdViewModel>(result);
                Assert.Equal(_release.Id, result.Id);
                Assert.Equal(_release.Title, result.Title);
            }
        }

        [Fact]
        public void GetReleasesForPublicationAsync_HasCanViewAllReleasesClaim()
        {
            var (userService, releaseHelper, publishingService, _, repository) = Mocks();

            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.Add(_release);
                context.SaveChanges();
                
                userService
                    .Setup(s => s.GetUserId())
                    .Returns(_userId);

                userService
                    .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                    .ReturnsAsync(true);

                var service = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);

                var result = service.GetLatestReleaseAsync(_release.PublicationId).Result.Right;
                Assert.IsAssignableFrom<TitleAndIdViewModel>(result);
                Assert.Equal(_release.Id, result.Id);
                Assert.Equal(_release.Title, result.Title);
            }
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
                .Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            
            userService
                .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(true);

            repository
                .Setup(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved))
                .ReturnsAsync(list);
            
            var service = new ReleaseService(contentDbContext.Object, AdminMapper(), 
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
            
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
            var (userService, releaseHelper, publishingService, contentDbContext, repository) = Mocks();

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
                publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
            
            var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
            
            Assert.Equal(list, result.Right);
            
            userService.Verify(s => s.MatchesPolicy(CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(_userId, ReleaseStatus.Approved));
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
            MockUtils.SetupCall(persistenceHelper, Publication.Id, Publication);

            return (
                new Mock<IUserService>(), 
                persistenceHelper, 
                new Mock<IPublishingService>(), 
                new Mock<ContentDbContext>(), 
                new Mock<IReleaseRepository>());
        }
    }
}