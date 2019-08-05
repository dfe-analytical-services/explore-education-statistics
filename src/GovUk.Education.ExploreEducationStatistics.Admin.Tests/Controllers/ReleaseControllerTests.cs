using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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

            releaseService.Setup(s => s.CreateReleaseAsync(It.IsAny<CreateReleaseViewModel>()))
                .Returns(Task.FromResult(new ReleaseViewModel()));
            var controller = new ReleasesController(releaseService.Object);

            // Method under test
            var result = await controller.CreateReleaseAsync(new CreateReleaseViewModel(), Guid.NewGuid());
            Assert.IsAssignableFrom<ReleaseViewModel>(result.Value);
        }


        [Fact]
        public async void Edit_Release_Summary_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>();
            var releaseId = new Guid("95bf7743-fe6f-4b85-a28f-49f6f6b8735a");

            releaseService
                .Setup(s => s.EditReleaseSummaryAsync(It.IsAny<EditReleaseSummaryViewModel>()))
                .Returns<EditReleaseSummaryViewModel>(e => Task.FromResult(new ReleaseViewModel
                {
                    Id = e.Id
                }));
            var controller = new ReleasesController(releaseService.Object);

            // Method under test
            var result = await controller.EditReleaseSummaryAsync(new EditReleaseSummaryViewModel(), releaseId);
            Assert.IsAssignableFrom<ReleaseViewModel>(result.Value);
            Assert.Equal(result.Value.Id, releaseId);
        }


        [Fact]
        public async void Get_Release_Summary_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>();
            var releaseId = new Guid("95bf7743-fe6f-4b85-a28f-49f6f6b8735a");

            releaseService
                .Setup(s => s.GetReleaseSummaryAsync(It.IsAny<Guid>()))
                .Returns<Guid>(id => Task.FromResult(new EditReleaseSummaryViewModel{Id = id}));
            var controller = new ReleasesController(releaseService.Object);

            // Method under test
            var result = await controller.GetReleaseSummaryAsync(releaseId);
            Assert.IsAssignableFrom<EditReleaseSummaryViewModel>(result.Value);
            Assert.Equal(result.Value.Id, releaseId);
        }
        
        
        [Fact]
        public async void Get_Releases_For_Publication_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>();
            var releaseId = new Guid("fc570a6c-d230-40ae-a5e5-febab330fb12");
            releaseService
                .Setup(s => s.GetReleasesForPublicationAsync(It.Is<Guid>(id => id == releaseId)))
                .Returns<Guid>(x => Task.FromResult(new List<ReleaseViewModel>()));
            var controller = new ReleasesController(releaseService.Object);

            // Method under test
            var result = await controller.GetReleaseForPublicationAsync(releaseId);
            Assert.IsAssignableFrom<List<ReleaseViewModel>>(result.Value);
        }                                    
    }
}