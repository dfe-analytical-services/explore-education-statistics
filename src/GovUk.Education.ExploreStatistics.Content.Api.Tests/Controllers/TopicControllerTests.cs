using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
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
    }
}