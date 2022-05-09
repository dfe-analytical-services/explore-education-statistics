#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class ReleaseSubjectRepositoryTests
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

            footnoteRepository.Setup(mock => mock.DeleteAllFootnotesBySubject(
                    releaseSubject.ReleaseId,
                    releaseSubject.SubjectId))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var subjectDeleter = new Mock<ReleaseSubjectRepository.SubjectDeleter>();

                subjectDeleter
                    .Setup(
                        s =>
                            s.Delete(
                                It.Is<Subject>(subject => subject.Id == releaseSubject.SubjectId),
                                It.IsAny<StatisticsDbContext>()
                            )
                    );

                var service = BuildReleaseSubjectRepository(statisticsDbContext,
                    footnoteRepository: footnoteRepository.Object,
                    subjectDeleter: subjectDeleter.Object);
                await service.DeleteReleaseSubject(releaseSubject.ReleaseId, releaseSubject.SubjectId);

                MockUtils.VerifyAllMocks(footnoteRepository, subjectDeleter);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

            footnoteRepository.Setup(mock => mock.DeleteAllFootnotesBySubject(
                    releaseSubject.ReleaseId, releaseSubject.SubjectId))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext,
                    footnoteRepository: footnoteRepository.Object);

                await service.DeleteReleaseSubject(releaseSubject.ReleaseId, releaseSubject.SubjectId, true);

                MockUtils.VerifyAllMocks(footnoteRepository);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

            footnoteRepository.Setup(mock => mock.DeleteAllFootnotesBySubject(
                    releaseSubject2.ReleaseId, releaseSubject2.SubjectId))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext,
                    footnoteRepository: footnoteRepository.Object);
                await service.DeleteReleaseSubject(releaseSubject2.ReleaseId, releaseSubject2.SubjectId);

                MockUtils.VerifyAllMocks(footnoteRepository);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

            footnoteRepository.Setup(mock => mock.DeleteAllFootnotesBySubject(
                    It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext,
                    footnoteRepository: footnoteRepository.Object);

                // Try to delete non-existing ReleaseSubject
                await service.DeleteReleaseSubject(Guid.NewGuid(), Guid.NewGuid());

                MockUtils.VerifyAllMocks(footnoteRepository);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

            footnoteRepository.Setup(mock => mock.DeleteAllFootnotesBySubject(release.Id,
                    It.IsIn(releaseSubject1.SubjectId, releaseSubject2.SubjectId)))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var subjectDeleter = new Mock<ReleaseSubjectRepository.SubjectDeleter>();

                subjectDeleter
                    .Setup(
                        s =>
                            s.Delete(
                                It.Is<Subject>(subject => subject.Id == releaseSubject1.SubjectId),
                                It.IsAny<StatisticsDbContext>()
                            )
                    );
                subjectDeleter
                    .Setup(
                        s => s.Delete(
                            It.Is<Subject>(subject => subject.Id == releaseSubject2.SubjectId),
                            It.IsAny<StatisticsDbContext>()
                        )
                    );

                var service = BuildReleaseSubjectRepository(statisticsDbContext,
                    footnoteRepository: footnoteRepository.Object,
                    subjectDeleter: subjectDeleter.Object);
                await service.DeleteAllReleaseSubjects(release.Id);

                MockUtils.VerifyAllMocks(footnoteRepository, subjectDeleter);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

            footnoteRepository.Setup(mock => mock.DeleteAllFootnotesBySubject(release.Id,
                    It.IsIn(releaseSubject1.SubjectId, releaseSubject2.SubjectId)))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext,
                    footnoteRepository: footnoteRepository.Object);
                await service.DeleteAllReleaseSubjects(release.Id, true);

                MockUtils.VerifyAllMocks(footnoteRepository);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
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

        [Fact]
        public async Task GetReleaseSubjectForLatestPublishedVersion()
        {
            var subject = new Subject();

            var releaseSubjectPreviousRelease = new ReleaseSubject
            {
                Subject = subject,
                Release = new Release
                {
                    Id = Guid.NewGuid(),
                    Published = DateTime.UtcNow.AddDays(-2),
                    PreviousVersionId = null
                }
            };

            var releaseSubjectLatestRelease = new ReleaseSubject
            {
                Subject = subject,
                Release = new Release
                {
                    Id = Guid.NewGuid(),
                    Published = DateTime.UtcNow.AddDays(-1),
                    PreviousVersionId = releaseSubjectPreviousRelease.Release.Id
                }
            };

            // Link the Subject to the next version of the Release with a future Published date/time
            // that should not be considered Live
            var releaseSubjectFutureRelease = new ReleaseSubject
            {
                Subject = subject,
                Release = new Release
                {
                    Id = Guid.NewGuid(),
                    Published = DateTime.UtcNow.AddDays(1),
                    PreviousVersionId = releaseSubjectLatestRelease.Release.Id
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(
                    releaseSubjectPreviousRelease,
                    releaseSubjectLatestRelease,
                    releaseSubjectFutureRelease);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext);

                var result = await service.GetReleaseSubjectForLatestPublishedVersion(subject.Id);

                Assert.NotNull(result);
                Assert.Equal(releaseSubjectLatestRelease.ReleaseId, result!.ReleaseId);
                Assert.Equal(releaseSubjectLatestRelease.SubjectId, result.SubjectId);
            }
        }

        [Fact]
        public async Task GetReleaseSubjectForLatestPublishedVersion_NoReleases()
        {
            var subject = new Subject();

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext);
                Assert.Null(await service.GetReleaseSubjectForLatestPublishedVersion(subject.Id));
            }
        }

        [Fact]
        public async Task GetReleaseSubjectForLatestPublishedVersion_NoPublishedReleases()
        {
            // Link the Subject to a Release with a future Published date/time that should not be considered Live
            var releaseSubjectFutureRelease = new ReleaseSubject
            {
                Subject = new Subject(),
                Release = new Release
                {
                    Published = DateTime.UtcNow.AddDays(1)
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubjectFutureRelease);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseSubjectRepository(statisticsDbContext);
                Assert.Null(
                    await service.GetReleaseSubjectForLatestPublishedVersion(releaseSubjectFutureRelease.SubjectId));
            }
        }

        private static ReleaseSubjectRepository BuildReleaseSubjectRepository(
            StatisticsDbContext statisticsDbContext,
            IFootnoteRepository? footnoteRepository = null,
            ReleaseSubjectRepository.SubjectDeleter? subjectDeleter = null)
        {
            return new ReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
                subjectDeleter ?? Mock.Of<ReleaseSubjectRepository.SubjectDeleter>(MockBehavior.Strict)
            );
        }
    }
}
