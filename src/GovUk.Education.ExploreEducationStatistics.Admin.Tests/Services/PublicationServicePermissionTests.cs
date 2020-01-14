using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServicePermissionTests
    {
        [Fact]
        public async void GetMyPublicationsAndReleasesByTopicAsync_CanViewAllReleases()
        {
            var (context, userService, repository, persistenceHelper) = Mocks();

            var topicId = Guid.NewGuid();
            
            var publicationService = new PublicationService(context.Object, MapperForProfile<MappingProfiles>(), 
                userService.Object, repository.Object, persistenceHelper.Object);

            userService.
                Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).
                ReturnsAsync(true);

            var list = new List<PublicationViewModel>()
            {
                new PublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            repository.
                Setup(s => s.GetAllPublicationsForTopicAsync(topicId)).
                ReturnsAsync(list);
            
            var result = await publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetAllPublicationsForTopicAsync(topicId));
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyPublicationsAndReleasesByTopicAsync_CanViewRelatedReleases()
        {
            var (context, userService, repository, persistenceHelper) = Mocks();

            var topicId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var publicationService = new PublicationService(context.Object, MapperForProfile<MappingProfiles>(), 
                userService.Object, repository.Object, persistenceHelper.Object);

            userService.
                Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).
                ReturnsAsync(false);

            userService.
                Setup(s => s.GetUserId()).
                Returns(userId);

            var list = new List<PublicationViewModel>()
            {
                new PublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            repository.
                Setup(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId)).
                ReturnsAsync(list);
            
            var result = await publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId));
            repository.VerifyNoOtherCalls();
        }
        
        private (
            Mock<ContentDbContext>, 
            Mock<IUserService>, 
            Mock<IPublicationRepository>,
            Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            var userService = new Mock<IUserService>();

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases))
                .ReturnsAsync(true);

            return (new Mock<ContentDbContext>(), userService, new Mock<IPublicationRepository>(), 
                    MockUtils.MockPersistenceHelper<ContentDbContext, Publication>());
        }
    }
}