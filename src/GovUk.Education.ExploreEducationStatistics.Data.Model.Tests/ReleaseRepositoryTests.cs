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
    public class ReleaseRepositoryTests
    {
        [Fact]
        public void GetLatestPublishedRelease()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var publicationAId = Guid.NewGuid();
                var publicationBId = Guid.NewGuid();

                var publicationARelease1 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationAId,
                    Published = DateTime.UtcNow,
                    Slug = "publication-a-release-1",
                    TimeIdentifier = AcademicYearQ1,
                    Year = 2018
                };

                var publicationARelease2 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationAId,
                    Published = DateTime.UtcNow,
                    Slug = "publication-a-release-2",
                    TimeIdentifier = AcademicYearQ4,
                    Year = 2017
                };

                var publicationARelease3 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationAId,
                    Published = null,
                    Slug = "publication-a-release-3",
                    TimeIdentifier = AcademicYearQ2,
                    Year = 2018
                };

                var publicationBRelease1 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationBId,
                    Published = DateTime.UtcNow,
                    Slug = "publication-b-release-1",
                    TimeIdentifier = AcademicYearQ1,
                    Year = 2018
                };

                context.AddRange(new List<Release>
                {
                    publicationARelease1, publicationARelease2, publicationARelease3, publicationBRelease1
                });

                context.SaveChanges();

                var repository = new ReleaseRepository(context);

                var result = repository.GetLatestPublishedRelease(publicationAId);
                Assert.Equal(publicationARelease1, result);
            }
        }

        [Fact]
        public void GetLatestPublishedRelease_PublicationIdNotFound()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var repository = new ReleaseRepository(context);

                var result = repository.GetLatestPublishedRelease(Guid.NewGuid());
                Assert.Null(result);
            }
        }
    }
}
