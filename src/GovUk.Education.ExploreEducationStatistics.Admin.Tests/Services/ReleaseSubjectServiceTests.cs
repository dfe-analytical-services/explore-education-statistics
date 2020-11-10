using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseSubjectServiceTests
    {
        [Fact]
        public async Task DeleteReleaseSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectService(statisticsDbContext);
                await service.DeleteReleaseSubject(releaseSubject.ReleaseId, releaseSubject.SubjectId);
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
                Assert.Empty(statisticsDbContext.Subject.IgnoreQueryFilters().ToList());
            }
        }

        [Fact]
        public async Task DeleteReleaseSubject_SoftDeleteOrphanedSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectService(statisticsDbContext);
                await service.DeleteReleaseSubject(releaseSubject.ReleaseId, releaseSubject.SubjectId, true);
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());

                var subjects = statisticsDbContext.Subject.IgnoreQueryFilters().ToList();
                Assert.Single(subjects);
                Assert.Equal(releaseSubject.Subject.Id, subjects[0].Id);
                Assert.True(subjects[0].SoftDeleted);
            }
        }

        [Fact]
        public async Task DeleteReleaseSubject_NonOrphanedSubject()
        {
            var subject = new Subject();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = new Release(),
                Subject = subject,
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = new Release(),
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectService(statisticsDbContext);
                await service.DeleteReleaseSubject(releaseSubject2.ReleaseId, releaseSubject2.SubjectId);
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
                Assert.Single(releaseSubjects);
                Assert.Equal(subject.Id, releaseSubjects[0].SubjectId);

                var subjects = statisticsDbContext.Subject.ToList();
                Assert.Single(subjects);
                Assert.Equal(subject.Id, subjects[0].Id);
            }
        }

        [Fact]
        public async Task DeleteReleaseSubject_NotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectService(statisticsDbContext);
                // Try to delete non-existing ReleaseSubject
                await service.DeleteReleaseSubject(Guid.NewGuid(), Guid.NewGuid());
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                // Check that other ReleaseSubject has not been deleted
                var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
                Assert.Single(releaseSubjects);
                Assert.Equal(releaseSubject.SubjectId, releaseSubjects[0].SubjectId);

                var subjects = statisticsDbContext.Subject.ToList();
                Assert.Single(subjects);
                Assert.Equal(releaseSubject.SubjectId, subjects[0].Id);
            }
        }

        [Fact]
        public async Task DeleteAllReleaseSubjects()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectService(statisticsDbContext);
                await service.DeleteAllReleaseSubjects(release.Id);
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
                Assert.Empty(statisticsDbContext.Subject.IgnoreQueryFilters().ToList());
            }
        }

        [Fact]
        public async Task DeleteAllReleaseSubjects_SoftDeleteOrphanedSubjects()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectService(statisticsDbContext);
                await service.DeleteAllReleaseSubjects(release.Id, true);
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());

                var subjects = statisticsDbContext.Subject.IgnoreQueryFilters().ToList();
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.SubjectId, subjects[0].Id);
                Assert.True(subjects[0].SoftDeleted);

                Assert.Equal(releaseSubject2.SubjectId, subjects[1].Id);
                Assert.True(subjects[1].SoftDeleted);
            }
        }

        private ReleaseSubjectService BuildReleaseSubjectService(
            StatisticsDbContext statisticsDbContext,
            IFootnoteService footnoteService = null)
        {
            return new ReleaseSubjectService(
                statisticsDbContext,
                footnoteService ?? new Mock<IFootnoteService>().Object
            );
        }
    }
}