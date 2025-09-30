#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;

public class ReleaseSubjectRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task DeleteReleaseSubject()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = _fixture.DefaultStatsReleaseVersion(),
            Subject = subject,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock =>
                mock.DeleteFootnotesBySubject(
                    releaseSubject.ReleaseVersionId,
                    releaseSubject.SubjectId
                )
            )
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object
            );

            await service.DeleteReleaseSubject(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId
            );

            MockUtils.VerifyAllMocks(footnoteRepository);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());

            var subjects = statisticsDbContext.Subject.IgnoreQueryFilters().ToList();
            Assert.Single(subjects);
            Assert.Equal(subject.Id, subjects[0].Id);
            Assert.True(subjects[0].SoftDeleted);
        }
    }

    [Fact]
    public async Task DeleteReleaseSubject_NonOrphanedSubject()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = _fixture.DefaultStatsReleaseVersion(),
            Subject = subject,
        };

        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = _fixture.DefaultStatsReleaseVersion(),
            Subject = subject,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(
                releaseSubject1,
                releaseSubject2
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock =>
                mock.DeleteFootnotesBySubject(
                    releaseSubject2.ReleaseVersionId,
                    releaseSubject2.SubjectId
                )
            )
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object
            );

            await service.DeleteReleaseSubject(
                releaseVersionId: releaseSubject2.ReleaseVersionId,
                subjectId: releaseSubject2.SubjectId
            );

            MockUtils.VerifyAllMocks(footnoteRepository);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
            Assert.Single(releaseSubjects);
            Assert.Equal(releaseSubject1.ReleaseVersionId, releaseSubjects[0].ReleaseVersionId);
            Assert.Equal(subject.Id, releaseSubjects[0].SubjectId);

            var subjects = statisticsDbContext.Subject.ToList();
            Assert.Single(subjects);
            Assert.Equal(subject.Id, subjects[0].Id);
        }
    }

    [Fact]
    public async Task DeleteReleaseSubject_SoftDeleteFalse()
    {
        var subject = _fixture.DefaultSubject();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = _fixture.DefaultStatsReleaseVersion(),
            Subject = subject,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock =>
                mock.DeleteFootnotesBySubject(
                    releaseSubject.ReleaseVersionId,
                    releaseSubject.SubjectId
                )
            )
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var subjectDeleter = new Mock<ReleaseSubjectRepository.SubjectDeleter>();

            subjectDeleter.Setup(s =>
                s.Delete(releaseSubject.SubjectId, It.IsAny<StatisticsDbContext>())
            );

            var service = BuildReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object,
                subjectDeleter: subjectDeleter.Object
            );

            await service.DeleteReleaseSubject(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                softDeleteOrphanedSubject: false
            );

            MockUtils.VerifyAllMocks(footnoteRepository, subjectDeleter);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
        }
    }

    [Fact]
    public async Task DeleteReleaseSubject_NotFound()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = _fixture.DefaultStatsReleaseVersion(),
            Subject = subject,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock => mock.DeleteFootnotesBySubject(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object
            );

            // Try to delete non-existing ReleaseSubject
            await service.DeleteReleaseSubject(
                releaseVersionId: Guid.NewGuid(),
                subjectId: Guid.NewGuid()
            );

            MockUtils.VerifyAllMocks(footnoteRepository);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            // Check that other ReleaseSubject has not been deleted
            var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
            Assert.Single(releaseSubjects);
            Assert.Equal(subject.Id, releaseSubjects[0].SubjectId);

            var subjects = statisticsDbContext.Subject.ToList();
            Assert.Single(subjects);
            Assert.Equal(subject.Id, subjects[0].Id);
        }
    }

    [Fact]
    public async Task DeleteAllReleaseSubjects()
    {
        var releaseVersion = _fixture.DefaultStatsReleaseVersion().Generate();

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = _fixture.DefaultSubject(),
        };

        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = _fixture.DefaultSubject(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject1, releaseSubject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock =>
                mock.DeleteFootnotesBySubject(
                    releaseVersion.Id,
                    It.IsIn(releaseSubject1.SubjectId, releaseSubject2.SubjectId)
                )
            )
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object
            );

            await service.DeleteAllReleaseSubjects(releaseVersionId: releaseVersion.Id);

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
    public async Task DeleteAllReleaseSubjects_SoftDeleteFalse()
    {
        var releaseVersion = _fixture.DefaultStatsReleaseVersion().Generate();

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = _fixture.DefaultSubject(),
        };

        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = _fixture.DefaultSubject(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject1, releaseSubject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock =>
                mock.DeleteFootnotesBySubject(
                    releaseVersion.Id,
                    It.IsIn(releaseSubject1.SubjectId, releaseSubject2.SubjectId)
                )
            )
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var subjectDeleter = new Mock<ReleaseSubjectRepository.SubjectDeleter>();

            subjectDeleter.Setup(s =>
                s.Delete(releaseSubject1.SubjectId, It.IsAny<StatisticsDbContext>())
            );
            subjectDeleter.Setup(s =>
                s.Delete(releaseSubject2.SubjectId, It.IsAny<StatisticsDbContext>())
            );

            var service = BuildReleaseSubjectRepository(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object,
                subjectDeleter: subjectDeleter.Object
            );

            await service.DeleteAllReleaseSubjects(
                releaseVersionId: releaseVersion.Id,
                softDeleteOrphanedSubjects: false
            );

            MockUtils.VerifyAllMocks(footnoteRepository, subjectDeleter);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
        }
    }

    private static ReleaseSubjectRepository BuildReleaseSubjectRepository(
        StatisticsDbContext statisticsDbContext,
        IFootnoteRepository? footnoteRepository = null,
        ReleaseSubjectRepository.SubjectDeleter? subjectDeleter = null
    )
    {
        return new ReleaseSubjectRepository(
            statisticsDbContext,
            footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
            subjectDeleter ?? Mock.Of<ReleaseSubjectRepository.SubjectDeleter>(MockBehavior.Strict)
        );
    }
}
