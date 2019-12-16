using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class DownloadControllerTests
    {
        [Fact]
        public void Get_DownloadTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("download/tree.json")).ReturnsAsync(
                JsonConvert.SerializeObject(new List<ThemeTree>
                    {
                        new ThemeTree
                        {
                            Title = "Theme A"
                        },
                        new ThemeTree
                        {
                            Title = "Theme B"
                        },
                    }
                ));

            var controller = new DownloadController(fileStorageService.Object);

            var result = controller.GetDownloadTree();

            var content = result.Result.Result as ContentResult;

            Assert.IsAssignableFrom<List<ThemeTree>>(JsonConvert.DeserializeObject<List<ThemeTree>>(content.Content));
            Assert.Contains("Theme A", content.Content);
            Assert.Contains("Theme B", content.Content);
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