using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
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
        }
        
//        [Fact]
//        public void GetCsvBundle()
//        {
//            var result = _controller.GetCsvBundle("pupil-absence-in-schools-in-england");
//            
//            var viewResult = Assert.IsType<ViewResult>(result);
//        }
        
        [Fact]
        public void GetCsvBundle_NotFound()
        {
            var result = _controller.GetCsvBundle("");
            
            var viewResult = Assert.IsType<NotFoundResult>(result);
        }
    }
}