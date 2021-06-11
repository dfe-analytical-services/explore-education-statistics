using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var methodologyId = Guid.NewGuid();

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.Get("test-slug"))
                .ReturnsAsync(new MethodologyViewModel
                {
                    Id = methodologyId
                });

            var controller = new MethodologyController(methodologyService.Object);

            var result = await controller.Get("test-slug");
            var methodologyViewModel = result.Value;

            Assert.Equal(methodologyId, methodologyViewModel.Id);

            MockUtils.VerifyAllMocks(methodologyService);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.Get(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new MethodologyController(methodologyService.Object);

            var result = await controller.Get("unknown-slug");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(methodologyService);
        }
    }
}
