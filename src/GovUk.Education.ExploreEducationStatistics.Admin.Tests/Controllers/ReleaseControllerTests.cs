using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async void Create_Release_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>();

            releaseService.Setup(s => s.CreateReleaseAsync(It.IsAny<CreateReleaseViewModel>())).Returns(Task.FromResult(new ReleaseViewModel()));
            var controller = new ReleasesController(releaseService.Object);
            
            // Method under test
            var result = await controller.CreateReleaseAsync(new CreateReleaseViewModel(), Guid.NewGuid());
            Assert.IsAssignableFrom<ReleaseViewModel>(result.Value);
        }
    }
}