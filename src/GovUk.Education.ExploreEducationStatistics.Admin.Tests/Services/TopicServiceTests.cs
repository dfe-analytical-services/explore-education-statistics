using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Content.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Content.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class TopicServiceTests
    {
        [Fact]
        public async Task CreateTopic()
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
                    new TopicSaveViewModel
                    {
                        Title = "Test topic",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsRight);
                Assert.Equal("Test topic", result.Right.Title);
                Assert.Equal("test-topic", result.Right.Slug);
                Assert.Equal(theme.Id, result.Right.ThemeId);

                var savedTopic = await context.Topics.FindAsync(result.Right.Id);

                Assert.Equal("Test topic", savedTopic.Title);
                Assert.Equal("test-topic", savedTopic.Slug);
                Assert.Equal(theme.Id, savedTopic.ThemeId);
            }
        }

        [Fact]
        public async Task CreateTopic_FailsNonExistingTheme()
        {
            await using var context = DbUtils.InMemoryApplicationDbContext();

            var service = SetupTopicService(context);

            var result = await service.CreateTopic(
                new TopicSaveViewModel
                {
                    Title = "Test topic",
                    ThemeId = Guid.NewGuid()
                }
            );

            Assert.True(result.IsLeft);
            ValidationTestUtil.AssertValidationProblem(
                result.Left, ThemeDoesNotExist);
        }

        [Fact]
        public async Task CreateTopic_FailsNonUniqueSlug()
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
                    new TopicSaveViewModel
                    {
                        Title = "Test topic",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsLeft);
                ValidationTestUtil.AssertValidationProblem(
                    result.Left, SlugNotUnique);
            }
        }

        [Fact]
        public async Task UpdateTopic()
        {
            var theme = new Theme
            {
                Title = "New theme",
            };

            var topic = new Topic
            {
                Title = "Old title",
                Slug = "old-title",
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
                    new TopicSaveViewModel
                    {
                        Title = "New title",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsRight);
                Assert.Equal(topic.Id, result.Right.Id);
                Assert.Equal("New title", result.Right.Title);
                Assert.Equal("new-title", result.Right.Slug);
                Assert.Equal(theme.Id, result.Right.ThemeId);

                var savedTopic = await context.Topics.FindAsync(result.Right.Id);

                Assert.Equal("New title", savedTopic.Title);
                Assert.Equal("new-title", savedTopic.Slug);
                Assert.Equal(theme.Id, savedTopic.ThemeId);
            }
        }

        [Fact]
        public async Task UpdateTopic_FailsNonExistingTheme()
        {
            var topic = new Topic
            {
                Title = "Old title",
                Slug = "old-title",
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
                    new TopicSaveViewModel
                    {
                        Title = "Test topic",
                        ThemeId = Guid.NewGuid()
                    }
                );

                Assert.True(result.IsLeft);
                ValidationTestUtil.AssertValidationProblem(
                    result.Left, ThemeDoesNotExist);
            }
        }

        [Fact]
        public async Task UpdateTopic_FailsNonUniqueSlug()
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
                    new TopicSaveViewModel
                    {
                        Title = "Other topic",
                        ThemeId = topic.ThemeId
                    }
                );

                Assert.True(result.IsLeft);
                ValidationTestUtil.AssertValidationProblem(
                    result.Left, SlugNotUnique);
            }
        }

        [Fact]
        public async Task GetTopic()
        {
            var topic = new Topic
            {
                Title = "Test topic",
                Slug = "test-topic",
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
                Assert.Equal(topic.ThemeId, result.Right.ThemeId);
            }
        }

        [Fact]
        public async Task DeleteTopic()
        {
            var topicId = Guid.NewGuid();

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Topic = new Topic
                {
                    Id = topicId,
                    Title = "UI test topic"
                }
            };

            var release = new Release
            {
                PublicationId = publication.Id
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                contentContext.Add(publication);
                statisticsContext.Add(release);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();
            }

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupTopicService(contentContext, statisticsContext: statisticsContext);

                var result = await service.DeleteTopic(topicId);

                Assert.True(result.IsRight);

                Assert.Equal(0, contentContext.Topics.Count());
                Assert.Equal(0, statisticsContext.Release.Count());
            }
        }

        private TopicService SetupTopicService(
            ContentDbContext contentContext,
            StatisticsDbContext statisticsContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IMapper mapper = null,
            IUserService userService = null,
            IReleaseSubjectRepository releaseSubjectRepository = null,
            IReleaseDataFileService releaseDataFilesService = null,
            IReleaseFileService releaseFileService = null,
            IPublishingService publishingService = null)
        {
            return new TopicService(
                contentContext,
                statisticsContext ?? new Mock<StatisticsDbContext>().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                mapper ?? AdminMapper(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                releaseSubjectRepository ?? new Mock<IReleaseSubjectRepository>().Object,
                releaseDataFilesService ?? new Mock<IReleaseDataFileService>().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object
            );
        }
    }
}
