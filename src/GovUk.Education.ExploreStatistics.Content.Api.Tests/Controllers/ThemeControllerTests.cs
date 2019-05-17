using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Controllers
{
    public class ThemeControllerTests
    {
        [Fact]
        public void Get_ReturnsAActionResult_WithAListOfThemes()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "ListThemes");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var themes = new List<Theme>
                {
                    new Theme {Title = "Theme A"},
                    new Theme {Title = "Theme B"},
                };

                context.AddRange(themes);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ThemeController(context);

                var result = controller.Get();

                var actionResult = Assert.IsType<ActionResult<List<Theme>>>(result);
                var model = Assert.IsAssignableFrom<List<Theme>>(actionResult.Value);
                
                Assert.Equal(2, model.Count);
            }
        }
        
        [Fact]
        public void GetId_ReturnsAActionResult_WithATheme()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindTheme");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var themes = new List<Theme>
                {
                    new Theme { Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Theme A"},
                    new Theme {Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "Topic B"},
                };

                context.AddRange(themes);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ThemeController(context);

                var result = controller.Get("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<Theme>>(result);
                
                Assert.Equal("Theme A", actionResult.Value.Title);
            }
        }
        
        [Fact]
        public void GetTopic_ReturnsAActionResult_WithAListOfTopics()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindThemeTopics");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var topics = new List<Topic>
                {
                    new Topic { ThemeId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Topic A"},
                    new Topic { ThemeId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Topic A"},
                    new Topic { ThemeId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "Topic c"},
                };

                context.AddRange(topics);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ThemeController(context);

                var result = controller.GetTopics("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<List<Topic>>>(result);
                
                Assert.Equal(2, actionResult.Value.Count);
            }
        }
    }
}