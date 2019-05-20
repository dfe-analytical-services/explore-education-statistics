using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Services
{
    public class MethodologyServiceTests
    {
        [Fact]
        public void MethodologyService_GetTree()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "GetTree");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var themes = new List<Theme>
                {
                    new Theme { Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488"), Title = "Theme A"}
                };
                var topics = new List<Topic>
                {
                    new Topic { Id = new Guid("0144e3f2-41e1-4aec-9c55-2671f454c85f"), Title = "Topic A", ThemeId = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488")}
                };
                var publications = new List<Publication>
                {
                    new Publication {Id = new Guid("ed70afba-f7e1-4ab3-bded-74d078b6fca0"), Title = "Publication A", TopicId = new Guid("0144e3f2-41e1-4aec-9c55-2671f454c85f")},
                    new Publication {Id = new Guid("e45cf030-f29b-42c3-8270-3cc8267026f0"), Title = "Publication B", TopicId = new Guid("0144e3f2-41e1-4aec-9c55-2671f454c85f")},
                };

                context.AddRange(themes);
                context.AddRange(topics);
                context.AddRange(publications);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new MethodologyService(context);

                var result = service.GetTree();

                Assert.True(result.Any());
                Assert.Single(result);
                Assert.Equal("Theme A", result.FirstOrDefault().Title);
                Assert.Single(result.FirstOrDefault().Topics);
                Assert.Equal("Topic A", result.FirstOrDefault().Topics.FirstOrDefault().Title);
                Assert.Equal(2, result.FirstOrDefault().Topics.FirstOrDefault().Publications.Count());
            }
        }
    }
}
