using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class DownloadControllerTests
    {
        private DownloadController _controller;

        public DownloadControllerTests()
        {
            _controller = new DownloadController();
            _controller.ControllerContext = new ControllerContext();
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        }
        
        [Theory(Skip="File path not relative")]
        [InlineData("pupil-absence-in-schools-in-england")]
        [InlineData("permanent-and-fixed-period-exclusions")]
        [InlineData("schools-pupils-and-their-characteristics")]
        public void GetCsvBundle(string query)
        {
            var result = _controller.GetCsvBundle(query);
            
            var viewResult = Assert.IsType<ViewResult>(result);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("this-does-not-exist")]
        public void GetCsvBundle_NotFound(string query)
        {
            var result = _controller.GetCsvBundle(query);
            
            var viewResult = Assert.IsType<NotFoundResult>(result);
        }
    }
}