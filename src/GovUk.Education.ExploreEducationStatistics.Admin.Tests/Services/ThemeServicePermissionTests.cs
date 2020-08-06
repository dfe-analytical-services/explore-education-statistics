using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

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
            var repository = new Mock<IThemeRepository>();

            var themeList = new List<Theme>
            {
                new Theme
                {
                    Id = Guid.NewGuid()
                }
            };

            repository
                .Setup(s => s.GetAllThemesAsync())
                .ReturnsAsync(themeList);

            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem)
                .ExpectCheck(SecurityPolicies.CanViewAllTopics)
                .AssertSuccess(
                    async userService =>
                    {
                        var service = SetupThemeService(
                            userService: userService.Object,
                            themeRepository: repository.Object
                        );

                        var result = await service.GetMyThemes();
                        Assert.Equal(themeList, result.Right);

                        return result;
                    }
                );

            repository.Verify(s => s.GetAllThemesAsync());
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetMyThemes_CanViewLinkedTopics()
        {
            var repository = new Mock<IThemeRepository>();

            var userId = Guid.NewGuid();

            var themeList = new List<Theme>
            {
                new Theme
                {
                    Id = Guid.NewGuid()
                }
            };

            repository
                .Setup(s => s.GetThemesRelatedToUserAsync(userId))
                .ReturnsAsync(themeList);

            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem)
                .ExpectCheck(SecurityPolicies.CanViewAllTopics, false)
                .AssertSuccess(
                    async userService =>
                    {
                        userService
                            .Setup(s => s.GetUserId())
                            .Returns(userId);

                        var service = SetupThemeService(
                            userService: userService.Object,
                            themeRepository: repository.Object
                        );

                        var result = await service.GetMyThemes();
                        Assert.Equal(themeList, result.Right);

                        return result;
                    }
                );

            repository.Verify(s => s.GetThemesRelatedToUserAsync(userId));
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetMyThemes_NoAccessToSystem()
        {
            var repository = new Mock<IThemeRepository>();

            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(
                            userService: userService.Object,
                            themeRepository: repository.Object
                        );

                        return await service.GetMyThemes();
                    }
                );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void CreateTheme()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.CreateTheme(
                            new SaveThemeViewModel
                            {
                                Title = "Test title",
                                Summary = "Test summary"
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateTheme()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.UpdateTheme(
                            _theme.Id,
                            new SaveThemeViewModel
                            {
                                Title = "Test title",
                                Summary = "Test summary"
                            }
                        );
                    }
                );
        }


        [Fact]
        public void GetTheme()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheck(_theme, SecurityPolicies.CanViewSpecificTheme, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetTheme(_theme.Id);
                    }
                );
        }

        private ThemeService SetupThemeService(
            ContentDbContext context = null,
            IMapper mapper = null,
            IUserService userService = null,
            IThemeRepository themeRepository = null,
            PersistenceHelper<ContentDbContext> persistenceHelper = null)
        {
            return new ThemeService(
                context ?? new Mock<ContentDbContext>().Object,
                mapper ?? AdminMapper(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                themeRepository ?? new Mock<IThemeRepository>().Object,
                persistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext, Theme>(_theme.Id, _theme).Object
            );
        }
    }
}