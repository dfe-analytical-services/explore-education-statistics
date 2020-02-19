using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests
{
    public class ReleaseServiceTests
    {
        [Fact]
        public void GetLatestRelease()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase("GetLatestRelease");
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var publicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Slug = "publication-a",
                    Title = "Publication A"
                };

                var publicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Slug = "publication-b",
                    Title = "Publication B"
                };

                var publicationARelease1 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationA.Id,
                    ReleaseDate = DateTime.UtcNow,
                    Slug = "publication-a-release-1",
                    TimeIdentifier = AcademicYearQ1,
                    Year = 2018
                };

                var publicationARelease2 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationA.Id,
                    ReleaseDate = DateTime.UtcNow,
                    Slug = "publication-a-release-2",
                    TimeIdentifier = AcademicYearQ4,
                    Year = 2017
                };

                var publicationARelease3 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationA.Id,
                    ReleaseDate = DateTime.UtcNow.AddDays(1),
                    Slug = "publication-a-release-3",
                    TimeIdentifier = AcademicYearQ2,
                    Year = 2018
                };

                var publicationBRelease1 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationB.Id,
                    ReleaseDate = DateTime.UtcNow,
                    Slug = "publication-b-release-1",
                    TimeIdentifier = AcademicYearQ1,
                    Year = 2018
                };

                context.AddRange(new List<Publication>
                {
                    publicationA, publicationB
                });

                context.AddRange(new List<Release>
                {
                    publicationARelease1, publicationARelease2, publicationARelease3, publicationBRelease1
                });

                context.SaveChanges();

                var service = new ReleaseService(context, new Mock<ILogger<ReleaseService>>().Object);

                var result = service.GetLatestRelease(publicationA.Id);
                Assert.Equal(publicationARelease1.Id, result);
            }
        }

        [Fact]
        public void GetLatestRelease_PublicationIdNotFound()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase("GetLatestRelease");
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var service = new ReleaseService(context, new Mock<ILogger<ReleaseService>>().Object);

                var result = service.GetLatestRelease(Guid.NewGuid());
                Assert.False(result.HasValue);
            }
        }
    }
}