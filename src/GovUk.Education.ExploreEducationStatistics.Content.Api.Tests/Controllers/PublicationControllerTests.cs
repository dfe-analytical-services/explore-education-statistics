using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        [Fact]
        public async Task GetPublicationTitle()
        {
            var publicationId = Guid.NewGuid();

            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService
                .Setup(s =>
                    s.GetDeserialized<PublicationTitleViewModel>("publications/publication-a/publication.json")
                )
                .ReturnsAsync(new PublicationTitleViewModel
                    {
                        Id = publicationId,
                        Title = "Test title"
                    });

            var controller = new PublicationController(fileStorageService.Object);

            var publicationTitleViewModel = (await controller.GetPublicationTitle("publication-a")).Value;

            Assert.IsType<PublicationTitleViewModel>(publicationTitleViewModel);

            Assert.Equal(publicationId, publicationTitleViewModel.Id);
            Assert.Equal("Test title", publicationTitleViewModel.Title);
        }

        [Fact]
        public async Task GetPublicationTitle_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService
                .Setup(s => s.GetDeserialized<PublicationTitleViewModel>(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new PublicationController(fileStorageService.Object);
            var result = await controller.GetPublicationTitle("missing-publication");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPublicationMethodology()
        {
            var methodologyId = Guid.NewGuid();

            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(
                    s => s.GetDeserialized<PublicationMethodologyViewModel>(
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(new PublicationMethodologyViewModel
                {
                    Methodology = new MethodologySummaryViewModel
                    {
                        Id = methodologyId,
                        Slug = "methodologySlug",
                        Summary = "methodologySummary",
                        Title = "methodologyTitle"
                    },
                    ExternalMethodology = new ExternalMethodologyViewModel
                    {
                        Title = "externalMethodologyTitle",
                        Url = "externalMethodologyUrl"
                    }
                });

            var controller = new PublicationController(fileStorageService.Object);

            var publicationMethodologyViewModel = (await controller.GetPublicationMethodology("publication-a")).Value;

            Assert.IsType<PublicationMethodologyViewModel>(publicationMethodologyViewModel);

            Assert.Equal("externalMethodologyTitle", publicationMethodologyViewModel.ExternalMethodology.Title);
            Assert.Equal("externalMethodologyUrl", publicationMethodologyViewModel.ExternalMethodology.Url);

            Assert.Equal(methodologyId, publicationMethodologyViewModel.Methodology.Id);
            Assert.Equal("methodologySlug", publicationMethodologyViewModel.Methodology.Slug);
            Assert.Equal("methodologySummary", publicationMethodologyViewModel.Methodology.Summary);
            Assert.Equal("methodologyTitle", publicationMethodologyViewModel.Methodology.Title);
        }

        [Fact]
        public async Task GetPublicationMethodology_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService
                .Setup(s => s.GetDeserialized<PublicationMethodologyViewModel>(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new PublicationController(fileStorageService.Object);
            var result = await controller.GetPublicationMethodology("missing-publication");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}