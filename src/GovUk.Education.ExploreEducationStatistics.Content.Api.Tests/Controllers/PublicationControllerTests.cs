using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
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

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);

            fileStorageService
                .Setup(s =>
                    s.GetDeserialized<PublicationTitleViewModel>("publications/publication-a/publication.json")
                )
                .ReturnsAsync(new PublicationTitleViewModel
                {
                    Id = publicationId,
                    Title = "Test title"
                });

            var controller = BuildPublicationController(fileStorageService.Object);

            var publicationTitleViewModel = (await controller.GetPublicationTitle("publication-a")).Value;

            Assert.IsType<PublicationTitleViewModel>(publicationTitleViewModel);

            Assert.Equal(publicationId, publicationTitleViewModel.Id);
            Assert.Equal("Test title", publicationTitleViewModel.Title);

            MockUtils.VerifyAllMocks(fileStorageService);
        }

        [Fact]
        public async Task GetPublicationTitle_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);

            fileStorageService
                .Setup(s => s.GetDeserialized<PublicationTitleViewModel>(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildPublicationController(fileStorageService.Object);

            var result = await controller.GetPublicationTitle("missing-publication");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(fileStorageService);
        }

        private static PublicationController BuildPublicationController(
            IFileStorageService fileStorageService = null
        )
        {
            return new PublicationController(
                fileStorageService ?? new Mock<IFileStorageService>().Object
            );
        }
    }
}
