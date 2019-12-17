using System;
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
    public class ContentControllerTests
    {
        [Fact]
        public void Get_PublicationsTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("publications/tree.json")).ReturnsAsync(
                JsonConvert.SerializeObject(new List<ThemeTree>
                    {
                        new ThemeTree
                        {
                            Title = "Theme A"
                        }
                    }
                ));

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetPublicationsTree();
            var content = result.Result.Result as ContentResult;

            Assert.IsAssignableFrom<List<ThemeTree>>(JsonConvert.DeserializeObject<List<ThemeTree>>(content.Content));
            Assert.Contains("Theme A", content.Content);
            Assert.Equal("application/json", content.ContentType);
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

            fileStorageService.Setup(s => s.DownloadTextAsync($"publications/publication-a/publication.json"))
                .ReturnsAsync(JsonConvert.SerializeObject(new PublicationViewModel
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488"),
                    Title = "Publication A"
                }));

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetPublication("publication-a");
            var content = result.Result.Result as ContentResult;

            Assert.IsAssignableFrom<PublicationViewModel>(
                JsonConvert.DeserializeObject<PublicationViewModel>(content.Content));
            Assert.Contains("a7772148-fbbd-4c85-8530-f33c9ef25488", content.Content);
            Assert.Contains("Publication A", content.Content);
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
                .ReturnsAsync(JsonConvert.SerializeObject(new ReleaseViewModel
                {
                    Title = "Publication A",
                    Slug = "publication-a"
                }));

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetLatestRelease("publication-a");
            var content = result.Result.Result as ContentResult;

            Assert.Contains("Publication A", content.Content);
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
                .ReturnsAsync(JsonConvert.SerializeObject(new ReleaseViewModel()
                {
                    Slug = "publication-a"
                }));

            var controller = new ContentController(fileStorageService.Object);

            var result = controller.GetRelease("publication-a", "2016");
            var content = result.Result.Result as ContentResult;

            Assert.Contains("publication-a", content.Content);
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