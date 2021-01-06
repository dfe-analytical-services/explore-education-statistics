using System;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class TopicServicePermissionTests
    {
        private readonly Topic _topic = new Topic
        {
            Id = Guid.NewGuid(),
        };

        [Fact]
        public void CreateTopic()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupTopicService(userService: userService.Object);

                        return await service.CreateTopic(
                            new SaveTopicViewModel
                            {
                                Title = "Test title",
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateTopic()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupTopicService(userService: userService.Object);

                        return await service.UpdateTopic(
                            _topic.Id,
                            new SaveTopicViewModel
                            {
                                Title = "Test title",
                            }
                        );
                    }
                );
        }

        [Fact]
        public void GetTopic()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheck(_topic, SecurityPolicies.CanViewSpecificTopic, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupTopicService(userService: userService.Object);

                        return await service.GetTopic(_topic.Id);
                    }
                );
        }

        [Fact]
        public void DeleteTopic()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupTopicService(userService: userService.Object);

                        return await service.DeleteTopic(_topic.Id);
                    }
                );
        }

        private TopicService SetupTopicService(
            ContentDbContext contentContext = null,
            StatisticsDbContext statisticsContext = null,
            PersistenceHelper<ContentDbContext> persistenceHelper = null,
            IMapper mapper = null,
            IUserService userService = null,
            IReleaseSubjectService releaseSubjectService = null,
            IReleaseDataFileService releaseDataFileService = null,
            IReleaseFileService releaseFileService = null,
            IPublishingService publishingService = null)
        {
            return new TopicService(
                contentContext ?? new Mock<ContentDbContext>().Object,
                statisticsContext ?? new Mock<StatisticsDbContext>().Object,
                persistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext, Topic>(_topic.Id, _topic).Object,
                mapper ?? AdminMapper(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                releaseSubjectService ?? new Mock<IReleaseSubjectService>().Object,
                releaseDataFileService ?? new Mock<IReleaseDataFileService>().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object
            );
        }
    }
}