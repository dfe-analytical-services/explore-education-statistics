using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ReleaseControllerTests
    {
        [Fact]
        public async Task GetLatestRelease()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService
                .Setup(
                    s => s.GetDeserialized<CachedPublicationViewModel>(
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(new CachedPublicationViewModel
                {
                    Id = publicationId,
                    LatestReleaseId = releaseId,
                    Releases = new List<ReleaseTitleViewModel>
                    {
                        new ReleaseTitleViewModel
                        {
                            Id = releaseId
                        }
                    }
                });

            fileStorageService
                .Setup(
                    s => s.GetDeserialized<CachedReleaseViewModel>(
                        "publications/publication-a/latest-release.json"
                    )
                )
                .ReturnsAsync(new CachedReleaseViewModel
                {
                    Id = releaseId,
                    Content = new List<ContentSectionViewModel>
                    {
                        new ContentSectionViewModel
                        {
                            Content = new List<IContentBlockViewModel>
                            {
                                new HtmlBlockViewModel()
                            }
                        }
                    }
                });

            var controller = new ReleaseController(fileStorageService.Object);

            var result = await controller.GetLatestRelease("publication-a");
            var releaseViewModel = result.Value;

            Assert.IsType<ReleaseViewModel>(releaseViewModel);
            Assert.True(releaseViewModel.LatestRelease);

            Assert.Single(releaseViewModel.Content);

            var contentSection = releaseViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsType<HtmlBlockViewModel>(contentSection.Content.First());

            var publication = releaseViewModel.Publication;
            Assert.Equal(publicationId, publication.Id);
        }

        [Fact]
        public async Task GetLatestRelease_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedReleaseViewModel>(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new ReleaseController(fileStorageService.Object);
            var result = await controller.GetLatestRelease("publication-a");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetRelease()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(
                    s => s.GetDeserialized<CachedPublicationViewModel>(
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(new CachedPublicationViewModel
                {
                    Id = publicationId,
                    LatestReleaseId = releaseId,
                    Releases = new List<ReleaseTitleViewModel>
                    {
                        new ReleaseTitleViewModel
                        {
                            Id = releaseId
                        }
                    }
                });

            fileStorageService.Setup(
                    s => s.GetDeserialized<CachedReleaseViewModel>(
                        "publications/publication-a/releases/2016.json"
                    )
                )
                .ReturnsAsync(new CachedReleaseViewModel
                {
                    Id = releaseId,
                    Content = new List<ContentSectionViewModel>
                    {
                        new ContentSectionViewModel
                        {
                            Content = new List<IContentBlockViewModel>
                            {
                                new HtmlBlockViewModel()
                            }
                        }
                    }
                });

            var controller = new ReleaseController(fileStorageService.Object);

            var result = await controller.GetRelease("publication-a", "2016");
            var releaseViewModel = result.Value;

            Assert.IsType<ReleaseViewModel>(releaseViewModel);
            Assert.True(releaseViewModel.LatestRelease);

            Assert.Single(releaseViewModel.Content);

            var contentSection = releaseViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsType<HtmlBlockViewModel>(contentSection.Content.First());

            var publication = releaseViewModel.Publication;
            Assert.Equal(publicationId, publication.Id);
        }

        [Fact]
        public async Task GetRelease_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedReleaseViewModel>(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new ReleaseController(fileStorageService.Object);
            var result = await controller.GetRelease("publication-a", "2000");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}