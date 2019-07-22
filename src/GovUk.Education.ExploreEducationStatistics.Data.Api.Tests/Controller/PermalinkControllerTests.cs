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

        private readonly Guid _validId = new Guid();

        public PermalinkControllerTests()
        {
            var permalinkService = new Mock<IPermalinkService>();

            permalinkService.Setup(s => s.Get(_validId)).Returns(
                new Permalink() {Id = _validId}
            );

            _controller = new PermalinkController(permalinkService.Object);
        }

        [Fact]
        public async void Get_Permalink()
        {
            var result = _controller.Get(_validId);

            Assert.IsAssignableFrom<OkObjectResult>(result);
        }

        [Fact]
        public async void Create_Permalink_Returns_Id()
        {
            var result = await  _controller.Create();

            Assert.IsAssignableFrom<OkObjectResult>(result);
        }
    }
}