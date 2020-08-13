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

            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem)
                .ExpectCheck(SecurityPolicies.CanViewAllTopics)
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
        public async void GetThemes_CanViewLinkedTopics()
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

            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanAccessSystem)
                .ExpectCheck(SecurityPolicies.CanViewAllTopics, false)
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