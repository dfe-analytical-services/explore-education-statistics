using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public void Get_Tree_Returns_Ok()
        {
            var service = new Mock<IMethodologyService>();

            service.Setup(s => s.GetTree()).Returns(
                new List<ThemeTree>()
                );


            var controller = new MethodologyController(service.Object);

            var result = controller.GetMethedologyTree();

            var actionResult = Assert.IsAssignableFrom<ActionResult>(result);
        }
    }
}
