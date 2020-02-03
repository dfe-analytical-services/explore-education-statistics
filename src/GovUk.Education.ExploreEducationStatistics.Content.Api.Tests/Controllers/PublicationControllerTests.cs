using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        private const string Text = "result";

        [Fact]
        public void Get_PublicationTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/tree.json")).ReturnsAsync(Text);

            var controller = new PublicationController(fileStorageService.Object);

            var result = controller.GetPublicationTree();
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_PublicationTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new PublicationController(fileStorageService.Object);
            var result = controller.GetPublicationTree();
            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Publication_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/publication.json"))
                .ReturnsAsync(Text);

            var controller = new PublicationController(fileStorageService.Object);

            var result = controller.GetPublication("publication-a");
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_Publication_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new PublicationController(fileStorageService.Object);
            var result = controller.GetPublication("missing-publication");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}