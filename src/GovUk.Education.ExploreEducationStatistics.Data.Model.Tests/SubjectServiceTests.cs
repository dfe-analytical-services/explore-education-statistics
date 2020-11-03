using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests
{
    public class SubjectServiceTests
    {
        [Fact]
        public async Task IsSubjectForLatestPublishedRelease()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Publication = new Publication(),
                    Published = DateTime.UtcNow
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = new Mock<IReleaseService>();

                releaseService
                    .Setup(s => s.GetLatestPublishedRelease(releaseSubject.Release.PublicationId))
                    .Returns(releaseSubject.ReleaseId);

                var service = BuildSubjectService(context, releaseService: releaseService.Object);
                var result = await service.IsSubjectForLatestPublishedRelease(releaseSubject.SubjectId);

                Assert.True(result);
            }
        }

        [Fact]
        public async Task IsSubjectForLatestPublishedRelease_SubjectBelongsToOldRelease()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Publication = new Publication(),
                    Published = DateTime.UtcNow
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = new Mock<IReleaseService>();

                releaseService
                    .Setup(s => s.GetLatestPublishedRelease(releaseSubject.Release.PublicationId))
                    .Returns(Guid.NewGuid());

                var service = BuildSubjectService(context, releaseService: releaseService.Object);
                var result = await service.IsSubjectForLatestPublishedRelease(releaseSubject.SubjectId);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task IsSubjectForLatestPublishedRelease_SubjectBelongsToNonLiveRelease()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Publication = new Publication(),
                    Published = null
                },
                Subject = new Subject()
            };


            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = new Mock<IReleaseService>();

                releaseService
                    .Setup(s => s.GetLatestPublishedRelease(releaseSubject.Release.PublicationId))
                    .Returns(Guid.NewGuid());

                var service = BuildSubjectService(context, releaseService: releaseService.Object);
                var result = await service.IsSubjectForLatestPublishedRelease(releaseSubject.SubjectId);

                Assert.False(result);
            }
        }

        private SubjectService BuildSubjectService(
            StatisticsDbContext statisticsDbContext,
            IReleaseService releaseService = null)
        {
            return new SubjectService(
                statisticsDbContext,
                new Mock<ILogger<SubjectService>>().Object,
                releaseService ?? new Mock<IReleaseService>().Object
            );
        }
    }
}