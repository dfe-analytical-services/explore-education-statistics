using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServicePermissionTests
    {
        [Fact]
        public async void GetMyThemes_CanViewAllTopics()
        {
            var (context, userService, repository) = Mocks();

            var themeList = new List<Theme>
            {
                new Theme
                {
                    Id = Guid.NewGuid()
                }
            };

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics))
                .ReturnsAsync(true);

            repository
                .Setup(s => s.GetAllThemesAsync())
                .ReturnsAsync(themeList);
            
            var service = new ThemeService(context.Object, userService.Object, repository.Object);

            var result = await service.GetMyThemesAsync();
            Assert.Equal(themeList, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics));
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetAllThemesAsync());
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyThemes_CanViewLinkedTopics()
        {
            var (context, userService, repository) = Mocks();

            var userId = Guid.NewGuid();

            var themeList = new List<Theme>
            {
                new Theme
                {
                    Id = Guid.NewGuid()
                }
            };

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics))
                .ReturnsAsync(false);

            userService
                .Setup(s => s.GetUserId())
                .Returns(userId);

            repository
                .Setup(s => s.GetThemesRelatedToUserAsync(userId))
                .ReturnsAsync(themeList);
            
            var service = new ThemeService(context.Object, userService.Object, repository.Object);

            var result = await service.GetMyThemesAsync();
            Assert.Equal(themeList, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetThemesRelatedToUserAsync(userId));
            repository.VerifyNoOtherCalls();
        }
        
        private (Mock<ContentDbContext>, Mock<IUserService>, Mock<IThemeRepository>) Mocks()
        {
            var context = new Mock<ContentDbContext>();
            var userService = new Mock<IUserService>();
            var repository = new Mock<IThemeRepository>();

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics))
                .ReturnsAsync(true);

            return (context, userService, repository);
        }
    }
}