using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ContentControllerTests
    {
        private const string Text = "result";

        [Fact]
        public void Get_PublicationsTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/tree.json")).ReturnsAsync(Text);

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetPublicationsTree();
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_PublicationsTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new ContentController(fileStorageService.Object);
            var result = controller.GetPublicationsTree();
            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Publication_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/publication.json")).ReturnsAsync(Text);

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetPublication("publication-a");
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_Publication_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new ContentController(fileStorageService.Object);
            var result = controller.GetPublication("missing-publication");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }


        [Fact]
        public void Get_LatestRelease_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/latest-release.json"))
                .ReturnsAsync(Text);

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetLatestRelease("publication-a");
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_LatestRelease_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new ContentController(fileStorageService.Object);
            var result = controller.GetLatestRelease("publication-a");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Release_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/releases/2016.json"))
                .ReturnsAsync(Text);

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetRelease("publication-a", "2016");
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_Release_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new ContentController(fileStorageService.Object);
            var result = controller.GetRelease("publication-a", "2000");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}