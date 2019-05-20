using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        [Fact]
        public void Get_ReturnsAActionResult_WithAListOfPublications()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "ListPublications");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var publications = new List<Publication>
                {
                    new Publication {Title = "Publication A"},
                    new Publication {Title = "Publication B"},
                };

                context.AddRange(publications);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new PublicationController(context);

                var result = controller.Get();

                var actionResult = Assert.IsType<ActionResult<List<Publication>>>(result);
                var model = Assert.IsAssignableFrom<List<Publication>>(actionResult.Value);

                Assert.Equal(2, model.Count);
            }
        }
        
        [Fact]
        public void GetId_ReturnsAActionResult_WithAPublication()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindPublication");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var publications = new List<Publication>
                {
                    new Publication { Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Publication A"},
                    new Publication {Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "Publication B"},
                };

                context.AddRange(publications);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new PublicationController(context);

                var result = controller.Get("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<Publication>>(result);
                
                Assert.Equal("Publication A", actionResult.Value.Title);
            }
        }
        
        [Fact]
        public void GetLatest_ReturnsAActionResult_WithARelease()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindLatestPublication");
            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var publications = new List<Publication>
                {
                    new Publication { Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Publication A"},
                    new Publication {Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "Publication B"},
                };
                
                var releases = new List<Release>
                {
                    new Release {Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), PublicationId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Release A"},
                    new Release {Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), PublicationId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Release B"},
                };
                
                context.AddRange(publications);
                context.AddRange(releases);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new PublicationController(context);

                var result = controller.GetLatest("1003fa5c-b60a-4036-a178-e3a69a81b852");

                var actionResult = Assert.IsType<ActionResult<Release>>(result);
            }
        }
    }
}