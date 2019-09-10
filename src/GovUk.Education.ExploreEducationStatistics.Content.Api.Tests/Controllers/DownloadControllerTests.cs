using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class DownloadControllerTests
    {
        [Fact]
        public void Get_DownloadTree_Returns_Ok()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetDownloadTreeAsync()).ReturnsAsync(
                new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            );

            var controller = new DownloadController(cache.Object);

            var result = controller.GetDownloadTree();

            Assert.IsAssignableFrom<List<ThemeTree>>(result.Result.Value);

            var firstTheme = result.Result.Value.FirstOrDefault();
            Assert.Equal("Theme A", firstTheme.Title);
        }

        [Fact]
        public void Get_DownloadTree_Returns_NoContent()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetDownloadTreeAsync()).ReturnsAsync(
                new List<ThemeTree>()
            );

            var controller = new DownloadController(cache.Object);

            var result = controller.GetDownloadTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }
    }
}