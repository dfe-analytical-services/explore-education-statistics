using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ThemeControllerTests
    {
        
        [Fact]
        public void Get_UserTheme_Returns_Ok()
        {
            var themeService = new Mock<IThemeService>();
            themeService.Setup(s => s.GetMyThemesAsync()).ReturnsAsync(
                new Either<ActionResult, List<Theme>>(new List<Theme>
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
                })
            );
            var controller = new ThemeController(themeService.Object);
            
            // Method under test
            var result = controller.GetMyThemes();
            Assert.IsAssignableFrom<List<Theme>>(((OkObjectResult) result.Result.Result).Value);
        }
    }
}