using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class TopicControllerTests
    {
        [Fact]
        public async void CreateTopic_Returns_Ok()
        {
            var themeId = Guid.NewGuid();
            
            var request = new CreateTopicRequest
            {
                Title = "Test topic"
            };
            
            var viewModel = new TopicViewModel
            {
                Id = Guid.NewGuid()
            };

            var topicService = new Mock<ITopicService>();
            topicService.Setup(s => s.CreateTopic(themeId, request)).ReturnsAsync(viewModel);

            var controller = new TopicController(topicService.Object);

            var result = await controller.CreateTopic(request, themeId);
            Assert.Equal(viewModel, result.Value);
        }
    }
}