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
    public class PublicationServiceTests
    {
        [Fact]
        public void GetId_ReturnsA_Publication()
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
                var service = new PublicationService(context);
                
                var result = service.GetPublication("1003fa5c-b60a-4036-a178-e3a69a81b852");

                Assert.Equal("Publication A", result.Title);
            }
        }
    }
}