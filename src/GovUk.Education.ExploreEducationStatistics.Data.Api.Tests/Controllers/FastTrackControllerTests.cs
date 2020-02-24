using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class FastTrackControllerTests
    {
        private readonly FastTrackController _controller;

        private readonly Guid _validId = Guid.NewGuid();
        private readonly Guid _notFoundId = Guid.NewGuid();

        public FastTrackControllerTests()
        {
            var fastTrackService = new Mock<IFastTrackService>();

            fastTrackService.Setup(s => s.GetAsync(_validId)).ReturnsAsync(
                new FastTrackViewModel
                {
                    Id = _validId,
                    FullTable = new TableBuilderResultViewModel()
                }
            );

            fastTrackService.Setup(s => s.GetAsync(_notFoundId)).ReturnsAsync(new NotFoundResult());

            _controller = new FastTrackController(fastTrackService.Object);
        }

        [Fact]
        public async void Get_FastTrack()
        {
            var result = await _controller.GetAsync(_validId);

            Assert.IsType<FastTrackViewModel>(result.Value);
            Assert.Equal(_validId, result.Value.Id);
        }

        [Fact]
        public async void Get_FastTrack_NotFound()
        {
            var result = await _controller.GetAsync(_notFoundId);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}