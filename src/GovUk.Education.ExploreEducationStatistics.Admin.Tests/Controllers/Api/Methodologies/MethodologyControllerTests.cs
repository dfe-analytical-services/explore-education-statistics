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
        [Fact]
        public async void Topic_Methodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetTopicMethodologiesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Either<ActionResult, List<MethodologyViewModel>>(new List<MethodologyViewModel>()));
            
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetTopicMethodologiesAsync(Guid.NewGuid());
            Assert.IsAssignableFrom<List<MethodologyViewModel>>(result.Value);
        }
        
        [Fact]
        public async void Methodologies_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.ListAsync())
                .ReturnsAsync(new Either<ActionResult, List<MethodologyViewModel>>(new List<MethodologyViewModel>()));
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologiesAsync();
            Assert.IsAssignableFrom<List<MethodologyViewModel>>(result.Value);
        }
    }
}