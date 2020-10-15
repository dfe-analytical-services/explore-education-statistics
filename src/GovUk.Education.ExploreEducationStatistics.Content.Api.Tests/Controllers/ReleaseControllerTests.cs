using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.SampleContentJson;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ReleaseControllerTests
    {
        [Fact]
        public void Get_LatestRelease_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(PublicationJson);
            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "publications/publication-a/latest-release.json"
                    )
                )
                .ReturnsAsync(ReleaseJson);

            var controller = new ReleaseController(fileStorageService.Object);

            var result = controller.GetLatestRelease("publication-a");
            var releaseViewModel = result.Result.Value;
            Assert.True(releaseViewModel.LatestRelease);

            Assert.Single(releaseViewModel.Content);
            var contentSection = releaseViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsAssignableFrom<HtmlBlockViewModel>(contentSection.Content.First());

            var headlinesSection = releaseViewModel.HeadlinesSection;
            Assert.Single(headlinesSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(headlinesSection.Content.First());

            var keyStatisticsSection = releaseViewModel.KeyStatisticsSection;
            Assert.Single(keyStatisticsSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSection.Content.First());

            var keyStatisticsSecondarySection = releaseViewModel.KeyStatisticsSecondarySection;
            Assert.Single(keyStatisticsSecondarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSecondarySection.Content.First());

            var summarySection = releaseViewModel.SummarySection;
            Assert.Single(summarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(summarySection.Content.First());

            var publication = releaseViewModel.Publication;
            Assert.Equal(new Guid("4fd09502-15bb-4d2b-abd1-7fd112aeee14"), publication.Id);
        }

        [Fact]
        public void Get_LatestRelease_Returns_NotFound()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            var controller = new ReleaseController(fileStorageService.Object);
            var result = controller.GetLatestRelease("publication-a");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Release_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(PublicationJson);
            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "publications/publication-a/releases/2016.json"
                    )
                )
                .ReturnsAsync(ReleaseJson);

            var controller = new ReleaseController(fileStorageService.Object);

            var result = controller.GetRelease("publication-a", "2016");
            var releaseViewModel = result.Result.Value;
            Assert.True(releaseViewModel.LatestRelease);

            Assert.Single(releaseViewModel.Content);
            var contentSection = releaseViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsAssignableFrom<HtmlBlockViewModel>(contentSection.Content.First());

            var headlinesSection = releaseViewModel.HeadlinesSection;
            Assert.Single(headlinesSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(headlinesSection.Content.First());

            var keyStatisticsSection = releaseViewModel.KeyStatisticsSection;
            Assert.Single(keyStatisticsSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSection.Content.First());

            var keyStatisticsSecondarySection = releaseViewModel.KeyStatisticsSecondarySection;
            Assert.Single(keyStatisticsSecondarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSecondarySection.Content.First());

            var summarySection = releaseViewModel.SummarySection;
            Assert.Single(summarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(summarySection.Content.First());

            var publication = releaseViewModel.Publication;
            Assert.Equal(new Guid("4fd09502-15bb-4d2b-abd1-7fd112aeee14"), publication.Id);
        }

        [Fact]
        public void Get_Release_Returns_NotFound()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            var controller = new ReleaseController(fileStorageService.Object);
            var result = controller.GetRelease("publication-a", "2000");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}