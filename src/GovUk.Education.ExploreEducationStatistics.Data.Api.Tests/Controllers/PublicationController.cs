using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        private readonly PublicationController _controller;

        private readonly Guid _publicationId = Guid.NewGuid();

        public PublicationControllerTests()
        {
            var publicationService = new Mock<IPublicationService>();

            publicationService
                .Setup(s => s.GetLatestPublicationSubjectsAndHighlights(_publicationId))
                .ReturnsAsync(new SubjectsAndHighlightsViewModel());

            publicationService
                .Setup(s => s.GetLatestPublicationSubjectsAndHighlights(It.Is<Guid>(guid => guid != _publicationId)))
                .ReturnsAsync(new NotFoundResult());

            _controller = new PublicationController(publicationService.Object);
        }

        [Fact]
        public async Task GetPublication_Ok()
        {
            var result = await _controller.GetLatestPublicationSubjectsAndHighlights(_publicationId);
            Assert.IsAssignableFrom<SubjectsAndHighlightsViewModel>(result.Value);
        }

        [Fact]
        public async Task GetPublication_NotFound()
        {
            var result = await _controller.GetLatestPublicationSubjectsAndHighlights(Guid.NewGuid());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}