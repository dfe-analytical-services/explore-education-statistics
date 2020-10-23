using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class FileStorageServiceTests
    {
        private class TestModel
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task GetDeserialized()
        {
            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.DownloadBlobText(PublicContentContainerName, "test-path"))
                .ReturnsAsync(
                    @"
                {
                    ""Name"": ""Test"" 
                }");

            var fileStorageService = new FileStorageService(blobStorageService.Object);

            var result = await fileStorageService.GetDeserialized<TestModel>("test-path");

            Assert.True(result.IsRight);
            Assert.Equal("Test", result.Right.Name);
        }

        [Fact]
        public async Task GetDeserialized_ReleaseJson()
        {
            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.DownloadBlobText(PublicContentContainerName, "test-path"))
                .ReturnsAsync(SampleContentJson.ReleaseJson);

            var fileStorageService = new FileStorageService(blobStorageService.Object);

            var result = await fileStorageService.GetDeserialized<CachedReleaseViewModel>("test-path");

            Assert.True(result.IsRight);
            var releaseViewModel = result.Right;

            Assert.Single(releaseViewModel.Content);
            var contentSection = releaseViewModel.Content[0];
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
        }

        [Fact]
        public async Task GetDeserialized_PublicationJson()
        {
            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.DownloadBlobText(PublicContentContainerName, "test-path"))
                .ReturnsAsync(SampleContentJson.PublicationJson);

            var fileStorageService = new FileStorageService(blobStorageService.Object);

            var result = await fileStorageService.GetDeserialized<CachedPublicationViewModel>("test-path");

            Assert.True(result.IsRight);
            var viewModel = result.Right;

            Assert.Single(viewModel.Releases);
        }

        [Fact]
        public async Task GetDeserialized_MethodologyJson()
        {
            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.DownloadBlobText(PublicContentContainerName, "test-path"))
                .ReturnsAsync(SampleContentJson.MethodologyJson);

            var fileStorageService = new FileStorageService(blobStorageService.Object);

            var result = await fileStorageService.GetDeserialized<MethodologyViewModel>("test-path");

            Assert.True(result.IsRight);
        }
    }
}