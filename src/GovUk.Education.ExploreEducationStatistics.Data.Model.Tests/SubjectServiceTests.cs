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
        public async Task Get_WithReleaseAndSubjectName()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject
                {
                    Name = "Test subject"
                }
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
                var result = await service.Get(releaseSubject.ReleaseId, "Test subject");

                Assert.NotNull(result);
                Assert.Equal(releaseSubject.SubjectId, result.Id);
                Assert.Equal("Test subject", result.Name);
            }
        }

        [Fact]
        public async Task Get_WithReleaseAndSubjectName_SubjectNameNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject
                {
                    Name = "Test subject"
                }
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
                var result = await service.Get(releaseSubject.ReleaseId, "Not the subject");

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task Get_WithReleaseAndSubjectName_ReleaseNotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);
                var result = await service.Get(Guid.NewGuid(), "Test subject");

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetPublicationForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Publication = new Publication
                    {
                        Title = "Test publication"
                    },
                },
                Subject = new Subject
                {
                    Name = "Test subject"
                }
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
                var result = await service.GetPublicationForSubject(releaseSubject.SubjectId);

                Assert.Equal(releaseSubject.Release.PublicationId, result.Id);
                Assert.Equal("Test publication", result.Title);
            }
        }

        [Fact]
        public async Task GetPublicationForSubject_NotFoundThrows()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectService(context);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GetPublicationForSubject(Guid.NewGuid())
                );
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