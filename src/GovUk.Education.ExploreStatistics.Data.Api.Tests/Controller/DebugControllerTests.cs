using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Debug;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class DebugControllerTests
    {
        private readonly DebugController _controller;

        public DebugControllerTests()
        {
            var geographicDataService = new Mock<IGeographicDataService>();
            var nationalCharacteristicDataService = new Mock<INationalCharacteristicDataService>();
            var laCharacteristicDataService = new Mock<ILaCharacteristicDataService>();

            geographicDataService.Setup(g => g.Count()).Returns(100);
            nationalCharacteristicDataService.Setup(g => g.Count()).Returns(200);
            laCharacteristicDataService.Setup(g => g.Count()).Returns(300);

            _controller = new DebugController(geographicDataService.Object, nationalCharacteristicDataService.Object,
                laCharacteristicDataService.Object);
        }

        [Fact]
        public void DebugReport()
        {
            var result = _controller.GetReport();

            Assert.IsType<ActionResult<DebugReport>>(result);

            Assert.Equal(100, result.Value.geographicCount);
            Assert.Equal(200, result.Value.nationalCharacteristicCount);
            Assert.Equal(300, result.Value.laCharacteristicCount);
        }
    }
}