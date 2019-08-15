using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class FastTrackControllerTests
    {
        private readonly FastTrackController _controller;

        private readonly Guid _validId = Guid.NewGuid();
        private readonly PermalinkQueryContext _query = new PermalinkQueryContext();

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
            
            _controller = new FastTrackController(fastTrackService.Object);
        }

        [Fact]
        public async void Get_FastTrack()
        {
            var result = await _controller.Get(_validId) as OkObjectResult;

            Assert.IsAssignableFrom<FastTrackViewModel>(result.Value);

            var link = result.Value as FastTrackViewModel;
            
            Assert.Equal(_validId, link.Id);
        }
        
        [Fact]
        public async void Get_FastTrack_NotFound()
        {
            var result = _controller.Get(Guid.NewGuid());

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}