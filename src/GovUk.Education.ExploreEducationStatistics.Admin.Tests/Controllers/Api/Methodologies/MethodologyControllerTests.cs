using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Methodologies
{
    public class MethodologyControllerTests
    {
        private readonly Guid _methodologyId = Guid.NewGuid();

        [Fact]
        public async void GetMethodologiesAsync_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.ListAsync())
                .ReturnsAsync(
                    new Either<ActionResult, List<MethodologyStatusViewModel>>(new List<MethodologyStatusViewModel>()));
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologiesAsync();
            AssertOkResult(result);
        }

        [Fact]
        public async void GetMethodologyStatusAsync_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetStatusAsync(_methodologyId))
                .ReturnsAsync(new MethodologyStatusViewModel());
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologyStatusAsync(_methodologyId);
            AssertOkResult(result);
        }

        [Fact]
        public async void GetMethodologyStatusAsync_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetStatusAsync(It.Is<Guid>(guid => guid != _methodologyId)))
                .ReturnsAsync(new NotFoundResult());
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologyStatusAsync(Guid.NewGuid());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetMethodologySummaryAsync_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetSummaryAsync(_methodologyId))
                .ReturnsAsync(new MethodologySummaryViewModel());
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologySummaryAsync(_methodologyId);
            AssertOkResult(result);
        }

        [Fact]
        public async void GetMethodologySummaryAsync_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetSummaryAsync(It.Is<Guid>(guid => guid != _methodologyId)))
                .ReturnsAsync(new NotFoundResult());
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologySummaryAsync(Guid.NewGuid());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        private static T AssertOkResult<T>(ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<T>(result.Value);
            return result.Value;
        }
    }
}