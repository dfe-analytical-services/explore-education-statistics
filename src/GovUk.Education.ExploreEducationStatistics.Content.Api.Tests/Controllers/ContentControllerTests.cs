using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree()).Returns(
                new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            );
            publicationService.Setup(s => s.GetPublication("test"));
            releaseService.Setup(s => s.GetRelease("test"));

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetContentTree();

            Assert.IsAssignableFrom<List<ThemeTree>>(result.Value);
        }

        [Fact]
        public void Get_ContentTree_Returns_NoContent()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree()).Returns(new List<ThemeTree>());
            publicationService.Setup(s => s.GetPublication("test"));
            releaseService.Setup(s => s.GetRelease("test"));

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetContentTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result);
        }

        [Fact]
        public void Get_Publication_Returns_Ok()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree());
            publicationService.Setup(s => s.GetPublication("publication-a")).Returns(
                new PublicationViewModel
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488"),
                    Title = "Publication A"
                });
            releaseService.Setup(s => s.GetRelease("test"));

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetPublication("publication-a");

            Assert.IsAssignableFrom<PublicationViewModel>(result.Value);
            Assert.Equal("a7772148-fbbd-4c85-8530-f33c9ef25488", result.Value.Id.ToString());
            Assert.Equal("Publication A", result.Value.Title);
        }

        [Fact]
        public void Get_Publication_Returns_NotFound()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree());
            publicationService.Setup(s => s.GetPublication("test-publication")).Returns(
                new PublicationViewModel
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488"),
                    Title = "Publication A"
                });
            releaseService.Setup(s => s.GetRelease("test"));

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetPublication("missing-publication");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Get_LatestRelease_Returns_Ok()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree());
            publicationService.Setup(s => s.GetPublication("publication-a"));
            releaseService.Setup(s => s.GetLatestRelease("publication-a")).Returns(
                new ReleaseViewModel
                {
                    Title = "Publication A",
                    Slug = "publication-a"
                });

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetLatestRelease("publication-a");

            Assert.IsAssignableFrom<ReleaseViewModel>(result.Value);
            Assert.Equal("Publication A", result.Value.Title);
        }

        [Fact]
        public void Get_LatestRelease_Returns_NotFound()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            releaseService.Setup(s => s.GetLatestRelease("publication-a")).Returns((ReleaseViewModel) null);

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetLatestRelease("publication-a");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Get_Release_Returns_Ok()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            releaseService.Setup(s => s.GetRelease("publication-a")).Returns(
                new Release
                {
                    Title = "Publication A",
                    Slug = "publication-a"
                });

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetRelease("publication-a");

            Assert.IsAssignableFrom<Release>(result.Value);
            Assert.Equal("Publication A", result.Value.Title);
        }

        [Fact]
        public void Get_Release_Returns_NotFound()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            releaseService.Setup(s => s.GetRelease("publication-a")).Returns((Release) null);

            var controller =
                new ContentController(contentService.Object, publicationService.Object, releaseService.Object);

            var result = controller.GetRelease("publication-a");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}