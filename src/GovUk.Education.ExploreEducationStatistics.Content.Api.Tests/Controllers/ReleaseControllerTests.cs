#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ReleaseControllerTests
    {
        [Fact]
        public async Task GetLatestRelease()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.Get(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/latest-release.json"))
                .ReturnsAsync(BuildReleaseViewModel());

            var controller = BuildReleaseController(releaseService.Object);

            var result = await controller.GetLatestRelease("publication-a");
            var releaseViewModel = result.Value;

            Assert.IsType<ReleaseViewModel>(releaseViewModel);

            MockUtils.VerifyAllMocks(releaseService);
        }

        [Fact]
        public async Task GetLatestRelease_NotFound()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.Get(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/latest-release.json"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseService.Object);
            var result = await controller.GetLatestRelease("publication-a");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(releaseService);
        }

        [Fact]
        public async Task GetRelease()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.Get(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/releases/2000.json"))
                .ReturnsAsync(BuildReleaseViewModel());

            var controller = BuildReleaseController(releaseService.Object);

            var result = await controller.GetRelease("publication-a", "2000");
            var releaseViewModel = result.Value;

            Assert.IsType<ReleaseViewModel>(releaseViewModel);

            MockUtils.VerifyAllMocks(releaseService);
        }


        [Fact]
        public async Task GetRelease_NotFound()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.Get(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/releases/2000.json"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseService.Object);
            var result = await controller.GetRelease("publication-a", "2000");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(releaseService);
        }

        [Fact]
        public async Task GetLatestReleaseSummary()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.GetSummary(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/latest-release.json"))
                .ReturnsAsync(BuildReleaseSummaryViewModel());

            var controller = BuildReleaseController(releaseService.Object);

            var result = await controller.GetLatestReleaseSummary("publication-a");
            var releaseViewModel = result.Value;

            Assert.IsType<ReleaseSummaryViewModel>(releaseViewModel);

            MockUtils.VerifyAllMocks(releaseService);
        }

        [Fact]
        public async Task GetLatestReleaseSummary_NotFound()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.GetSummary(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/latest-release.json"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseService.Object);

            var result = await controller.GetLatestReleaseSummary("publication-a");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(releaseService);
        }

        [Fact]
        public async Task GetReleaseSummary()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.GetSummary(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/releases/2000.json"))
                .ReturnsAsync(BuildReleaseSummaryViewModel());

            var controller = BuildReleaseController(releaseService.Object);

            var result = await controller.GetReleaseSummary("publication-a", "2000");
            var releaseViewModel = result.Value;

            Assert.IsType<ReleaseSummaryViewModel>(releaseViewModel);

            MockUtils.VerifyAllMocks(releaseService);
        }

        [Fact]
        public async Task GetReleaseSummary_NotFound()
        {
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService.Setup(mock => mock.GetSummary(
                    "publications/publication-a/publication.json",
                    "publications/publication-a/releases/2000.json"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildReleaseController(releaseService.Object);

            var result = await controller.GetReleaseSummary("publication-a", "2000");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(releaseService);
        }

        private static ReleaseViewModel BuildReleaseViewModel()
        {
            var releaseId = Guid.NewGuid();

            return new ReleaseViewModel(
                new CachedReleaseViewModel(releaseId)
                {
                    Type = new ReleaseTypeViewModel
                    {
                        Title = "National Statistics"
                    }
                },
                new CachedPublicationViewModel
                {
                    Releases = AsList(new ReleaseTitleViewModel
                    {
                        Id = releaseId
                    })
                });
        }

        private static ReleaseSummaryViewModel BuildReleaseSummaryViewModel()
        {
            var releaseId = Guid.NewGuid();

            return new ReleaseSummaryViewModel(
                new CachedReleaseViewModel(releaseId)
                {
                    Type = new ReleaseTypeViewModel
                    {
                        Title = "National Statistics"
                    }
                },
                new CachedPublicationViewModel
                {
                    Releases = AsList(new ReleaseTitleViewModel
                    {
                        Id = releaseId
                    })
                });
        }

        private static ReleaseController BuildReleaseController(
            IReleaseService? releaseService = null
        )
        {
            return new(
                releaseService ?? Mock.Of<IReleaseService>()
            );
        }
    }
}
