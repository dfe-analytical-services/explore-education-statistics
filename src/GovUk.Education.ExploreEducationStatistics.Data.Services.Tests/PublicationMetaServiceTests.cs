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
    public class PublicationMetaServiceTests
    {
        [Fact]
        public async Task GetSubjectsForLatestRelease()
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
            };

            var publicationRelease1Subject1Link = new ReleaseSubject
            {
                ReleaseId = publicationRelease1.Id,
                SubjectId = publicationRelease1Subject1.Id,
                SubjectName = "Release 1 subject 1",
                Subject = publicationRelease1Subject1
            };

            var publicationRelease1Subject2 = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var publicationRelease1Subject2Link = new ReleaseSubject
            {
                ReleaseId = publicationRelease1.Id,
                SubjectId = publicationRelease1Subject2.Id,
                SubjectName = "Release 1 subject 2",
                Subject = publicationRelease1Subject2
            };

            var publicationRelease2Subject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var publicationRelease2SubjectLink = new ReleaseSubject
            {
                ReleaseId = publicationRelease2.Id,
                SubjectId = publicationRelease2Subject.Id,
                SubjectName = "Release 2 subject",
                Subject = publicationRelease2Subject
            };

            var publicationRelease3Subject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var publicationRelease3SubjectLink = new ReleaseSubject
            {
                ReleaseId = publicationRelease3.Id,
                SubjectId = publicationRelease3Subject.Id,
                SubjectName = "Release 3 subject",
                Subject = publicationRelease3Subject
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

                var service = BuildPublicationMetaService(context, mocks);

                var result = (await service.GetSubjectsForLatestRelease(publication.Id)).Right;
                var highlights = result.Highlights.ToList();
                var subjects = result.Subjects.ToList();

                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(2, highlights.Count);
                Assert.Equal(releaseFastTrack1.FastTrackId, highlights[0].Id);
                Assert.Equal(releaseFastTrack1.HighlightName, highlights[0].Label);
                Assert.Equal(releaseFastTrack2.FastTrackId, highlights[1].Id);
                Assert.Equal(releaseFastTrack2.HighlightName, highlights[1].Label);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(publicationRelease1Subject1.Id, subjects[0].Id);
                Assert.Equal(publicationRelease1Subject1Link.SubjectName, subjects[0].Label);
                Assert.Equal(publicationRelease1Subject2.Id, subjects[1].Id);
                Assert.Equal(publicationRelease1Subject2Link.SubjectName, subjects[1].Label);
            }
        }

        [Fact]
        public async Task GetSubjectsForLatestRelease_PublicationNotFound()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            await using (var context = new StatisticsDbContext(options, null))
            {
                var mocks = Mocks();
                var service = BuildPublicationMetaService(context, mocks);

                var result = await service.GetSubjectsForLatestRelease(Guid.NewGuid());
                Assert.IsAssignableFrom<NotFoundResult>(result.Left);
            }
        }

        private static PublicationMetaService BuildPublicationMetaService(StatisticsDbContext context,
            (Mock<ILogger<ReleaseService>> LoggerReleaseService,
                Mock<ILogger<SubjectService>> LoggerSubjectService,
                Mock<ITableStorageService> TableStorageService) mocks)
        {
            var releaseService = new ReleaseService(context, mocks.LoggerReleaseService.Object);
            var subjectService = new SubjectService(context, mocks.LoggerSubjectService.Object, releaseService);
            return new PublicationMetaService(releaseService,
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
