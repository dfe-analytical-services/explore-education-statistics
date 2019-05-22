using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class DownloadControllerTests
    {
        private readonly DownloadController _controller;

        public DownloadControllerTests()
        {
            _controller = new DownloadController
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
        }

        [Theory(Skip = "File path not relative")]
        [InlineData("pupil-absence-in-schools-in-england")]
        [InlineData("permanent-and-fixed-period-exclusions")]
        [InlineData("schools-pupils-and-their-characteristics")]
        public void GetCsvBundle(string query)
        {
            var result = _controller.GetCsvBundle(query);
            Assert.IsType<IActionResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("this-does-not-exist")]
        public void GetCsvBundle_NotFound(string query)
        {
            var result = _controller.GetCsvBundle(query);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}