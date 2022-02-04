using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class SubjectRepositoryTests
    {
        [Fact]
        public async Task IsSubjectForLatestPublishedRelease()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    PublicationId = Guid.NewGuid(),
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
                var releaseRepository = new Mock<IReleaseRepository>();

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(releaseSubject.Release.PublicationId))
                    .Returns(releaseSubject.Release);

                var service = BuildSubjectService(context, releaseRepository: releaseRepository.Object);
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
                    PublicationId = Guid.NewGuid(),
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
                var releaseService = new Mock<IReleaseRepository>();

                releaseService
                    .Setup(s => s.GetLatestPublishedRelease(releaseSubject.Release.PublicationId))
                    .Returns(new Release
                    {
                        Id = Guid.NewGuid()
                    });

                var service = BuildSubjectService(context, releaseRepository: releaseService.Object);
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
                    PublicationId = Guid.NewGuid(),
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
                var releaseRepository = new Mock<IReleaseRepository>();

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(releaseSubject.Release.PublicationId))
                    .Returns(new Release
                    {
                        Id = Guid.NewGuid()
                    });

                var service = BuildSubjectService(context, releaseRepository: releaseRepository.Object);
                var result = await service.IsSubjectForLatestPublishedRelease(releaseSubject.SubjectId);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task Get_WithSubjectId()
        {
            var subject = new Subject();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(subject);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.Get(subject.Id);

                Assert.NotNull(result);
                Assert.Equal(subject.Id, result.Id);
            }
        }


        [Fact]
        public async Task Get_WithSubjectId_NotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GetPublicationIdForSubject(Guid.NewGuid())
                );
            }
        }

        [Fact]
        public async Task FindPublicationForSubject()
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.FindPublicationIdForSubject(releaseSubject.SubjectId);

                Assert.Equal(releaseSubject.Release.PublicationId, result);
            }
        }

        [Fact]
        public async Task FindPublicationForSubject_NotFoundReturnsNull()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.FindPublicationIdForSubject(Guid.NewGuid());

                Assert.Null(result);
            }
        }

        private SubjectRepository BuildSubjectService(
            StatisticsDbContext statisticsDbContext,
            IReleaseRepository releaseRepository = null)
        {
            return new SubjectRepository(
                statisticsDbContext,
                releaseRepository ?? new Mock<IReleaseRepository>().Object
            );
        }
    }
}
