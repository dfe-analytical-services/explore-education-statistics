using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.WindowsAzure.Storage.Table;


namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class PermalinkControllerTests
    {
        private readonly PermalinkController _controller;

        private readonly Guid _validId = Guid.NewGuid();

        public PermalinkControllerTests()
        {
            var permalinkService = new Mock<IPermalinkService>();

            permalinkService.Setup(s => s.GetAsync(_validId)).ReturnsAsync(
                new Permalink() {RowKey = _validId.ToString()}
            );

            _controller = new PermalinkController(permalinkService.Object);
        }

        [Fact]
        public async void Get_Permalink()
        {
            var result = _controller.Get(_validId);

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
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
            var result = await  _controller.Create();

            Assert.IsAssignableFrom<OkObjectResult>(result);
        }
    }
}