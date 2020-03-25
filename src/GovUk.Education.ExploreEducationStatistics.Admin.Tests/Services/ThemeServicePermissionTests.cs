using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServicePermissionTests
    {
        private readonly Theme _theme = new Theme
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public async void GetMyThemes_CanViewAllTopics()
        {
            var (userService, repository, persistenceHelper) = Mocks();

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
            
            var service = new ThemeService(userService.Object, repository.Object, persistenceHelper.Object);

            var result = await service.GetMyThemesAsync();
            Assert.Equal(themeList, result.Right);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics));
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetAllThemesAsync());
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyThemes_CanViewLinkedTopics()
        {
            var (userService, repository, persistenceHelper) = Mocks();

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
            
            var service = new ThemeService(userService.Object, repository.Object, persistenceHelper.Object);

            var result = await service.GetMyThemesAsync();
            Assert.Equal(themeList, result.Right);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem));
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllTopics));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetThemesRelatedToUserAsync(userId));
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyThemes_NoAccessToSystem()
        {
            var (userService, repository, persistenceHelper) = Mocks();

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem))
                .ReturnsAsync(false);

            var service = new ThemeService(userService.Object, repository.Object, persistenceHelper.Object);

            var result = await service.GetMyThemesAsync();
            Assert.IsAssignableFrom<ForbidResult>(result.Left);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem));
            userService.VerifyNoOtherCalls();

            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void GetSummaryAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.GetSummaryAsync(_theme.Id),  
                _theme,
                SecurityPolicies.CanViewSpecificTheme);
        }
        
        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<ThemeService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity, params SecurityPolicies[] policies)
            where TEntity : class
        {
            var (userService, repository, persistenceHelper) = Mocks();

            var service = new ThemeService(userService.Object, repository.Object, persistenceHelper.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, userService, service, policies);
        }
        
        private (Mock<IUserService>, Mock<IThemeRepository>, Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            return (
                MockUtils.AlwaysTrueUserService(),
                new Mock<IThemeRepository>(), 
                MockUtils.MockPersistenceHelper<ContentDbContext, Theme>(_theme.Id, _theme));
        }
    }
}