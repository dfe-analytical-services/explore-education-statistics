using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
{
    public class ReleaseControllerTests
    {
        
        [Fact]
        public void Get_UserTheme_Returns_Ok()
        {
            var releaseService = new Mock<ReleaseService>();

            releaseService.Setup(s => s.CreateRelease(It.IsAny<CreateReleaseViewModel>())).Returns(new ReleaseViewModel());
            var controller = new ReleasesController(releaseService.Object);
            
            // Method under test
            var result = controller.CreateRelease(new CreateReleaseViewModel(), Guid.NewGuid());
            Assert.IsAssignableFrom<ReleaseViewModel>(result.Value);
        }
    }
}