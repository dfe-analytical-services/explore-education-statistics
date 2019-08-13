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
    public class PermalinkControllerTests
    {
        private readonly PermalinkController _controller;

        private readonly Guid _validId = Guid.NewGuid();
        private readonly PermalinkQueryContext _query = new PermalinkQueryContext();

        public PermalinkControllerTests()
        {
            var permalinkService = new Mock<IPermalinkService>();

            permalinkService.Setup(s => s.GetAsync(_validId)).ReturnsAsync(
                new PermalinkViewModel
                {
                    Id = _validId,
                    FullTable = new TableBuilderResultViewModel()
                }
            );
            
            permalinkService.Setup(s => s.CreateAsync(_query)).ReturnsAsync(
                new PermalinkViewModel
                {
                    Id = Guid.NewGuid(),
                    FullTable = new TableBuilderResultViewModel()
                }
            );
            
            _controller = new PermalinkController(permalinkService.Object);
        }

        [Fact]
        public async void Get_Permalink()
        {
            var result = await _controller.Get(_validId) as OkObjectResult;

            Assert.IsAssignableFrom<PermalinkViewModel>(result.Value);

            var link = result.Value as PermalinkViewModel;
            
            Assert.Equal(_validId, link.Id);
        }
        
        [Fact]
        public async void Get_Permalink_NotFound()
        {
            var result = _controller.Get(Guid.NewGuid());

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void Create_Permalink_Returns_Id()
        {
            var result = await  _controller.Create(_query) as OkObjectResult;

            Assert.IsAssignableFrom<PermalinkViewModel>(result.Value);
            var link = result.Value as PermalinkViewModel;

            Assert.NotNull(link.Id);
        }
    }
}