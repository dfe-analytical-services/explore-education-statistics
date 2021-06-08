using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public async Task Get()
        {
            // TODO SOW4 EES-2375 Update tests after change to return methodology from content database

            var controller = new MethodologyController();

            var result = await controller.Get("test-slug");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var controller = new MethodologyController();

            var result = await controller.Get("unknown-slug");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
