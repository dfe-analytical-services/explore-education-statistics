using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class MetaControllerTests
    {
        private readonly MetaController _controller;

        public MetaControllerTests()
        {
            var themeMetaService = new Mock<IThemeMetaService>();

            themeMetaService.Setup(s => s.GetThemes())
                .Returns(new List<ThemeMetaViewModel>
                {
                    new ThemeMetaViewModel()
                });

            _controller = new MetaController(themeMetaService.Object);
        }

        [Fact]
        public void Get_Themes_Returns_Ok()
        {
            var result = _controller.GetThemes();

            Assert.IsAssignableFrom<IEnumerable<ThemeMetaViewModel>>(result.Value);
        }
    }
}