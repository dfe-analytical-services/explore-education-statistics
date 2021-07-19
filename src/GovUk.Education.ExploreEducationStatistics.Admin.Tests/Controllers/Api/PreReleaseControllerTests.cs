using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class PreReleaseControllerTests
    {
        [Fact]
        public async Task GetPreReleaseSummaryAsync_Returns_Ok()
        {
            var (preReleaseContactsService, preReleaseSummaryService) = Mocks();

            var preReleaseSummaryViewModel = new PreReleaseSummaryViewModel();
            var releaseId = Guid.NewGuid();

            preReleaseSummaryService
                .Setup(s => s.GetPreReleaseSummaryViewModelAsync(It.Is<Guid>(id => id == releaseId)))
                .ReturnsAsync(preReleaseSummaryViewModel);

            var controller =
                new PreReleaseController(preReleaseContactsService.Object, preReleaseSummaryService.Object);

            var result = await controller.GetPreReleaseSummaryAsync(releaseId);
            result.AssertOkResult();
        }

        private static (Mock<IPreReleaseUserService> PreReleaseContactsService,
            Mock<IPreReleaseSummaryService> PreReleaseSummaryService) Mocks()
        {
            return (new Mock<IPreReleaseUserService>(),
                new Mock<IPreReleaseSummaryService>());
        }
    }
}
