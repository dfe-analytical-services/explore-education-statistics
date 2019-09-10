using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class PermalinkControllerTests
    {
        private readonly PermalinkController _controller;

        private readonly Guid _createdId = Guid.NewGuid();
        private readonly Guid _validId = Guid.NewGuid();
        private readonly Guid _notFoundId = Guid.NewGuid();
        private readonly TableBuilderQueryContext _query = new TableBuilderQueryContext();

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

            permalinkService.Setup(s => s.GetAsync(_notFoundId)).Throws(new StorageException(new RequestResult
            {
                HttpStatusCode = 404
            }, null, null));

            permalinkService.Setup(s => s.CreateAsync(_query)).ReturnsAsync(
                new PermalinkViewModel
                {
                    Id = _createdId,
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
            var result = await _controller.Get(_notFoundId);

            Assert.NotNull(result);

            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Fact]
        public async void Create_Permalink_Returns_Id()
        {
            var result = await _controller.Create(_query) as OkObjectResult;

            Assert.IsAssignableFrom<PermalinkViewModel>(result.Value);
            var link = result.Value as PermalinkViewModel;

            Assert.Equal(_createdId, link.Id);
        }
    }
}