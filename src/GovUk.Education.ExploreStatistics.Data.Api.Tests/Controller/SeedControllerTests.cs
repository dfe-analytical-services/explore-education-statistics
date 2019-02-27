using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class SeedControllerTests
    {
        private SeedController _controller;

        public SeedControllerTests()
        {
            var seedService =  new Mock<ISeedService>();

            seedService.Setup(s => s.DeleteAll());
            
            _controller = new SeedController(seedService.Object);   
        }
        
        [Fact]
        public void DeleteAll ()
        {
            var result = _controller.DeleteAll();
            
            Assert.Equal("Deleted all rows", result);
        }
    }
}