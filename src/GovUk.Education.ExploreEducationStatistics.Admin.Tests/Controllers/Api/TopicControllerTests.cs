using System;
using System.Threading.Tasks;
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
        public async Task CreateTopic_Returns_Ok()
        {
            var request = new TopicSaveViewModel
            {
                Title = "Test topic",
                ThemeId =  Guid.NewGuid()
            };

            var viewModel = new TopicViewModel
            {
                Id = Guid.NewGuid()
            };

            var topicService = new Mock<ITopicService>();
            topicService.Setup(s => s.CreateTopic(request)).ReturnsAsync(viewModel);

            var controller = new TopicController(topicService.Object);

            var result = await controller.CreateTopic(request);
            Assert.Equal(viewModel, result.Value);
        }
    }
}
