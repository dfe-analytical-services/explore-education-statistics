#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class PublicationServiceTests
    {
        [Fact]
        public async Task ListLatestReleaseSubjects()
        {
            var publicationId = Guid.NewGuid();

            var release1 = new Release
            {
                PublicationId = publicationId,
                Published = DateTime.Parse("2018-06-01T09:00:00Z"),
                TimeIdentifier = AcademicYearQ1,
                Year = 2018
            };

            var release2 = new Release
            {
                PublicationId = publicationId,
                Published = DateTime.Parse("2018-03-01T09:00:00Z"),
                TimeIdentifier = AcademicYearQ4,
                Year = 2017
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release1, release2);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var subject1 = new SubjectViewModel(
                    id: Guid.NewGuid(),
                    name: "Subject 1",
                    content: "Subject 1 content",
                    timePeriods: new TimePeriodLabels("2018", "2021"),
                    geographicLevels: ListOf("National", "Local Authority"),
                    file: new FileInfo
                    {
                        Id = Guid.NewGuid(),
                        FileName = "file1.csv",
                        Size = "1 Mb"
                    }
                );
                var subject2 = new SubjectViewModel(
                    id: Guid.NewGuid(),
                    name: "Subject 2",
                    content: "Subject 2 content",
                    timePeriods: new TimePeriodLabels("2015", "2020"),
                    geographicLevels: ListOf("Local Authority District"),
                    file: new FileInfo
                    {
                        Id = Guid.NewGuid(),
                        FileName = "file2.csv",
                        Size = "2 Mb"
                    }
                );

                var releaseService = new Mock<IReleaseService>();

                releaseService
                    .Setup(s => s.ListSubjects(release1.Id))
                    .ReturnsAsync(ListOf(subject1, subject2));

                var service = BuildPublicationService(context, releaseService: releaseService.Object);

                var result = await service.ListLatestReleaseSubjects(publicationId);

                MockUtils.VerifyAllMocks(releaseService);

                var subjects = result.AssertRight();

                Assert.Equal(2, subjects.Count);
                Assert.Equal(subject1, subjects[0]);
                Assert.Equal(subject2, subjects[1]);
            }
        }

        [Fact]
        public async Task ListLatestReleaseSubjects_PublicationNotFound()
        {
            await using var context = StatisticsDbUtils.InMemoryStatisticsDbContext();
            var service = BuildPublicationService(context);

            var result = await service.ListLatestReleaseSubjects(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task ListLatestReleaseFeaturedTables()
        {
            var publicationId = Guid.NewGuid();

            var release1 = new Release
            {
                PublicationId = publicationId,
                Published = DateTime.Parse("2018-06-01T09:00:00Z"),
                TimeIdentifier = AcademicYearQ1,
                Year = 2018
            };

            var release2 = new Release
            {
                PublicationId = publicationId,
                Published = DateTime.Parse("2018-03-01T09:00:00Z"),
                TimeIdentifier = AcademicYearQ4,
                Year = 2017
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release1, release2);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var featuredTable1 = new FeaturedTableViewModel(
                    id: Guid.NewGuid(),
                    name: "Featured table 1",
                    description: "Featured table 1 description"
                );

                var featuredTable2 = new FeaturedTableViewModel(
                    id: Guid.NewGuid(),
                    name: "Featured table 2",
                    description: "Featured table 2 description"
                );

                var releaseService = new Mock<IReleaseService>();

                releaseService
                    .Setup(s => s.ListFeaturedTables(release1.Id))
                    .ReturnsAsync(ListOf(featuredTable1, featuredTable2));

                var service = BuildPublicationService(context, releaseService: releaseService.Object);

                var result = await service.ListLatestReleaseFeaturedTables(publicationId);

                MockUtils.VerifyAllMocks(releaseService);

                var featuredTables = result.AssertRight();

                Assert.Equal(2, featuredTables.Count);
                Assert.Equal(featuredTable1, featuredTables[0]);
                Assert.Equal(featuredTable2, featuredTables[1]);
            }
        }

        [Fact]
        public async Task ListLatestReleaseFeaturedTables_PublicationNotFound()
        {
            await using var context = StatisticsDbUtils.InMemoryStatisticsDbContext();
            var service = BuildPublicationService(context);

            var result = await service.ListLatestReleaseFeaturedTables(Guid.NewGuid());
            result.AssertNotFound();
        }


        private static PublicationService BuildPublicationService(
            StatisticsDbContext context,
            IReleaseRepository? releaseRepository = null,
            IReleaseService? releaseService = null)
        {
            return new PublicationService(
                releaseRepository ?? new ReleaseRepository(context),
                releaseService ?? Mock.Of<IReleaseService>()
            );
        }
    }
}
