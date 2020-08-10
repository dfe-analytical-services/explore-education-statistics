using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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
            Id = Guid.NewGuid(),
            Title = "Test topic"
        };

        private readonly Publication _publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Test publication",
        };

        [Fact]
        public void GetMyPublicationsAndReleasesByTopicAsync_NoAccessOfSystem()
        {
            var mocks = Mocks();
            var userService = mocks.UserService;
            var publicationRepository = mocks.PublicationRepository;

            var topicId = Guid.NewGuid();

            var publicationService = BuildPublicationService(mocks);

            userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem)).ReturnsAsync(false);

            var list = new List<MyPublicationViewModel>()
            {
                new MyPublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            publicationRepository.Setup(s => s.GetAllPublicationsForTopicAsync(topicId)).ReturnsAsync(list);

            var result = publicationService.GetMyPublicationsAndReleasesByTopic(topicId).Result.Left;
            Assert.IsAssignableFrom<ForbidResult>(result);

            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem));
            userService.VerifyNoOtherCalls();

            publicationRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetMyPublicationsAndReleasesByTopicAsync_CanViewAllReleases()
        {
            var mocks = Mocks();
            var userService = mocks.UserService;
            var publicationRepository = mocks.PublicationRepository;

            var topicId = Guid.NewGuid();

            var publicationService = BuildPublicationService(mocks);

            userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem)).ReturnsAsync(true);

            userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).ReturnsAsync(true);

            var list = new List<MyPublicationViewModel>()
            {
                new MyPublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            publicationRepository.Setup(s => s.GetAllPublicationsForTopicAsync(topicId)).ReturnsAsync(list);

            var result = publicationService.GetMyPublicationsAndReleasesByTopic(topicId).Result.Right;
            Assert.Equal(list, result);

            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.VerifyNoOtherCalls();

            publicationRepository.Verify(s => s.GetAllPublicationsForTopicAsync(topicId));
            publicationRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetMyPublicationsAndReleasesByTopicAsync_CanViewRelatedReleases()
        {
            var mocks = Mocks();
            var userService = mocks.UserService;
            var publicationRepository = mocks.PublicationRepository;

            var topicId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var publicationService = BuildPublicationService(mocks);

            userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem)).ReturnsAsync(true);

            userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).ReturnsAsync(false);

            userService.Setup(s => s.GetUserId()).Returns(userId);

            var list = new List<MyPublicationViewModel>()
            {
                new MyPublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            publicationRepository.Setup(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId))
                .ReturnsAsync(list);

            var result = publicationService.GetMyPublicationsAndReleasesByTopic(topicId).Result.Right;
            Assert.Equal(list, result);

            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            publicationRepository.Verify(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId));
            publicationRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreatePublication()
        {
            await using var context = DbUtils.InMemoryApplicationDbContext();

            var mocks = Mocks();

            context.Add(_topic);

            await context.SaveChangesAsync();

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.CreatePublication(new SavePublicationViewModel
                    {
                        TopicId = _topic.Id,
                    }),
                _topic,
                mocks.UserService,
                BuildPublicationService(mocks),
                SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }

        [Fact]
        public async Task UpdatePublication_CanUpdatePublication()
        {
            await using var context = DbUtils.InMemoryApplicationDbContext();

            var mocks = Mocks();

            context.Add(_topic);
            context.Add(_publication);

            await context.SaveChangesAsync();

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new SavePublicationViewModel
                    {
                        TopicId = _topic.Id,
                        Title = "Updated publication",
                        Contact = new SaveContactViewModel
                        {
                            TeamName = "Test team",
                            TeamEmail = "team@test.com",
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789"
                        }
                    }),
                _publication,
                mocks.UserService,
                BuildPublicationService(mocks),
                SecurityPolicies.CanUpdatePublication);
        }

        [Fact]
        public async Task UpdatePublication_CanCreatePublicationForSpecificTopic()
        {
            await using var context = DbUtils.InMemoryApplicationDbContext();

            var mocks = Mocks();

            context.Add(_topic);
            context.Add(_publication);

            await context.SaveChangesAsync();

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new SavePublicationViewModel
                    {
                        TopicId = _topic.Id,
                        Title = "Updated publication",
                        Contact = new SaveContactViewModel
                        {
                            TeamName = "Test team",
                            TeamEmail = "team@test.com",
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789"
                        }
                    }),
                _topic,
                mocks.UserService,
                BuildPublicationService(mocks),
                SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }

        [Fact]
        public void GetViewModel()
        {
            var mocks = Mocks();

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service => await service.GetViewModel(_publication.Id),
                _publication,
                mocks.UserService,
                BuildPublicationService(mocks),
                SecurityPolicies.CanViewSpecificPublication);
        }

        private static PublicationService BuildPublicationService((Mock<ContentDbContext>,
            Mock<IMapper>,
            Mock<IUserService> userService,
            Mock<IPublicationRepository> publicationRepository,
            Mock<IPublishingService> publishingService,
            Mock<IPersistenceHelper<ContentDbContext>>) mocks)
        {
            var (context, mapper, userService, publicationRepository, publishingService, persistenceHelper) = mocks;

            return new PublicationService(
                context.Object,
                mapper.Object,
                userService.Object,
                publicationRepository.Object,
                publishingService.Object,
                persistenceHelper.Object);
        }

        private (
            Mock<ContentDbContext> ContentDbContext,
            Mock<IMapper> Mapper,
            Mock<IUserService> UserService,
            Mock<IPublicationRepository> PublicationRepository,
            Mock<IPublishingService> PublishingService,
            Mock<IPersistenceHelper<ContentDbContext>> PersistenceHelper) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(persistenceHelper, _topic.Id, _topic);
            MockUtils.SetupCall(persistenceHelper, _publication.Id, _publication);

            return (
                new Mock<ContentDbContext>(),
                new Mock<IMapper>(),
                MockUtils.AlwaysTrueUserService(),
                new Mock<IPublicationRepository>(),
                new Mock<IPublishingService>(),
                persistenceHelper);
        }
    }
}