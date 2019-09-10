using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
                new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            );

            var controller = new ContentController(cache.Object);

            var result = controller.GetContentTree();

            Assert.IsAssignableFrom<List<ThemeTree>>(result.Result.Value);
        }

        [Fact]
        public void Get_ContentTree_Returns_NoContent()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetContentTreeAsync()).ReturnsAsync(
                new List<ThemeTree>()
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
                new PublicationViewModel
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488"),
                    Title = "Publication A"
                });

            var controller = new ContentController(cache.Object);

            var result = controller.GetPublication("publication-a");

            Assert.IsAssignableFrom<PublicationViewModel>(result.Result.Value);
            Assert.Equal("a7772148-fbbd-4c85-8530-f33c9ef25488", result.Result.Value.Id.ToString());
            Assert.Equal("Publication A", result.Result.Value.Title);
        }

        [Fact]
        public void Get_Publication_Returns_NotFound()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetPublicationAsync("test-publication")).ReturnsAsync(
                (PublicationViewModel) null);

            var controller = new ContentController(cache.Object);

            var result = controller.GetPublication("missing-publication");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }


         [Fact]
         public void Get_LatestRelease_Returns_Ok()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetLatestReleaseAsync("publication-a")).ReturnsAsync(
                 new ReleaseViewModel
                 {
                     Title = "Publication A",
                     Slug = "publication-a"
                 });

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetLatestRelease("publication-a");

             Assert.IsAssignableFrom<ReleaseViewModel>(result.Result.Value);
             Assert.Equal("Publication A", result.Result.Value.Title);
         }

         [Fact]
         public void Get_LatestRelease_Returns_NotFound()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetLatestReleaseAsync("publication-a")).ReturnsAsync((ReleaseViewModel) null);

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
                 new ReleaseViewModel()
                 {
                     Slug = "publication-a"
                 });

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetRelease("publication-a", "2016");

             Assert.IsAssignableFrom<ReleaseViewModel>(result.Result.Value);
             Assert.Equal("publication-a", result.Result.Value.Slug);
         }

         [Fact]
         public void Get_Release_Returns_NotFound()
         {
             var cache = new Mock<IContentCacheService>();

             cache.Setup(s => s.GetReleaseAsync("publication-a", "2000")).ReturnsAsync((ReleaseViewModel) null);

             var controller =
                 new ContentController(cache.Object);

             var result = controller.GetRelease("publication-a", "2000");

             Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
         }
    }
}