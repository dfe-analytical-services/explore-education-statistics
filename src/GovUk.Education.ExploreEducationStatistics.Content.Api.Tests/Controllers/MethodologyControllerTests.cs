using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public void Get_MethodologyTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("methodology/tree.json")).ReturnsAsync(
                JsonConvert.SerializeObject(new List<ThemeTree>
                    {
                        new ThemeTree
                        {
                            Title = "Theme A"
                        }
                    }
                ));

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.GetMethodologyTree();
            var content = result.Result.Result as ContentResult;

            Assert.IsAssignableFrom<List<ThemeTree>>(JsonConvert.DeserializeObject<List<ThemeTree>>(content.Content));
            Assert.Contains("Theme A", content.Content);
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

            fileStorageService.Setup(s => s.DownloadTextAsync("methodology/methodologies/test-slug.json")).ReturnsAsync(
                JsonConvert.SerializeObject(new Methodology
                    {
                        Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488")
                    }
                ));

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.Get("test-slug");
            var content = result.Result.Result as ContentResult;

            Assert.Contains("a7772148-fbbd-4c85-8530-f33c9ef25488", content.Content);
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