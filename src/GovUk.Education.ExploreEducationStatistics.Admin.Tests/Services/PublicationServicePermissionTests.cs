using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServicePermissionTests
    {
        private readonly Topic _topic = new Topic
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public async void GetMyPublicationsAndReleasesByTopicAsync_CanViewAllReleases()
        {
            var (context, mapper, userService, publicationRepository, persistenceHelper) = Mocks();

            var topicId = Guid.NewGuid();
            
            var publicationService = new PublicationService(context.Object, mapper.Object, 
                userService.Object, publicationRepository.Object, persistenceHelper.Object);

            userService.
                Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).
                ReturnsAsync(true);

            var list = new List<MyPublicationViewModel>()
            {
                new MyPublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            publicationRepository.
                Setup(s => s.GetAllPublicationsForTopicAsync(topicId)).
                ReturnsAsync(list);
            
            var result = await publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.VerifyNoOtherCalls();

            publicationRepository.Verify(s => s.GetAllPublicationsForTopicAsync(topicId));
            publicationRepository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyPublicationsAndReleasesByTopicAsync_CanViewRelatedReleases()
        {
            var (context, mapper, userService, publicationRepository, persistenceHelper) = Mocks();

            var topicId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var publicationService = new PublicationService(context.Object, mapper.Object, 
                userService.Object, publicationRepository.Object, persistenceHelper.Object);

            userService.
                Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).
                ReturnsAsync(false);

            userService.
                Setup(s => s.GetUserId()).
                Returns(userId);

            var list = new List<MyPublicationViewModel>()
            {
                new MyPublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            publicationRepository.
                Setup(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId)).
                ReturnsAsync(list);
            
            var result = await publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            publicationRepository.Verify(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId));
            publicationRepository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void CreatePublicationAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                service.CreatePublicationAsync(new CreatePublicationViewModel
                {
                    TopicId = _topic.Id
                }), 
                _topic,
                SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }
        
        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<PublicationService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity, params SecurityPolicies[] policies)
            where TEntity : class
        {
            var (contentDbContext, mapper, userService, publicationRepository, persistenceHelper) = Mocks();

            var service = new PublicationService(contentDbContext.Object, mapper.Object, 
                userService.Object, publicationRepository.Object, persistenceHelper.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, userService, service, policies);
        }
        
        private (
            Mock<ContentDbContext>, 
            Mock<IMapper>, 
            Mock<IUserService>, 
            Mock<IPublicationRepository>, 
            Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(persistenceHelper, _topic.Id, _topic);

            return (
                new Mock<ContentDbContext>(),
                new Mock<IMapper>(), 
                MockUtils.AlwaysTrueUserService(), 
                new Mock<IPublicationRepository>(), 
                persistenceHelper);
        }
    }
}