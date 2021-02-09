using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class PublicationServiceTests
    {
        [Fact]
        public async Task GetPublication()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
            };

            var publicationRelease1 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                Published = DateTime.UtcNow,
                TimeIdentifier = AcademicYearQ1,
                Year = 2018
            };

            var publicationRelease2 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                Published = DateTime.UtcNow,
                TimeIdentifier = AcademicYearQ4,
                Year = 2017
            };

            var publicationRelease3 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                Published = null,
                TimeIdentifier = AcademicYearQ2,
                Year = 2018
            };

            var publicationRelease1Subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 1 subject 1",
            };

            var publicationRelease1Subject1Link = new ReleaseSubject
            {
                ReleaseId = publicationRelease1.Id,
                SubjectId = publicationRelease1Subject1.Id
            };

            var publicationRelease1Subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 1 subject 2"
            };

            var publicationRelease1Subject2Link = new ReleaseSubject
            {
                ReleaseId = publicationRelease1.Id,
                SubjectId = publicationRelease1Subject2.Id
            };

            var publicationRelease2Subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 2 subject"
            };

            var publicationRelease2SubjectLink = new ReleaseSubject
            {
                ReleaseId = publicationRelease2.Id,
                SubjectId = publicationRelease2Subject.Id
            };

            var publicationRelease3Subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 3 subject"
            };

            var publicationRelease3SubjectLink = new ReleaseSubject
            {
                ReleaseId = publicationRelease3.Id,
                SubjectId = publicationRelease3Subject.Id
            };

            var releaseFastTrack1 = new ReleaseFastTrack(publicationRelease1.Id, Guid.NewGuid(), "table 1");
            var releaseFastTrack2 = new ReleaseFastTrack(publicationRelease1.Id, Guid.NewGuid(), "table 2");

            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            await using (var context = new StatisticsDbContext(options, null))
            {
                context.Add(publication);

                await context.AddRangeAsync(new List<Release>
                {
                    publicationRelease1, publicationRelease2, publicationRelease3
                });

                await context.AddRangeAsync(new List<Subject>
                {
                    publicationRelease1Subject1,
                    publicationRelease1Subject2,
                    publicationRelease2Subject,
                    publicationRelease3Subject
                });

                await context.AddRangeAsync(new List<ReleaseSubject>
                {
                    publicationRelease1Subject1Link,
                    publicationRelease1Subject2Link,
                    publicationRelease2SubjectLink,
                    publicationRelease3SubjectLink
                });

                await context.SaveChangesAsync();

                var mocks = Mocks();

                mocks.TableStorageService.Setup(storageService =>
                    storageService.ExecuteQueryAsync(PublicReleaseFastTrackTableName,
                        It.IsAny<TableQuery<ReleaseFastTrack>>())).ReturnsAsync(new List<ReleaseFastTrack>
                {
                    releaseFastTrack1, releaseFastTrack2
                });

                var service = BuildPublicationService(context, mocks);

                var result = (await service.GetPublication(publication.Id)).Right;
                var highlights = result.Highlights.ToList();
                var subjects = result.Subjects.ToList();

                Assert.Equal(publication.Id, result.Id);
                Assert.Equal(2, highlights.Count);
                Assert.Equal(releaseFastTrack1.FastTrackId, highlights[0].Id);
                Assert.Equal(releaseFastTrack1.HighlightName, highlights[0].Label);
                Assert.Equal(releaseFastTrack2.FastTrackId, highlights[1].Id);
                Assert.Equal(releaseFastTrack2.HighlightName, highlights[1].Label);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(publicationRelease1Subject1.Id, subjects[0].Id);
                Assert.Equal(publicationRelease1Subject1.Name, subjects[0].Label);
                Assert.Equal(publicationRelease1Subject2.Id, subjects[1].Id);
                Assert.Equal(publicationRelease1Subject2.Name, subjects[1].Label);
            }
        }

        [Fact]
        public async Task GetPublication_PublicationNotFound()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            await using (var context = new StatisticsDbContext(options, null))
            {
                var mocks = Mocks();
                var service = BuildPublicationService(context, mocks);

                var result = await service.GetPublication(Guid.NewGuid());
                Assert.IsAssignableFrom<NotFoundResult>(result.Left);
            }
        }

        private static PublicationService BuildPublicationService(StatisticsDbContext context,
            (Mock<ILogger<ReleaseService>> LoggerReleaseService,
                Mock<ILogger<SubjectService>> LoggerSubjectService,
                Mock<ITableStorageService> TableStorageService) mocks)
        {
            var releaseService = new ReleaseService(context, mocks.LoggerReleaseService.Object);
            var subjectService = new SubjectService(context, mocks.LoggerSubjectService.Object, releaseService);
            return new PublicationService(releaseService,
                subjectService,
                mocks.TableStorageService.Object,
                MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
        }

        private static (Mock<ILogger<ReleaseService>> LoggerReleaseService,
            Mock<ILogger<SubjectService>> LoggerSubjectService,
            Mock<ITableStorageService> TableStorageService) Mocks()
        {
            return (new Mock<ILogger<ReleaseService>>(),
                new Mock<ILogger<SubjectService>>(),
                new Mock<ITableStorageService>());
        }
    }
}