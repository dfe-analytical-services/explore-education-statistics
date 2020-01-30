using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        private const string Text = "result";
        
        [Fact]
        public void Get_MethodologyTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("methodology/tree.json")).ReturnsAsync(Text);

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.GetMethodologyTree();
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_MethodologyTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.GetMethodologyTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Methodology_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("methodology/methodologies/test-slug.json")).ReturnsAsync(Text);

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.Get("test-slug");
            var content = result.Result.Result as ContentResult;
            Assert.Contains(Text, content.Content);
            Assert.Equal(MediaTypeNames.Application.Json, content.ContentType);
        }

        [Fact]
        public void Get_Methodology_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.Get("unknown-slug");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}