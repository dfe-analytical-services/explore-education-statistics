using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class PreReleaseControllerTests
    {
        [Fact]
        public async void GetPreReleaseSummaryAsync_Returns_Ok()
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
            AssertOkResult(result);
        }

        private static (Mock<IPreReleaseContactsService> PreReleaseContactsService,
            Mock<IPreReleaseSummaryService> PreReleaseSummaryService) Mocks()
        {
            return (new Mock<IPreReleaseContactsService>(),
                new Mock<IPreReleaseSummaryService>());
        }

        private static T AssertOkResult<T>(ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<T>(result.Value);
            return result.Value;
        }
    }
}