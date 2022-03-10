#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
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

            var publicationService = new Mock<IPublicationService>(MockBehavior.Strict);

            publicationService.Setup(mock => mock.Get("publication-a"))
                .ReturnsAsync(new CachedPublicationViewModel
                {
                    Id = publicationId,
                    Title = "Test title",
                });

            var controller = BuildPublicationController(publicationService.Object);

            var publicationTitleViewModel = (await controller.GetPublicationTitle("publication-a")).Value;

            Assert.NotNull(publicationTitleViewModel);
            Assert.IsType<PublicationTitleViewModel>(publicationTitleViewModel);
            Assert.Equal(publicationId, publicationTitleViewModel!.Id);
            Assert.Equal("Test title", publicationTitleViewModel.Title);

            MockUtils.VerifyAllMocks();
        }

        [Fact]
        public async Task GetPublicationTitle_NotFound()
        {
            var publicationService = new Mock<IPublicationService>(MockBehavior.Strict);

            publicationService.Setup(mock => mock.Get("missing-publication"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildPublicationController(publicationService.Object);

            var result = await controller.GetPublicationTitle("missing-publication");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(publicationService);
        }

        private static PublicationController BuildPublicationController(
            IPublicationService? publicationService = null
        )
        {
            return new PublicationController(
                publicationService ?? Mock.Of<IPublicationService>()
            );
        }
    }
}
