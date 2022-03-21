using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServicePermissionTests
    {
        private readonly Theme _theme = new Theme
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetThemes_CanViewAllTopics()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new Theme
                    {
                        Title = "Test theme"
                    }
                );

                await context.SaveChangesAsync();
            }

            await PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanAccessSystem)
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy)
                .AssertSuccess(
                    async userService =>
                    {
                        await using var context = DbUtils.InMemoryApplicationDbContext(contextId);

                        var service = SetupThemeService(userService: userService.Object, context: context);
                        var result = await service.GetThemes();

                        Assert.Single(result.Right);
                        Assert.Equal("Test theme", result.Right[0].Title);

                        return result;
                    }
                );
        }

        [Fact]
        public async Task GetThemes_CanViewLinkedTopics()
        {
            var userId = Guid.NewGuid();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        Release = new Release
                        {
                            Publication = new Publication
                            {
                                Topic = new Topic
                                {
                                    Title = "Another topic",
                                    Theme = new Theme
                                    {
                                        Title = "Another theme"
                                    }
                                }
                            }
                        }
                    },
                    new UserReleaseRole
                    {
                        UserId = userId,
                        Release = new Release
                        {
                            Publication = new Publication
                            {
                                Topic = new Topic
                                {
                                    Title = "Expected topic",
                                    Theme = new Theme
                                    {
                                        Title = "Expected theme"
                                    }
                                }
                            }
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanAccessSystem)
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertSuccess(
                    async userService =>
                    {
                        await using var context = DbUtils.InMemoryApplicationDbContext(contextId);

                        userService
                            .Setup(s => s.GetUserId())
                            .Returns(userId);

                        var service = SetupThemeService(userService: userService.Object, context: context);
                        var result = await service.GetThemes();

                        Assert.Single(result.Right);
                        Assert.Equal("Expected theme", result.Right[0].Title);

                        Assert.Single(result.Right[0].Topics);
                        Assert.Equal("Expected topic", result.Right[0].Topics[0].Title);

                        return result;
                    }
                );
        }

        [Fact]
        public void GetMyThemes_NoAccessToSystem()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanAccessSystem, false)
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
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.CreateTheme(
                            new ThemeSaveViewModel
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
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.UpdateTheme(
                            _theme.Id,
                            new ThemeSaveViewModel
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
                .SetupResourceCheck(_theme, SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetTheme(_theme.Id);
                    }
                );
        }

        [Fact]
        public void DeleteTheme()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.DeleteTheme(_theme.Id);
                    }
                );
        }

        private static Mock<IConfiguration> DefaultConfigurationMock()
        {
            var mock = new Mock<IConfiguration>(MockBehavior.Strict);
            mock
                .Setup(x => x.GetSection(It.IsAny<String>()))
                .Returns(new Mock<IConfigurationSection>().Object);
            return mock;
        }

        private ThemeService SetupThemeService(
            ContentDbContext context = null,
            IMapper mapper = null,
            PersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null,
            ITopicService topicService = null,
            IPublishingService publishingService = null)
        {
            return new ThemeService(
                DefaultConfigurationMock().Object,
                context ?? new Mock<ContentDbContext>().Object,
                mapper ?? AdminMapper(),
                persistenceHelper ?? MockPersistenceHelper<ContentDbContext, Theme>(_theme.Id, _theme).Object,
                userService ?? AlwaysTrueUserService().Object,
                topicService ?? new Mock<ITopicService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object
            );
        }
    }
}
