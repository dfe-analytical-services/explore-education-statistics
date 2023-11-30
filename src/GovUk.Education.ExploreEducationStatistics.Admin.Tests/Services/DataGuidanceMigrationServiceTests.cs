#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-4661 Remove after the EES-4660 data guidance migration is successful
/// </summary>
public class DataGuidanceMigrationServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task MigrateDataGuidance()
    {
        // Set up 2 releases with data sets as follows:
        // Release 1: Data set 1, Data set 2
        // Release 2: Data set 1, Data set 3

        var statsReleases = _fixture.DefaultStatsRelease()
            .GenerateList(2);
        var (statsRelease1, statsRelease2) = statsReleases.ToTuple2();

        var subjects = _fixture.DefaultSubject()
            .GenerateList(3);
        var (subject1, subject2, subject3) = subjects.ToTuple3();

        var release1Subjects = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease1)
            .WithSubjects(ListOf(subject1, subject2))
            .GenerateList(2);
        var (release1Subject1, release1Subject2) = release1Subjects.ToTuple2();

        var release2Subjects = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease2)
            .WithSubjects(ListOf(subject1, subject3))
            .GenerateList(2);
        var (release2Subject1, release2Subject3) = release2Subjects.ToTuple2();

        var contentReleases = _fixture.DefaultRelease()
            .WithIds(statsReleases.Select(r => r.Id))
            .GenerateList(2);
        var (contentRelease1, contentRelease2) = contentReleases.ToTuple2();

        var release1Files = new List<ReleaseFile>
        {
            new()
            {
                Release = contentRelease1,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release1Subject1.SubjectId
                }
            },
            new()
            {
                Release = contentRelease1,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release1Subject2.SubjectId
                }
            }
        };

        var release2Files = new List<ReleaseFile>
        {
            new()
            {
                Release = contentRelease2,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release2Subject1.SubjectId
                }
            },
            new()
            {
                Release = contentRelease2,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release2Subject3.SubjectId
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Release.AddRangeAsync(statsReleases);
            await statisticsDbContext.Subject.AddRangeAsync(subjects);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(release1Subjects.Concat(release2Subjects));
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Releases.AddRangeAsync(contentReleases);
            await contentDbContext.ReleaseFiles.AddRangeAsync(release1Files.Concat(release2Files));
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateDataGuidance(dryRun: false);

            var report = result.AssertRight();

            Assert.False(report.DryRun);
            Assert.Equal(4, report.ReleaseDataFilesExcludingReplacementsCount);
            Assert.Empty(report.FileIdsWithNoMatchingSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseFiles = await contentDbContext.ReleaseFiles.ToListAsync();

            Assert.Equal(release1Subject1.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == release1Files[0].Id).Summary);
            Assert.Equal(release1Subject2.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == release1Files[1].Id).Summary);

            Assert.Equal(release2Subject1.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == release2Files[0].Id).Summary);
            Assert.Equal(release2Subject3.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == release2Files[1].Id).Summary);
        }
    }

    [Fact]
    public async Task MigrateDataGuidance_DryRunMakesNoChanges()
    {
        // Set up 2 releases with data sets as follows:
        // Release 1: Data set 1, Data set 2
        // Release 2: Data set 1, Data set 3

        var statsReleases = _fixture.DefaultStatsRelease()
            .GenerateList(2);
        var (statsRelease1, statsRelease2) = statsReleases.ToTuple2();

        var subjects = _fixture.DefaultSubject()
            .GenerateList(3);
        var (subject1, subject2, subject3) = subjects.ToTuple3();

        var release1Subjects = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease1)
            .WithSubjects(ListOf(subject1, subject2))
            .GenerateList(2);
        var (release1Subject1, release1Subject2) = release1Subjects.ToTuple2();

        var release2Subjects = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease2)
            .WithSubjects(ListOf(subject1, subject3))
            .GenerateList(2);
        var (release2Subject1, release2Subject3) = release2Subjects.ToTuple2();

        var contentReleases = _fixture.DefaultRelease()
            .WithIds(statsReleases.Select(r => r.Id))
            .GenerateList(2);
        var (contentRelease1, contentRelease2) = contentReleases.ToTuple2();

        var release1Files = new List<ReleaseFile>
        {
            new()
            {
                Release = contentRelease1,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release1Subject1.SubjectId
                }
            },
            new()
            {
                Release = contentRelease1,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release1Subject2.SubjectId
                }
            }
        };

        var release2Files = new List<ReleaseFile>
        {
            new()
            {
                Release = contentRelease2,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release2Subject1.SubjectId
                }
            },
            new()
            {
                Release = contentRelease2,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = release2Subject3.SubjectId
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Release.AddRangeAsync(statsReleases);
            await statisticsDbContext.Subject.AddRangeAsync(subjects);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(release1Subjects.Concat(release2Subjects));
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Releases.AddRangeAsync(contentReleases);
            await contentDbContext.ReleaseFiles.AddRangeAsync(release1Files.Concat(release2Files));
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateDataGuidance(dryRun: true);

            var report = result.AssertRight();
            Assert.True(report.DryRun);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseFiles = await contentDbContext.ReleaseFiles.ToListAsync();

            // Release file summaries should be untouched
            Assert.Equal(release1Files[0].Summary,
                actualReleaseFiles.Single(rf => rf.Id == release1Files[0].Id).Summary);
            Assert.Equal(release1Files[1].Summary,
                actualReleaseFiles.Single(rf => rf.Id == release1Files[1].Id).Summary);

            Assert.Equal(release2Files[0].Summary,
                actualReleaseFiles.Single(rf => rf.Id == release2Files[0].Id).Summary);
            Assert.Equal(release2Files[1].Summary,
                actualReleaseFiles.Single(rf => rf.Id == release2Files[1].Id).Summary);
        }
    }

    [Fact]
    public async Task MigrateDataGuidance_IgnoresNonDataFileTypes()
    {
        var statsRelease = _fixture.DefaultStatsRelease().Generate();

        var releaseSubject = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease)
            .WithSubject(_fixture.DefaultSubject().Generate())
            .Generate();

        var contentRelease = _fixture.DefaultRelease()
            .WithId(statsRelease.Id)
            .Generate();

        var releaseFiles = new List<ReleaseFile>
        {
            new()
            {
                Release = contentRelease,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = releaseSubject.SubjectId
                }
            },
            // Set up other release file with a non-data file type
            new()
            {
                Release = contentRelease,
                File = new File
                {
                    Type = FileType.Ancillary
                },
                Summary = "Ancillary file summary"
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Release.AddRangeAsync(statsRelease);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Releases.AddRangeAsync(contentRelease);
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateDataGuidance(dryRun: false);

            var report = result.AssertRight();

            // Count of release data files should not include the other file
            Assert.Equal(1, report.ReleaseDataFilesExcludingReplacementsCount);
            Assert.Empty(report.FileIdsWithNoMatchingSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseFiles = await contentDbContext.ReleaseFiles.ToListAsync();

            Assert.Equal(releaseSubject.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == releaseFiles[0].Id).Summary);

            // Summary of other release file should be untouched
            Assert.Equal("Ancillary file summary",
                actualReleaseFiles.Single(rf => rf.Id == releaseFiles[1].Id).Summary);
        }
    }

    [Fact]
    public async Task MigrateDataGuidance_IgnoresFileReplacementsInProgress()
    {
        // Set up 2 releases with data sets as follows:
        // Release 1: Data set 1
        // Release 2: Data set 1, Data set 2 (replacement of Data set 1 in progress)

        var statsReleases = _fixture.DefaultStatsRelease()
            .GenerateList(2);
        var (statsRelease1, statsRelease2) = statsReleases.ToTuple2();

        var subjects = _fixture.DefaultSubject()
            .GenerateList(2);
        var (subject1, subject2) = subjects.ToTuple2();

        var release1Subject1 = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease1)
            .WithSubject(subject1)
            .Generate();

        var release2Subjects = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease2)
            .WithSubjects(ListOf(subject1, subject2))
            .GenerateList(2);
        var (release2Subject1, release2Subject2) = release2Subjects.ToTuple2();

        var contentReleases = _fixture.DefaultRelease()
            .WithIds(statsReleases.Select(r => r.Id))
            .GenerateList(2);
        var (contentRelease1, contentRelease2) = contentReleases.ToTuple2();

        var release1File1 = new ReleaseFile
        {
            Release = contentRelease1,
            File = new File
            {
                Type = FileType.Data,
                SubjectId = release1Subject1.SubjectId
            }
        };

        // Set up the replacement of the data set in release 2

        var release2File1 = new ReleaseFile
        {
            Release = contentRelease2,
            File = new File
            {
                Type = FileType.Data,
                SubjectId = release2Subject1.SubjectId
            }
        };

        var release2File2 = new ReleaseFile
        {
            Release = contentRelease2,
            File = new File
            {
                Type = FileType.Data,
                SubjectId = release2Subject2.SubjectId,
                Replacing = release2File1.File
            }
        };

        release2File1.File.ReplacedBy = release2File2.File;

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Release.AddRangeAsync(statsReleases);
            await statisticsDbContext.Subject.AddRangeAsync(subjects);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(release1Subject1, release2Subject1,
                release2Subject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Releases.AddRangeAsync(contentReleases);
            await contentDbContext.ReleaseFiles.AddRangeAsync(release1File1, release2File1, release2File2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateDataGuidance(dryRun: false);

            var report = result.AssertRight();

            // Count of release data files should not include the replacement in progress
            Assert.Equal(2, report.ReleaseDataFilesExcludingReplacementsCount);
            Assert.Empty(report.FileIdsWithNoMatchingSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseFiles = await contentDbContext.ReleaseFiles.ToListAsync();

            Assert.Equal(release1Subject1.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == release1File1.Id).Summary);

            // Release file which is in the process of being replaced should have the data guidance set
            // from the original release subject
            Assert.Equal(release2Subject1.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == release2File1.Id).Summary);

            // Release file which is the replacement should not have data guidance set
            // It will be set as a copy of the data guidance from the original file when the replacement is executed
            Assert.Null(actualReleaseFiles.Single(rf => rf.Id == release2File2.Id).Summary);
        }
    }

    [Fact]
    public async Task MigrateDataGuidance_HandlesFileWithSubjectIdNotFound()
    {
        var statsRelease = _fixture.DefaultStatsRelease().Generate();

        var releaseSubject = _fixture.DefaultReleaseSubject()
            .WithRelease(statsRelease)
            .WithSubject(_fixture.DefaultSubject().Generate())
            .WithDataGuidance("Data set guidance")
            .Generate();

        var contentRelease = _fixture.DefaultRelease()
            .WithId(statsRelease.Id)
            .Generate();

        var releaseFiles = new List<ReleaseFile>
        {
            new()
            {
                Release = contentRelease,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = releaseSubject.SubjectId
                }
            },
            // Set up a release data file with a subject id that doesn't exist in the statistics db
            new()
            {
                Release = contentRelease,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            },
            // Set up a release data file with a subject id is null
            new()
            {
                Release = contentRelease,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = null
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Release.AddRangeAsync(statsRelease);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Releases.AddRangeAsync(contentRelease);
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateDataGuidance(dryRun: false);

            var report = result.AssertRight();

            Assert.Equal(3, report.ReleaseDataFilesExcludingReplacementsCount);

            // Files with no matching subject should have their id's added to the report
            Assert.Equal(SetOf(releaseFiles[1].FileId, releaseFiles[2].FileId),
                report.FileIdsWithNoMatchingSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseFiles = await contentDbContext.ReleaseFiles.ToListAsync();

            Assert.Equal(releaseSubject.DataGuidance,
                actualReleaseFiles.Single(rf => rf.Id == releaseFiles[0].Id).Summary);
            Assert.Null(actualReleaseFiles.Single(rf => rf.Id == releaseFiles[1].Id).Summary);
            Assert.Null(actualReleaseFiles.Single(rf => rf.Id == releaseFiles[2].Id).Summary);
        }
    }

    private static DataGuidanceMigrationService SetupService(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        IUserService? userService = null)
    {
        return new DataGuidanceMigrationService(
            contentDbContext ?? Mock.Of<ContentDbContext>(MockBehavior.Strict),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object
        );
    }
}
