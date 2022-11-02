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
        public async Task Get_WithSubjectId()
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
                var result = await service.Get(subject.Id);

                Assert.NotNull(result);
                Assert.Equal(subject.Id, result!.Id);
            }
        }


        [Fact]
        public async Task Get_WithSubjectId_NotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.Get(Guid.NewGuid());

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetPublicationIdForSubject()
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
                var result = await service.GetPublicationIdForSubject(releaseSubject.SubjectId);

                Assert.Equal(releaseSubject.Release.PublicationId, result);
            }
        }

        [Fact]
        public async Task GetPublicationIdForSubject_NotFoundThrows()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GetPublicationIdForSubject(Guid.NewGuid())
                );
            }
        }

        private static SubjectRepository BuildSubjectService(
            StatisticsDbContext statisticsDbContext)
        {
            return new SubjectRepository(
                statisticsDbContext
            );
        }
    }
}
