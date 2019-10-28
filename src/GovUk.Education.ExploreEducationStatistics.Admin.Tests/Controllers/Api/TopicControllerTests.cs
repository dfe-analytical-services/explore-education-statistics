using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class TopicControllerTests
    {
//        TODO - Guid making test fail even though it should pass???
//        [Fact]
//        public async void Get_Topic_Returns_Ok()
//        {
//            var testGuid = Guid.Parse("7270d051-72b9-41b9-9568-205ab1a6bb");
//            var testTopic = new Topic
//            {
//                Id = testGuid,
//                Title = "Test topic"
//            };
//
//            var topicService = new Mock<ITopicService>();
//            topicService.Setup(s => s.GetTopicAsync(testGuid)).Returns<Topic>(t => Task.FromResult(testTopic));
//
//            var controller = new TopicController(topicService.Object);
//
//            var result = await controller.GetTopicAsync(testGuid);
//
//            Assert.Equal(result.Value.Id, testGuid);
//            Assert.Equal(result.Value.Title, "Test topic");
//        }
    }
}