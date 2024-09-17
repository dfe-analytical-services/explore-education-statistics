#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServiceTests
    {
        [Fact]
        public async Task CreateTheme()
        {
            await using var context = InMemoryApplicationDbContext();

            var service = SetupThemeService(context);
            var result = await service.CreateTheme(
                new ThemeSaveViewModel
                {
                    Title = "Test theme",
                    Summary = "Test summary"
                }
            );

            result.AssertRight();
            Assert.Equal("Test theme", result.Right.Title);
            Assert.Equal("test-theme", result.Right.Slug);
            Assert.Equal("Test summary", result.Right.Summary);

            var savedTheme = await context.Themes.FindAsync(result.Right.Id);

            Assert.Equal("Test theme", savedTheme.Title);
            Assert.Equal("test-theme", savedTheme.Slug);
            Assert.Equal("Test summary", savedTheme.Summary);
        }

        [Fact]
        public async Task CreateTheme_FailsNonUniqueSlug()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                        Summary = "Test summary"
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);
                var result = await service.CreateTheme(
                    new ThemeSaveViewModel
                    {
                        Title = "Test theme",
                        Summary = "Test summary"
                    }
                );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task UpdateTheme()
        {
            var theme = new Theme
            {
                Title = "Test theme",
                Slug = "test-theme",
                Summary = "Test summary"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);
                var result = await service.UpdateTheme(
                    theme.Id,
                    new ThemeSaveViewModel
                    {
                        Title = "Updated theme",
                        Summary = "Updated summary"
                    }
                );

                result.AssertRight();
                Assert.Equal("Updated theme", result.Right.Title);
                Assert.Equal("updated-theme", result.Right.Slug);
                Assert.Equal("Updated summary", result.Right.Summary);

                var savedTheme = await context.Themes.FindAsync(result.Right.Id);

                Assert.Equal("Updated theme", savedTheme.Title);
                Assert.Equal("updated-theme", savedTheme.Slug);
                Assert.Equal("Updated summary", savedTheme.Summary);
            }
        }

        [Fact]
        public async Task UpdateTheme_FailsNonUniqueSlug()
        {
            var theme = new Theme
            {
                Title = "Test theme",
                Slug = "test-theme",
                Summary = "Test summary"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                context.Add(
                    new Theme
                    {
                        Title = "Other theme",
                        Slug = "other-theme",
                        Summary = "Other summary"
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);
                var result = await service.UpdateTheme(
                    theme.Id,
                    new ThemeSaveViewModel
                    {
                        Title = "Other theme",
                        Summary = "Updated summary"
                    }
                );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task GetTheme()
        {
            var theme = new Theme
            {
                Title = "Test theme",
                Slug = "test-theme",
                Summary = "Test summary",
                Topics =
                [
                    new()
                    {
                        Title = "Test topic 1",
                        Slug = "test-topic-1"
                    },
                    new()
                    {
                        Title = "Test topic 2",
                        Slug = "test-topic-2",
                    }
                ]
            };

            // This topic should not be included with
            // the theme as it is unrelated.
            var unrelatedTopic = new Topic
            {
                Title = "Unrelated topic",
                Slug = "unrelated-topic"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                context.Add(unrelatedTopic);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);
                var result = await service.GetTheme(theme.Id);

                var viewModel = result.AssertRight();
                Assert.Equal(theme.Id, viewModel.Id);
                Assert.Equal("Test theme", viewModel.Title);
                Assert.Equal("test-theme", viewModel.Slug);
                Assert.Equal("Test summary", viewModel.Summary);

                Assert.Equal(2, theme.Topics.Count);
                Assert.Equal("Test topic 1", theme.Topics[0].Title);
                Assert.Equal("test-topic-1", theme.Topics[0].Slug);

                Assert.Equal("Test topic 2", theme.Topics[1].Title);
                Assert.Equal("test-topic-2", theme.Topics[1].Slug);
            }
        }

        [Fact]
        public async Task GetThemes()
        {
            var contextId = Guid.NewGuid().ToString();

            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Test summary",
                Topics =
                [
                    new() {
                        Slug = "topic-b",
                        Title = "Topic B"
                    },
                    new() {
                        Slug = "topic-c",
                        Title = "Topic C"
                    },
                    new() {
                        Slug = "topic-a",
                        Title = "Topic A"
                    },
                ]
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);

                var themes = await service.GetThemes();

                Assert.Single(themes.Right);

                var themeViewModel = themes.Right[0];

                Assert.Equal(theme.Id, themeViewModel.Id);
                Assert.Equal("Theme A", themeViewModel.Title);
                Assert.Equal("Test summary", themeViewModel.Summary);

                Assert.Equal(3, themeViewModel.Topics.Count);

                // Orders topics alphabetically
                Assert.Equal(theme.Topics[2].Id, themeViewModel.Topics[0].Id);
                Assert.Equal("Topic A", themeViewModel.Topics[0].Title);

                Assert.Equal(theme.Topics[0].Id, themeViewModel.Topics[1].Id);
                Assert.Equal("Topic B", themeViewModel.Topics[1].Title);

                Assert.Equal(theme.Topics[1].Id, themeViewModel.Topics[2].Id);
                Assert.Equal("Topic C", themeViewModel.Topics[2].Title);
            }
        }

        [Fact]
        public async Task DeleteTheme()
        {
            var theme = new Theme
            {
                Title = "UI test theme",
                Topics =
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "UI test topic 1"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "UI test topic 2"
                    }
                ]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                await context.SaveChangesAsync();

                Assert.Equal(1, await context.Themes.CountAsync());
                Assert.Equal(2, await context.Topics.CountAsync());
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>(Strict);

                topicService
                    .Setup(s => s.DeleteTopic(theme.Topics[0].Id))
                    .ReturnsAsync(Unit.Instance);

                topicService
                    .Setup(s => s.DeleteTopic(theme.Topics[1].Id))
                    .ReturnsAsync(Unit.Instance);

                var service = SetupThemeService(context, topicService: topicService.Object);
                var result = await service.DeleteTheme(theme.Id);
                VerifyAllMocks(topicService);

                result.AssertRight();
                Assert.Equal(0, await context.Themes.CountAsync());
            }
        }

        [Fact]
        public async Task DeleteTheme_DisallowedByNamingConvention()
        {
            var theme = new Theme
            {
                Title = "Non-conforming title",
                Topics =
                [
                    new() { Title = "UI test topic 1" },
                    new() { Title = "UI test topic 2" }
                ]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>(Strict);
                var service = SetupThemeService(context, topicService: topicService.Object);

                var result = await service.DeleteTheme(theme.Id);
                VerifyAllMocks(topicService);
                result.AssertForbidden();

                Assert.Equal(1, await context.Themes.CountAsync());
            }
        }

        [Fact]
        public async Task DeleteTheme_DisallowedByConfiguration()
        {
            var theme = new Theme
            {
                Title = "UI test theme",
                Topics =
                [
                    new() { Title = "UI test topic 1" },
                    new() { Title = "UI test topic 2" }
                ]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>(Strict);

                var service = SetupThemeService(
                    context,
                    topicService: topicService.Object,
                    enableThemeDeletion: false);

                var result = await service.DeleteTheme(theme.Id);
                VerifyAllMocks(topicService);
                result.AssertForbidden();

                Assert.Equal(1, await context.Themes.CountAsync());
            }
        }

        [Fact]
        public async Task DeleteTheme_OtherThemesUnaffected()
        {
            var theme = new Theme
            {
                Title = "UI test theme",
                Topics =
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "UI test topic 1"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "UI test topic 2"
                    }
                ]
            };

            var otherTheme = new Theme
            {
                Title = "UI test theme",
                Topics =
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "UI test topic 1"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "UI test topic 2"
                    }
                ]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(theme, otherTheme);
                await context.SaveChangesAsync();

                Assert.Equal(2, await context.Themes.CountAsync());
                Assert.Equal(4, await context.Topics.CountAsync());
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>(Strict);

                topicService
                    .Setup(s => s.DeleteTopic(theme.Topics[0].Id))
                    .ReturnsAsync(Unit.Instance);

                topicService
                    .Setup(s => s.DeleteTopic(theme.Topics[1].Id))
                    .ReturnsAsync(Unit.Instance);

                var service = SetupThemeService(context, topicService: topicService.Object);
                var result = await service.DeleteTheme(theme.Id);
                VerifyAllMocks(topicService);

                result.AssertRight();
                Assert.Equal(otherTheme.Id, await context.Themes.AsQueryable().Select(t => t.Id).SingleAsync());
            }
        }

        [Fact]
        public async Task DeleteUITestThemes()
        {
            // Arrange
            var uiTestTheme1Id = Guid.NewGuid();
            var standardTitleThemeId = Guid.NewGuid();

            var uiTestThemeTopic = new Topic
            {
                ThemeId = uiTestTheme1Id,
                Title = "UI test theme topic",
            };

            var standardTitleThemeTopic = new Topic
            {
                ThemeId = standardTitleThemeId,
                Title = "Standard title theme topic",
            };

            var uiTestTheme = new Theme
            {
                Id = uiTestTheme1Id,
                Title = "UI test theme",
                Topics = [uiTestThemeTopic]
            };

            var standardTitleTheme = new Theme
            {
                Id = standardTitleThemeId,
                Title = "Standard title",
                Topics = [standardTitleThemeTopic]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(uiTestTheme, standardTitleTheme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>(Strict);

                topicService
                    .Setup(s => s.DeleteTopic(uiTestTheme.Topics[0].Id))
                    .ReturnsAsync(Unit.Instance);

                var service = SetupThemeService(context, topicService: topicService.Object);

                // Act
                await service.DeleteUITestThemes();

                // Assert
                var themesResult = await context.Themes.ToListAsync();
                var topicsResult = await context.Topics.ToListAsync();

                Assert.Single(themesResult);
                Assert.Single(topicsResult);
                Assert.DoesNotContain(themesResult, theme => theme.Title is "UI test theme");
            }
        }

        [Fact]
        public async Task DeleteUITestThemes_DisallowedByConfiguration()
        {
            var theme = new Theme
            {
                Title = "UI test theme",
                Topics =
                [
                    new() { Title = "UI test topic 1" },
                    new() { Title = "UI test topic 2" }
                ]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>(Strict);

                var service = SetupThemeService(
                    context,
                    topicService: topicService.Object,
                    enableThemeDeletion: false);

                var result = await service.DeleteUITestThemes();
                VerifyAllMocks(topicService);
                result.AssertForbidden();

                Assert.Equal(1, await context.Themes.CountAsync());
            }
        }

        private static ThemeService SetupThemeService(
            ContentDbContext context,
            IMapper? mapper = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            ITopicService? topicService = null,
            IPublishingService? publishingService = null,
            bool enableThemeDeletion = true)
        {
            var configuration =
                CreateMockConfiguration(TupleOf("enableThemeDeletion", enableThemeDeletion.ToString()));

            return new ThemeService(
                configuration.Object,
                context,
                mapper ?? AdminMapper(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object,
                topicService ?? new Mock<ITopicService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object
            );
        }
    }
}
