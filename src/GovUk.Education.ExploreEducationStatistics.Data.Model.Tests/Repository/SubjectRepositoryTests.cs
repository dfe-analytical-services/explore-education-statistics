#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class SubjectRepositoryTests
    {
        [Fact]
        public async Task Find_WithSubjectId()
        {
            var subject = new Subject();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(subject);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.Find(subject.Id);

                Assert.NotNull(result);
                Assert.Equal(subject.Id, result!.Id);
            }
        }


        [Fact]
        public async Task Find_WithSubjectId_NotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.Find(Guid.NewGuid());

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task FindPublicationIdForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    PublicationId = Guid.NewGuid(),
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.FindPublicationIdForSubject(releaseSubject.SubjectId);

                Assert.Equal(releaseSubject.Release.PublicationId, result);
            }
        }

        [Fact]
        public async Task FindPublicationIdForSubject_NotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.FindPublicationIdForSubject(Guid.NewGuid());

                Assert.Null(result);
            }
        }

        private static SubjectRepository BuildSubjectService(StatisticsDbContext statisticsDbContext)
        {
            return new SubjectRepository(
                statisticsDbContext
            );
        }
    }
}
