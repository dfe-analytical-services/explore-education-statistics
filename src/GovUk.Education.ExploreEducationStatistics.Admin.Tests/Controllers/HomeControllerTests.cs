using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Tools.Controllers;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsAViewResult_ForTheHomePage()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }
    }
}