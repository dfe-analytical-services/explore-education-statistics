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
        public void Get_ContentTree_Returns_Ok()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetContentTreeAsync()).ReturnsAsync(
                JsonConvert.SerializeObject(new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            ));

            var controller = new ContentController(cache.Object);

            var result = controller.GetContentTree();
            var content = result.Result.Result as ContentResult;
            
            Assert.Equal(200, content.StatusCode);
            Assert.IsAssignableFrom<List<ThemeTree>>(JsonConvert.DeserializeObject<List<ThemeTree>>(content.Content));
            Assert.Contains("Theme A", content.Content);           
        }

        [Fact]
        public void Get_ContentTree_Returns_NoContent()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetContentTreeAsync()).ReturnsAsync(
                 (string)null
            );

            var controller = new ContentController(cache.Object);

            var result = controller.GetContentTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Publication_Returns_Ok()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetPublicationAsync("publication-a")).ReturnsAsync(
                JsonConvert.SerializeObject(new PublicationViewModel
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488"),
                    Title = "Publication A"
                }));

            var controller = new ContentController(cache.Object);

            var result = controller.GetPublication("publication-a");
            var content = result.Result.Result as ContentResult;

            Assert.IsAssignableFrom<PublicationViewModel>(JsonConvert.DeserializeObject<PublicationViewModel>(content.Content));
            Assert.Contains("a7772148-fbbd-4c85-8530-f33c9ef25488", content.Content);
            Assert.Contains("Publication A", content.Content);
            Assert.Equal(200, content.StatusCode);
        }

        [Fact]
        public void Get_Publication_Returns_NotFound()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetPublicationAsync("test-publication")).ReturnsAsync(
                (string) null);

            var controller = new ContentController(cache.Object);

            var result = controller.GetPublication("missing-publication");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }


         [Fact]
         public void Get_LatestRelease_Returns_Ok()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetLatestReleaseAsync("publication-a")).ReturnsAsync(
                 JsonConvert.SerializeObject(new ReleaseViewModel
                 {
                     Title = "Publication A",
                     Slug = "publication-a"
                 }));

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetLatestRelease("publication-a");

             Assert.Contains("Publication A", result.Result.Value);
         }

         [Fact]
         public void Get_LatestRelease_Returns_NotFound()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetLatestReleaseAsync("publication-a")).ReturnsAsync((string) null);

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetLatestRelease("publication-a");

             Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
         }

         [Fact]
         public void Get_Release_Returns_Ok()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetReleaseAsync("publication-a", "2016")).ReturnsAsync(
                 JsonConvert.SerializeObject(new ReleaseViewModel()
                 {
                     Slug = "publication-a"
                 }));

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetRelease("publication-a", "2016");

             Assert.Contains("publication-a", result.Result.Value);
         }

         [Fact]
         public void Get_Release_Returns_NotFound()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetReleaseAsync("publication-a", "2000")).ReturnsAsync((string) null);

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetRelease("publication-a", "2000");

             Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
         }
    }
}