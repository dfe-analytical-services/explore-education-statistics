using System;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
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
        public async void GetThemes_CanViewAllTopics()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem)
                .ExpectCheck(SecurityPolicies.CanViewAllTopics)
                .AssertSuccess(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetThemes();
                    }
                );
        }

        [Fact]
        public void GetThemes_CanViewLinkedTopics()
        {
            var userId = Guid.NewGuid();

            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem)
                .ExpectCheck(SecurityPolicies.CanViewAllTopics, false)
                .AssertSuccess(
                    async userService =>
                    {
                        userService
                            .Setup(s => s.GetUserId())
                            .Returns(userId);

                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetThemes();
                    }
                );
        }

        [Fact]
        public void GetMyThemes_NoAccessToSystem()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetThemes();
                    }
                );
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
            PersistenceHelper<ContentDbContext> persistenceHelper = null)
        {
            return new ThemeService(
                context ?? new Mock<ContentDbContext>().Object,
                mapper ?? AdminMapper(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                persistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext, Theme>(_theme.Id, _theme).Object
            );
        }
    }
}