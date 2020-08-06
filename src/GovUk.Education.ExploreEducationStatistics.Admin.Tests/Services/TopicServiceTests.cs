using System;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class TopicServiceTests
    {
        [Fact]
        public async void CreateTopic()
        {
            var theme = new Theme
            {
                Title = "Test theme",
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.CreateTopic(
                    new SaveTopicViewModel
                    {
                        Title = "Test topic",
                        Summary = "Test summary",
                        Description = "Test description",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsRight);
                Assert.Equal("Test topic", result.Right.Title);
                Assert.Equal("test-topic", result.Right.Slug);
                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("Test summary", result.Right.Summary);
                Assert.Equal(theme.Id, result.Right.ThemeId);

                var savedTopic = await context.Topics.FindAsync(result.Right.Id);

                Assert.Equal("Test topic", savedTopic.Title);
                Assert.Equal("test-topic", savedTopic.Slug);
                Assert.Equal("Test description", savedTopic.Description);
                Assert.Equal("Test summary", savedTopic.Summary);
                Assert.Equal(theme.Id, savedTopic.ThemeId);
            }
        }

        [Fact]
        public async void CreateTopic_FailsNonExistingTheme()
        {
            await using var context = DbUtils.InMemoryApplicationDbContext();

            var service = SetupTopicService(context);

            var result = await service.CreateTopic(
                new SaveTopicViewModel
                {
                    Title = "Test topic",
                    Summary = "Test summary",
                    Description = "Test description",
                    ThemeId = Guid.NewGuid()
                }
            );

            Assert.True(result.IsLeft);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
            var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            Assert.Equal("THEME_DOES_NOT_EXIST", details.Errors[""].First());
        }

        [Fact]
        public async void CreateTopic_FailsNonUniqueSlug()
        {
            var theme = new Theme
            {
                Title = "Test theme",
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                context.Add(
                    new Topic
                    {
                        Title = "Test topic",
                        Slug = "test-topic",
                        Theme = theme
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.CreateTopic(
                    new SaveTopicViewModel
                    {
                        Title = "Test topic",
                        Summary = "Test summary",
                        Description = "Test description",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateTopic()
        {
            var theme = new Theme
            {
                Title = "New theme",
            };

            var topic = new Topic
            {
                Title = "Old title",
                Slug = "old-title",
                Description = "Old description",
                Summary = "Old summary",
                Theme = new Theme
                {
                    Title = "Old theme"
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                context.Add(topic);

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.UpdateTopic(
                    topic.Id,
                    new SaveTopicViewModel
                    {
                        Title = "New title",
                        Summary = "New summary",
                        Description = "New description",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsRight);
                Assert.Equal(topic.Id, result.Right.Id);
                Assert.Equal("New title", result.Right.Title);
                Assert.Equal("new-title", result.Right.Slug);
                Assert.Equal("New description", result.Right.Description);
                Assert.Equal("New summary", result.Right.Summary);
                Assert.Equal(theme.Id, result.Right.ThemeId);

                var savedTopic = await context.Topics.FindAsync(result.Right.Id);

                Assert.Equal("New title", savedTopic.Title);
                Assert.Equal("new-title", savedTopic.Slug);
                Assert.Equal("New description", savedTopic.Description);
                Assert.Equal("New summary", savedTopic.Summary);
                Assert.Equal(theme.Id, savedTopic.ThemeId);
            }
        }

        [Fact]
        public async void UpdateTopic_FailsNonExistingTheme()
        {
            var topic = new Topic
            {
                Title = "Old title",
                Slug = "old-title",
                Description = "Old description",
                Summary = "Old summary",
                Theme = new Theme
                {
                    Title = "Old theme"
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.UpdateTopic(
                    topic.Id,
                    new SaveTopicViewModel
                    {
                        Title = "Test topic",
                        Summary = "Test summary",
                        Description = "Test description",
                        ThemeId = Guid.NewGuid()
                    }
                );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

                Assert.Equal("THEME_DOES_NOT_EXIST", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateTopic_FailsNonUniqueSlug()
        {
            var theme = new Theme
            {
                Title = "Test theme",
            };
            var topic = new Topic
            {
                Title = "Old title",
                Slug = "old-title",
                Theme = theme
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(
                    new Topic
                    {
                        Title = "Other topic",
                        Slug = "other-topic",
                        Theme = theme
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.UpdateTopic(
                    topic.Id,
                    new SaveTopicViewModel
                    {
                        Title = "Other topic",
                        Summary = "Test summary",
                        Description = "Test description",
                        ThemeId = topic.ThemeId
                    }
                );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void GetTopic()
        {
            var topic = new Topic
            {
                Title = "Test topic",
                Slug = "test-topic",
                Description = "Test description",
                Summary = "Test summary",
                Theme = new Theme
                {
                    Title = "Test theme"
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.GetTopic(topic.Id);

                Assert.True(result.IsRight);

                Assert.Equal(topic.Id, result.Right.Id);
                Assert.Equal("Test topic", result.Right.Title);
                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("Test summary", result.Right.Summary);
                Assert.Equal(topic.ThemeId, result.Right.ThemeId);
            }
        }

        private TopicService SetupTopicService(
            ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IMapper mapper = null,
            IUserService userService = null)
        {
            return new TopicService(
                context,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                mapper ?? AdminMapper(),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}