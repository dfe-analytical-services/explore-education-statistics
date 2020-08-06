using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ThemeControllerTests
    {
        [Fact]
        public async Task CreateTheme_Returns_Ok()
        {
            var request = new SaveThemeViewModel
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
        public async Task GetMyThemes_Returns_Ok()
        {
            var themes = new List<Theme>
            {
                new Theme
                {
                    Title = "Theme A",
                    Topics = new List<Topic>
                    {
                        new Topic
                        {
                            Title = "Topic A"
                        }
                    }
                }
            };

            var themeService = new Mock<IThemeService>();
            themeService.Setup(s => s.GetMyThemes()).ReturnsAsync(themes);

            var controller = new ThemeController(themeService.Object);

            var result = await controller.GetMyThemes();
            Assert.Equal(themes, result.Value);
        }
    }
}