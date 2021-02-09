using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class ThemeControllerTests
    {
        private readonly ThemeController _controller;

        public ThemeControllerTests()
        {
            var themeMetaService = new Mock<IThemeService>();

            themeMetaService.Setup(s => s.ListThemes())
                .Returns(new List<ThemeViewModel>
                {
                    new ThemeViewModel()
                });

            _controller = new ThemeController(themeMetaService.Object);
        }

        [Fact]
        public void ListThemes_Ok()
        {
            var result = _controller.ListThemes();

            Assert.IsAssignableFrom<IEnumerable<ThemeViewModel>>(result.Value);
        }
    }
}