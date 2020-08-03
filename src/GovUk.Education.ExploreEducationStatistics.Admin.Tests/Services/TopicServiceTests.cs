using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class TopicServiceTests
    {
        [Fact]
        public async void GetTopic()
        {
            await using var context = DbUtils.InMemoryApplicationDbContext();

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

            context.Add(topic);
            await context.SaveChangesAsync();

            var (userService, _) = Mocks();

            var service = new TopicService(
                context,
                new PersistenceHelper<ContentDbContext>(context),
                AdminMapper(),
                userService.Object
            );

            var result = await service.GetTopic(topic.Id);

            Assert.True(result.IsRight);

            Assert.Equal(topic.Id, result.Right.Id);
            Assert.Equal("Test topic", result.Right.Title);
            Assert.Equal("Test description", result.Right.Description);
            Assert.Equal("Test summary", result.Right.Summary);
            Assert.Equal(topic.ThemeId, result.Right.ThemeId);
        }

        private (
            Mock<IUserService>,
            Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall<ContentDbContext, Topic>(persistenceHelper);

            return (
                MockUtils.AlwaysTrueUserService(),
                persistenceHelper
            );
        }
    }
}