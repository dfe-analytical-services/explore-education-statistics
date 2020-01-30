using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class DownloadControllerTests
    {
        private const string Text = "result";

        [Fact]
        public void Get_DownloadTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("download/tree.json")).ReturnsAsync(Text);

            var controller = new DownloadController(fileStorageService.Object);

            var result = controller.GetDownloadTree();
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_DownloadTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            var controller = new DownloadController(fileStorageService.Object);

            var result = controller.GetDownloadTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }
    }
}