using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ThemeControllerTests
    {
        [Fact]
        public async Task CreateTheme_Returns_Ok()
        {
            var request = new ThemeSaveViewModel
            {
                Title = "Test theme"
            };

            var viewModel = new ThemeViewModel
            {
                Id = Guid.NewGuid()
            };

            var themeService = new Mock<IThemeService>();
            themeService.Setup(s => s.CreateTheme(request)).ReturnsAsync(viewModel);

            var controller = new ThemeController(themeService.Object);

            var result = await controller.CreateTheme(request);
            Assert.Equal(viewModel, result.Value);
        }

        [Fact]
        public async Task GetThemes_Returns_Ok()
        {
            var themes = new List<ThemeViewModel>
            {
                new ThemeViewModel
                {
                    Title = "Theme A",
                    Topics = new List<TopicViewModel>
                    {
                        new TopicViewModel
                        {
                            Title = "Topic A"
                        }
                    }
                }
            };

            var themeService = new Mock<IThemeService>();
            themeService.Setup(s => s.GetThemes()).ReturnsAsync(themes);

            var controller = new ThemeController(themeService.Object);

            var result = await controller.GetThemes();
            Assert.Equal(themes, result.Value);
        }
    }
}
