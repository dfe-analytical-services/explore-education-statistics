#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        [Fact]
        public async Task GetPublicationTitle()
        {
            var publicationId = Guid.NewGuid();

            var publicationService = new Mock<IPublicationService>(Strict);

            publicationService.Setup(mock => mock.GetCachedPublication("publication-a"))
                .ReturnsAsync(new PublicationViewModel
                {
                    Id = publicationId,
                    Title = "Test title",
                });

            var controller = BuildPublicationController(publicationService.Object);

            var result = await controller.GetPublicationTitle("publication-a");

            VerifyAllMocks(publicationService);

            var publicationTitleViewModel = result.AssertOkResult();

            Assert.Equal(publicationId, publicationTitleViewModel.Id);
            Assert.Equal("Test title", publicationTitleViewModel.Title);
        }

        [Fact]
        public async Task GetPublicationTitle_NotFound()
        {
            var publicationService = new Mock<IPublicationService>(Strict);

            publicationService.Setup(mock => mock.GetCachedPublication("missing-publication"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildPublicationController(publicationService.Object);

            var result = await controller.GetPublicationTitle("missing-publication");

            VerifyAllMocks(publicationService);

            result.AssertNotFoundResult();
        }

        private static PublicationController BuildPublicationController(
            IPublicationService? publicationService = null
        )
        {
            return new PublicationController(
                publicationService ?? Mock.Of<IPublicationService>(Strict)
            );
        }
    }
}
