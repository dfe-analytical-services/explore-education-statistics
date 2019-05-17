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
    public class ReleaseControllerTests
    {
        [Fact]
        public void Get_ReturnsAActionResult_WithAListOfThemes()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "ListReleases");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var releases = new List<Release>
                {
                    new Release {Title = "Release A"},
                    new Release {Title = "Release B"},
                };

                context.AddRange(releases);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ReleaseController(context);

                var result = controller.Get();

                var actionResult = Assert.IsType<ActionResult<List<Release>>>(result);
                var model = Assert.IsAssignableFrom<List<Release>>(actionResult.Value);

                Assert.Equal(2, model.Count);
            }
        }

        [Fact]
        public void GetId_ReturnsAActionResult_WithATheme()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "ReleaseTheme");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var releases = new List<Release>
                {
                    new Release {Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Release A"},
                    new Release {Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Release B"},
                };

                var publications = new List<Publication>
                {
                    new Publication {Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Publication 1"}
                };

                context.AddRange(releases);
                context.AddRange(publications);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ReleaseController(context);

                var result = controller.Get("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<Release>>(result);

                Assert.Equal("Release A", actionResult.Value.Title);
            }
        }
    }
}