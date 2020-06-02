using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class FastTrackControllerTests
    {
        private readonly FastTrackController _controller;

        private readonly Guid _validId = Guid.NewGuid();
        private readonly Guid _notFoundId = Guid.NewGuid();
        private readonly Guid _releaseId = new Guid("03730cff-22d5-446c-8971-68921e933b50");
        
        public FastTrackControllerTests()
        {
            var fastTrackService = new Mock<IFastTrackService>();

            fastTrackService.Setup(s => s.GetAsync(_releaseId, _validId)).ReturnsAsync(
                new FastTrackViewModel
                {
                    Id = _validId,
                    FullTable = new TableBuilderResultViewModel()
                }
            );

            fastTrackService.Setup(s => s.GetAsync(_releaseId, _notFoundId)).ReturnsAsync(new NotFoundResult());

            _controller = new FastTrackController(fastTrackService.Object);
        }

        [Fact]
        public async void Get_FastTrack()
        {
            var result = await _controller.GetAsync(_releaseId.ToString(), _validId.ToString());

            Assert.IsType<FastTrackViewModel>(result.Value);
            Assert.Equal(_validId, result.Value.Id);
        }

        [Fact]
        public async void Get_FastTrack_NotFound()
        {
            var result = await _controller.GetAsync(_releaseId.ToString(), _notFoundId.ToString());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void Get_FastTrack_InvalidId()
        {
            var result = await _controller.GetAsync(_releaseId.ToString(), "InvalidGuid");
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}