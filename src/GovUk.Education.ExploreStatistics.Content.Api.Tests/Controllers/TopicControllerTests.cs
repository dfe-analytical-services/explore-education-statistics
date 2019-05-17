using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Controllers
{
    public class TopicControllerTests
    {
        [Fact]
        public void Get_ReturnsAActionResult_WithAListOfTopics()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "ListTopics");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var topics = new List<Topic>
                {
                    new Topic {Title = "Topic A"},
                    new Topic {Title = "Topic B"},
                };

                context.AddRange(topics);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new TopicController(context);

                var result = controller.Get();

                var actionResult = Assert.IsType<ActionResult<List<Topic>>>(result);
                var model = Assert.IsAssignableFrom<List<Topic>>(actionResult.Value);
                
                Assert.Equal(2, model.Count);
            }
        }

        [Fact]
        public void GetId_ReturnsAActionResult_WithATopic()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindTopic");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var topics = new List<Topic>
                {
                    new Topic { Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Topic A"},
                    new Topic {Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "Topic B"},
                };

                context.AddRange(topics);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new TopicController(context);

                var result = controller.Get("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<Topic>>(result);
            }
        }
        
        [Fact]
        public void GetPublications_ReturnsAActionResult_WithATopicsPublications()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindTopicPublications");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var publications = new List<Publication>
                {
                    new Publication { Id = new Guid("5c2f0aae-abe7-4d61-be75-5dedd34fc9dc"), TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Publication B", },
                    new Publication { Id = new Guid("fe6e5c87-86bc-44cd-877e-705433e0e2f1"), TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Publication A"},
                    new Publication { TopicId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "Topic B"},
                };
                
                var releases = new List<Release>
                {
                    new Release { PublicationId = new Guid("5c2f0aae-abe7-4d61-be75-5dedd34fc9dc") },
                    new Release { PublicationId = new Guid("fe6e5c87-86bc-44cd-877e-705433e0e2f1") },
                };
                
                context.AddRange(publications);
                context.AddRange(releases);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new TopicController(context);

                var result = controller.GetPublications("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<List<Publication>>>(result);
                
                Assert.Equal(2, actionResult.Value.Count);
            }
        }
    }
}