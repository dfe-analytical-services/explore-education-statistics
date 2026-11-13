#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseDataSetServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Delete()
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
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseSubject.ReleaseVersionId, releaseSubject.SubjectId))
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, footnoteRepository: footnoteRepository.Object);

            await service.Delete(
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
    public async Task Delete_NonOrphanedSubject()
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
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1, releaseSubject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);

        footnoteRepository
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseSubject2.ReleaseVersionId, releaseSubject2.SubjectId))
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, footnoteRepository: footnoteRepository.Object);

            await service.Delete(
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
    public async Task Delete_SoftDeleteFalse()
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
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseSubject.ReleaseVersionId, releaseSubject.SubjectId))
            .Returns(Task.CompletedTask);

        var subjectDeleter = new Mock<ISubjectDeleter>(MockBehavior.Strict);

        subjectDeleter.Setup(mock => mock.Delete(subject.Id));

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object,
                subjectDeleter: subjectDeleter.Object
            );

            await service.Delete(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                softDeleteOrphanedDataSet: false
            );

            MockUtils.VerifyAllMocks(footnoteRepository, subjectDeleter);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
        }
    }

    [Fact]
    public async Task Delete_NotFound()
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
            var service = BuildService(statisticsDbContext, footnoteRepository: footnoteRepository.Object);

            // Try to delete non-existing ReleaseSubject
            await service.Delete(releaseVersionId: Guid.NewGuid(), subjectId: Guid.NewGuid());

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
    public async Task DeleteAll()
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
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseVersion.Id, releaseSubject1.SubjectId))
            .Returns(Task.CompletedTask);
        footnoteRepository
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseVersion.Id, releaseSubject2.SubjectId))
            .Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, footnoteRepository: footnoteRepository.Object);

            await service.DeleteAll(releaseVersionId: releaseVersion.Id);

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
    public async Task DeleteAll_SoftDeleteFalse()
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
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseVersion.Id, releaseSubject1.SubjectId))
            .Returns(Task.CompletedTask);
        footnoteRepository
            .Setup(mock => mock.DeleteFootnotesBySubject(releaseVersion.Id, releaseSubject2.SubjectId))
            .Returns(Task.CompletedTask);

        var subjectDeleter = new Mock<ISubjectDeleter>(MockBehavior.Strict);

        subjectDeleter.Setup(mock => mock.Delete(releaseSubject1.SubjectId));
        subjectDeleter.Setup(mock => mock.Delete(releaseSubject2.SubjectId));

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(
                statisticsDbContext,
                footnoteRepository: footnoteRepository.Object,
                subjectDeleter: subjectDeleter.Object
            );

            await service.DeleteAll(releaseVersionId: releaseVersion.Id, softDeleteOrphanedDataSets: false);

            MockUtils.VerifyAllMocks(footnoteRepository, subjectDeleter);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(statisticsDbContext.ReleaseSubject.ToList());
        }
    }

    private static ReleaseDataSetService BuildService(
        StatisticsDbContext statisticsDbContext,
        IFootnoteRepository? footnoteRepository = null,
        ISubjectDeleter? subjectDeleter = null
    )
    {
        return new ReleaseDataSetService(
            statisticsDbContext,
            footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
            subjectDeleter ?? Mock.Of<ISubjectDeleter>(MockBehavior.Strict)
        );
    }
}
