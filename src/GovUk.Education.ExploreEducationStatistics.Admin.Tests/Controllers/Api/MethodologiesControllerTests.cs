using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class MethodologiesControllerTests
    {
        [Fact]
        public async void Topic_Methodology_Returns_Ok()
        {
            var releaseService = new Mock<IMethodologyService>();

            releaseService.Setup(s => s.GetTopicMethodologiesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<MethodologyViewModel>());
            var controller = new MethodologiesController(releaseService.Object);

            // Method under test
            var result = await controller.GetTopicMethodologiesAsync(Guid.NewGuid());
            Assert.IsAssignableFrom<List<MethodologyViewModel>>(result.Value);
        }
        
        [Fact]
        public async void Methodologies_Returns_Ok()
        {
            var releaseService = new Mock<IMethodologyService>();

            releaseService.Setup(s => s.ListAsync())
                .ReturnsAsync(new List<MethodologyViewModel>());
            var controller = new MethodologiesController(releaseService.Object);

            // Method under test
            var result = await controller.GetMethodologiesAsync();
            Assert.IsAssignableFrom<List<MethodologyViewModel>>(result.Value);
        }
    }
}