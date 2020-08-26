using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServiceTests
    {
        [Fact]
        public async void CreateTheme()
        {
            await using var context = InMemoryApplicationDbContext();

            var service = SetupThemeService(context);
            var result = await service.CreateTheme(
                new SaveThemeViewModel
                {
                    Title = "Test theme",
                    Summary = "Test summary"
                }
            );

            Assert.True(result.IsRight);
            Assert.Equal("Test theme", result.Right.Title);
            Assert.Equal("test-theme", result.Right.Slug);
            Assert.Equal("Test summary", result.Right.Summary);

            var savedTheme = await context.Themes.FindAsync(result.Right.Id);

            Assert.Equal("Test theme", savedTheme.Title);
            Assert.Equal("test-theme", savedTheme.Slug);
            Assert.Equal("Test summary", savedTheme.Summary);
        }

        [Fact]
        public async void CreateTheme_FailsNonUniqueSlug()
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
                    new SaveThemeViewModel
                    {
                        Title = "Test theme",
                        Summary = "Test summary"
                    }
                );

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateTheme()
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
                    new SaveThemeViewModel
                    {
                        Title = "Updated theme",
                        Summary = "Updated summary"
                    }
                );

                Assert.True(result.IsRight);
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
        public async void UpdateTheme_FailsNonUniqueSlug()
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
                    new SaveThemeViewModel
                    {
                        Title = "Other theme",
                        Summary = "Updated summary"
                    }
                );

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void GetTheme()
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
                var result = await service.GetTheme(theme.Id);

                Assert.True(result.IsRight);
                Assert.Equal(theme.Id, result.Right.Id);
                Assert.Equal("Test theme", result.Right.Title);
                Assert.Equal("test-theme", result.Right.Slug);
                Assert.Equal("Test summary", result.Right.Summary);
            }
        }

        [Fact]
        public async void GetThemes()
        {
            var contextId = Guid.NewGuid().ToString();

            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Test summary",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Slug = "topic-b",
                        Title = "Topic B"
                    },
                    new Topic
                    {
                        Slug = "topic-c",
                        Title = "Topic C"
                    },
                    new Topic
                    {
                        Slug = "topic-a",
                        Title = "Topic A"
                    },
                }
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
        public async void DeleteTheme()
        {
            var theme = new Theme
            {
                Title = "UI test theme",
                Topics = new List<Topic>()
                {
                    new Topic
                    {
                        Title = "UI test topic 1"
                    },
                    new Topic
                    {
                        Title = "UI test topic 2"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var topicService = new Mock<ITopicService>();

                topicService.Setup(s => s.DeleteTopic(theme.Topics[0].Id));
                topicService.Setup(s => s.DeleteTopic(theme.Topics[0].Id));

                var service = SetupThemeService(context, topicService: topicService.Object);
                var result = await service.DeleteTheme(theme.Id);

                Assert.True(result.IsRight);

                Assert.Equal(0, context.Themes.Count());

                topicService.VerifyAll();
            }
        }

        private static ThemeService SetupThemeService(
            ContentDbContext context,
            IMapper mapper = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null,
            ITopicService topicService = null)
        {
            return new ThemeService(
                context,
                mapper ?? AdminMapper(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                topicService ?? new Mock<ITopicService>().Object
            );
        }
    }
}