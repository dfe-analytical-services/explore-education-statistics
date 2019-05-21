using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class ReleaseServiceTests
    {
        [Fact]
        public void GetLatest_ReturnsA_WithARelease()
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
                var service = new ReleaseService(context);

                var result = service.GetLatestRelease("1003fa5c-b60a-4036-a178-e3a69a81b852");

                Assert.IsType<Release>(result);
            }
        }
        
        [Fact]
        public void GetId_ReturnsA_WithATheme()
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
                var service = new ReleaseService(context);

                var result = service.GetRelease("1003fa5c-b60a-4036-a178-e3a69a81b852");

                Assert.Equal("Release A", result.Title);
            }
        }
    }
}