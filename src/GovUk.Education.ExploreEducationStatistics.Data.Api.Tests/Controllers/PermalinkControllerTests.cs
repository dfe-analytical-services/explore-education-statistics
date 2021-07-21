using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class PermalinkControllerTests
    {
        private readonly PermalinkController _controller;

        private readonly Guid _createdId = Guid.NewGuid();
        private readonly Guid _validId = Guid.NewGuid();
        private readonly Guid _notFoundId = Guid.NewGuid();
        private readonly CreatePermalinkRequest _request = new CreatePermalinkRequest();
        private readonly Guid _releaseId = new Guid("03730cff-22d5-446c-8971-68921e933b50");

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

            permalinkService.Setup(s => s.GetAsync(_notFoundId)).ReturnsAsync(new NotFoundResult());

            permalinkService.Setup(s => s.CreateAsync(_releaseId, _request)).ReturnsAsync(
                new PermalinkViewModel
                {
                    Id = _createdId,
                    FullTable = new TableBuilderResultViewModel()
                }
            );

            _controller = new PermalinkController(permalinkService.Object);
        }

        [Fact]
        public async Task Get_Permalink()
        {
            var result = await _controller.GetAsync(_validId.ToString());

            Assert.IsAssignableFrom<PermalinkViewModel>(result.Value);
            Assert.Equal(_validId, result.Value.Id);
        }

        [Fact]
        public async Task Get_Permalink_NotFound()
        {
            var result = await _controller.GetAsync(_notFoundId.ToString());
            Assert.IsType<NotFoundResult>(result.Result);
        }
        
        [Fact]
        public async Task Get_Permalink_InvalidId()
        {
            var result = await _controller.GetAsync("InvalidGuid");
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_Permalink_Returns_Id()
        {
            var result = await _controller.CreateAsync(_releaseId, _request);

            Assert.IsType<PermalinkViewModel>(result.Value);
            Assert.Equal(_createdId, result.Value.Id);
        }
    }
}
