#nullable enable
using System.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class ReleaseServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task ListSubjects()
    {
        var statisticsReleaseVersion = new Model.ReleaseVersion();

        var releaseSubject1 = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var releaseSubject2 = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var subject1Filter1 = new Filter { Subject = releaseSubject1.Subject, Label = "subject 1 filter 1" };

        var subject1Filter2 = new Filter { Subject = releaseSubject1.Subject, Label = "subject 1 filter 2" };

        var subject1IndicatorGroup1 = new IndicatorGroup
        {
            Subject = releaseSubject1.Subject,
            Label = "subject 1 indicator group 1",
            Indicators = new List<Indicator>
            {
                new Indicator { Label = "subject 1 indicator group 1 indicator 1" },
                new Indicator { Label = "subject 1 indicator group 1 indicator 2" },
                new Indicator { Label = "subject 1 indicator group 1 indicator 3" },
            },
        };

        var subject1IndicatorGroup2 = new IndicatorGroup
        {
            Subject = releaseSubject1.Subject,
            Label = "subject 1 indicator group 2",
            Indicators = new List<Indicator>
            {
                new Indicator { Label = "subject 1 indicator group 2 indicator 1" },
                new Indicator { Label = "subject 1 indicator group 2 indicator 2" },
            },
        };

        var subject2Filter1 = new Filter { Subject = releaseSubject2.Subject, Label = "subject 2 filter 1" };

        var subject2IndicatorGroup1 = new IndicatorGroup
        {
            Subject = releaseSubject2.Subject,
            Label = "subject 2 indicator group 1",
            Indicators = new List<Indicator> { new Indicator { Label = "subject 2 indicator group 1 indicator 1" } },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1, releaseSubject2);
            await statisticsDbContext.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject2Filter1);
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(
                subject1IndicatorGroup1,
                subject1IndicatorGroup2,
                subject2IndicatorGroup1
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new ReleaseVersion { Id = statisticsReleaseVersion.Id };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Data set 1",
            File = new File
            {
                Filename = "data1.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
            Summary = "Data set 1 guidance",
        };

        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Data set 2",
            File = new File
            {
                Filename = "data2.csv",
                ContentLength = 20480,
                Type = FileType.Data,
                SubjectId = releaseSubject2.Subject.Id,
            },
            Summary = "Data set 2 guidance",
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };

        var import2 = new DataImport { File = releaseFile2.File, Status = DataImportStatus.COMPLETE };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
            await contentDbContext.AddRangeAsync(import1, import2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);
            var timePeriodService = new Mock<ITimePeriodService>(Strict);

            timePeriodService
                .Setup(s => s.GetTimePeriodLabels(releaseSubject1.SubjectId))
                .ReturnsAsync(new TimePeriodLabels("2020/21", "2021/22"));

            timePeriodService
                .Setup(s => s.GetTimePeriodLabels(releaseSubject2.SubjectId))
                .ReturnsAsync(new TimePeriodLabels("2030", "2031"));

            dataGuidanceDataSetService
                .Setup(s => s.ListGeographicLevels(releaseSubject1.SubjectId, default))
                .ReturnsAsync(ListOf("Local Authority", "Local Authority District"));

            dataGuidanceDataSetService
                .Setup(s => s.ListGeographicLevels(releaseSubject2.SubjectId, default))
                .ReturnsAsync(ListOf("National"));

            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            MockUtils.VerifyAllMocks(dataGuidanceDataSetService, timePeriodService);

            var subjects = result.AssertRight();

            Assert.NotNull(subjects);
            Assert.Equal(2, subjects.Count);
            Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
            Assert.Equal(releaseFile1.Name, subjects[0].Name);
            Assert.Equal(releaseFile1.Summary, subjects[0].Content);
            Assert.Equal(releaseFile1.File.Id, subjects[0].File.Id);
            Assert.Equal(releaseFile1.File.Filename, subjects[0].File.FileName);
            Assert.Equal("10 Kb", subjects[0].File.Size);
            Assert.Equal("csv", subjects[0].File.Extension);

            Assert.Equal("2020/21", subjects[0].TimePeriods.From);
            Assert.Equal("2021/22", subjects[0].TimePeriods.To);

            Assert.Equal(2, subjects[0].GeographicLevels.Count);
            Assert.Equal("Local Authority", subjects[0].GeographicLevels[0]);
            Assert.Equal("Local Authority District", subjects[0].GeographicLevels[1]);

            Assert.Equal(2, subjects[0].Filters.Count);
            Assert.Equal("subject 1 filter 1", subjects[0].Filters[0]);
            Assert.Equal("subject 1 filter 2", subjects[0].Filters[1]);

            Assert.Equal(5, subjects[0].Indicators.Count);
            Assert.Equal("subject 1 indicator group 1 indicator 1", subjects[0].Indicators[0]);
            Assert.Equal("subject 1 indicator group 1 indicator 2", subjects[0].Indicators[1]);
            Assert.Equal("subject 1 indicator group 1 indicator 3", subjects[0].Indicators[2]);
            Assert.Equal("subject 1 indicator group 2 indicator 1", subjects[0].Indicators[3]);
            Assert.Equal("subject 1 indicator group 2 indicator 2", subjects[0].Indicators[4]);

            Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
            Assert.Equal(releaseFile2.Name, subjects[1].Name);
            Assert.Equal(releaseFile2.Summary, subjects[1].Content);
            Assert.Equal(releaseFile2.File.Id, subjects[1].File.Id);
            Assert.Equal(releaseFile2.File.Filename, subjects[1].File.FileName);
            Assert.Equal("20 Kb", subjects[1].File.Size);
            Assert.Equal("csv", subjects[1].File.Extension);

            Assert.Equal("2030", subjects[1].TimePeriods.From);
            Assert.Equal("2031", subjects[1].TimePeriods.To);

            Assert.Single(subjects[1].GeographicLevels);
            Assert.Equal("National", subjects[1].GeographicLevels[0]);

            Assert.Single(subjects[1].Filters);
            Assert.Equal("subject 2 filter 1", subjects[1].Filters[0]);

            Assert.Single(subjects[1].Indicators);
            Assert.Equal("subject 2 indicator group 1 indicator 1", subjects[1].Indicators[0]);
        }
    }

    [Fact]
    public async Task ListSubjects_NoSubjects()
    {
        var statsReleaseVersion = new Model.ReleaseVersion();

        var contentReleaseVersion = new ReleaseVersion { Id = statsReleaseVersion.Id };

        await using var statisticsDbContext = InMemoryStatisticsDbContext();
        await using var contentDbContext = InMemoryContentDbContext();

        statisticsDbContext.ReleaseVersion.Add(statsReleaseVersion);
        contentDbContext.ReleaseVersions.Add(contentReleaseVersion);

        var service = BuildReleaseService(contentDbContext: contentDbContext, statisticsDbContext: statisticsDbContext);

        var result = await service.ListSubjects(contentReleaseVersion.Id);

        result.AssertNotFound();
    }

    [Fact]
    public async Task ListSubjects_StatsDbHasMissingSubject()
    {
        Model.ReleaseVersion statisticsReleaseVersion = _fixture.DefaultStatsReleaseVersion();

        ReleaseSubject releaseSubject1 = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statisticsReleaseVersion)
            .WithSubject(_fixture.DefaultSubject());

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject1);
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithId(statisticsReleaseVersion.Id);

        ReleaseFile releaseFile1 = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(contentReleaseVersion)
            .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(releaseSubject1.SubjectId));

        ReleaseFile releaseFile2 = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(contentReleaseVersion)
            .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(Guid.NewGuid()));

        DataImport import1 = _fixture
            .DefaultDataImport()
            .WithFile(releaseFile1.File)
            .WithStatus(DataImportStatus.COMPLETE);

        DataImport import2 = _fixture
            .DefaultDataImport()
            .WithFile(releaseFile2.File)
            .WithStatus(DataImportStatus.COMPLETE);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile1, releaseFile2);
            await contentDbContext.DataImports.AddRangeAsync(import1, import2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var exception = await Assert.ThrowsAsync<DataException>(() =>
                service.ListSubjects(contentReleaseVersion.Id)
            );

            Assert.Equal(
                $"""
                Statistics DB has a different subjects than the Content DB
                StatsDB subjects: {releaseSubject1.SubjectId}
                ContentDb subjects: {releaseFile1.File.SubjectId},{releaseFile2.File.SubjectId}
                """,
                exception.Message
            );
        }
    }

    [Fact]
    public async Task ListSubjects_FiltersPendingReplacementSubjects()
    {
        var statisticsReleaseVersion = new Model.ReleaseVersion();

        var releaseSubject1 = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var releaseSubject2 = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var releaseSubject2Replacement = new ReleaseSubject
        {
            ReleaseVersion = statisticsReleaseVersion,
            Subject = new Subject(),
        };
        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject2Replacement);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new ReleaseVersion { Id = statisticsReleaseVersion.Id };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
        };

        var file2 = new File
        {
            Filename = "data2.csv",
            Type = FileType.Data,
            SubjectId = releaseSubject2.Subject.Id,
        };

        var file2Replacement = new File
        {
            Filename = "data2_replacement.csv",
            Type = FileType.Data,
            SubjectId = releaseSubject2Replacement.Subject.Id,
            Replacing = file2,
        };

        file2.ReplacedBy = file2Replacement;

        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 2",
            File = file2,
        };

        var releaseFile2Replacement = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 2 Replacement",
            File = file2Replacement,
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };

        var import2 = new DataImport { File = releaseFile2.File, Status = DataImportStatus.COMPLETE };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile2Replacement);
            await contentDbContext.AddRangeAsync(import1, import2);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);
        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListGeographicLevels(It.IsAny<Guid>(), default))
            .ReturnsAsync(new List<string>());

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            var subjects = result.AssertRight();

            Assert.NotNull(subjects);
            Assert.Equal(2, subjects.Count);
            Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
            Assert.Equal(releaseFile1.Name, subjects[0].Name);
            Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
            Assert.Equal(releaseFile2.Name, subjects[1].Name);
        }
    }

    [Fact]
    public async Task ListSubjects_FiltersImportingSubjects()
    {
        var statisticsReleaseVersion = new Model.ReleaseVersion();

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = statisticsReleaseVersion,
            Subject = new Subject { Id = Guid.NewGuid() },
        };

        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = statisticsReleaseVersion,
            Subject = new Subject { Id = Guid.NewGuid() },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new ReleaseVersion { Id = statisticsReleaseVersion.Id };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
        };

        var releaseFile2 = new ReleaseFile
        {
            Name = "Data 2",
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                Filename = "data2.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject2.Subject.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.STAGE_1 };

        var import2 = new DataImport { File = releaseFile2.File, Status = DataImportStatus.COMPLETE };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
            await contentDbContext.AddRangeAsync(import1, import2);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);
        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListGeographicLevels(It.IsAny<Guid>(), default))
            .ReturnsAsync(new List<string>());

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            var subjects = result.AssertRight();

            Assert.Single(subjects);
            Assert.Equal(releaseSubject2.Subject.Id, subjects[0].Id);
            Assert.Equal(releaseFile2.Name, subjects[0].Name);
        }
    }

    [Fact]
    public async Task ListSubjects_FiltersSubjectsWithNoImport()
    {
        var statisticsReleaseVersion = new Model.ReleaseVersion();
        var releaseSubject = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new ReleaseVersion { Id = statisticsReleaseVersion.Id };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject.Subject.Id,
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(contentReleaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            var subjects = result.AssertRight();

            Assert.Empty(subjects);
        }
    }

    [Fact]
    public async Task ListSubjects_FiltersSubjectsWithNoFileSubjectId()
    {
        var releaseVersionId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = releaseVersionId };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File { Filename = "data1.csv", Type = FileType.Data },
        };

        var import = new DataImport { File = releaseFile.File, Status = DataImportStatus.COMPLETE };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.AddRangeAsync(releaseFile);
            await contentDbContext.AddRangeAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);

            var result = await service.ListSubjects(releaseVersion.Id);

            var subjects = result.AssertRight();

            Assert.Empty(subjects);
        }
    }

    [Fact]
    public async Task ListSubjects_FilterOrder()
    {
        var statisticsReleaseVersion = new Model.ReleaseVersion();

        var subject1Filter1Id = Guid.NewGuid();
        var subject1Filter2Id = Guid.NewGuid();
        var subject1Filter3Id = Guid.NewGuid();

        var releaseSubject1 = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var subject1Filter1 = new Filter
        {
            Id = subject1Filter1Id,
            Subject = releaseSubject1.Subject,
            Label = "subject 1 filter 1",
        };

        var subject1Filter2 = new Filter
        {
            Id = subject1Filter2Id,
            Subject = releaseSubject1.Subject,
            Label = "subject 1 filter 2",
        };

        var subject1Filter3 = new Filter
        {
            Id = subject1Filter3Id,
            Subject = releaseSubject1.Subject,
            Label = "subject 1 filter 3",
        };

        var subject1IndicatorGroup1 = new IndicatorGroup
        {
            Subject = releaseSubject1.Subject,
            Label = "subject 1 indicator group 1",
            Indicators = new List<Indicator> { new() { Label = "subject 1 indicator group 1 indicator 1" } },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1);
            await statisticsDbContext.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject1Filter3);
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new ReleaseVersion { Id = statisticsReleaseVersion.Id };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
            FilterSequence = new List<FilterSequenceEntry>
            {
                new(subject1Filter2Id, new List<FilterGroupSequenceEntry>()),
                new(subject1Filter1Id, new List<FilterGroupSequenceEntry>()),
                new(subject1Filter3Id, new List<FilterGroupSequenceEntry>()),
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(releaseFile1);
            await contentDbContext.AddRangeAsync(import1);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);
        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListGeographicLevels(It.IsAny<Guid>(), default))
            .ReturnsAsync(new List<string>());

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);
            var subjects = result.AssertRight();
            var subject = Assert.Single(subjects);

            Assert.Equal(3, subject.Filters.Count);
            Assert.Equal("subject 1 filter 2", subject.Filters[0]);
            Assert.Equal("subject 1 filter 1", subject.Filters[1]);
            Assert.Equal("subject 1 filter 3", subject.Filters[2]);
        }
    }

    [Fact]
    public async Task ListSubjects_IndicatorOrder()
    {
        var statisticsReleaseVersion = new Model.ReleaseVersion();

        var subject1Indicator1 = new Indicator { Id = Guid.NewGuid(), Label = "subject 1 indicator 1" };
        var subject1Indicator2 = new Indicator { Id = Guid.NewGuid(), Label = "subject 1 indicator 2" };
        var subject1Indicator3 = new Indicator { Id = Guid.NewGuid(), Label = "subject 1 indicator 3" };

        var releaseSubject1 = new ReleaseSubject { ReleaseVersion = statisticsReleaseVersion, Subject = new Subject() };

        var subject1Filter1 = new Filter { Subject = releaseSubject1.Subject, Label = "subject 1 filter 1" };

        var subject1IndicatorGroup1 = new IndicatorGroup
        {
            Subject = releaseSubject1.Subject,
            Label = "subject 1 indicator group 1",
            Indicators = new List<Indicator> { subject1Indicator1, subject1Indicator2 },
        };

        var subject1IndicatorGroup2 = new IndicatorGroup
        {
            Subject = releaseSubject1.Subject,
            Label = "subject 1 indicator group 2",
            Indicators = new List<Indicator> { subject1Indicator3 },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1);
            await statisticsDbContext.Filter.AddRangeAsync(subject1Filter1);
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1, subject1IndicatorGroup2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new ReleaseVersion { Id = statisticsReleaseVersion.Id };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
            IndicatorSequence = new List<IndicatorGroupSequenceEntry>
            {
                new(Guid.NewGuid(), new List<Guid> { subject1Indicator2.Id, subject1Indicator1.Id }),
                new(Guid.NewGuid(), new List<Guid> { subject1Indicator3.Id }),
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(releaseFile1);
            await contentDbContext.AddRangeAsync(import1);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);
        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListGeographicLevels(It.IsAny<Guid>(), default))
            .ReturnsAsync(new List<string>());

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);
            var subjects = result.AssertRight();
            var subject = Assert.Single(subjects);

            Assert.Equal(3, subject.Indicators.Count);
            Assert.Equal("subject 1 indicator 2", subject.Indicators[0]);
            Assert.Equal("subject 1 indicator 1", subject.Indicators[1]);
            Assert.Equal("subject 1 indicator 3", subject.Indicators[2]);
        }
    }

    [Fact]
    public async Task ListFeaturedTables()
    {
        var releaseVersionId = Guid.NewGuid();
        var releaseVersion = new ReleaseVersion { Id = releaseVersionId };
        var statsReleaseVersion = new Model.ReleaseVersion { Id = releaseVersionId };

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = statsReleaseVersion,
            Subject = new Subject { Id = Guid.NewGuid() },
        };
        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = statsReleaseVersion,
            Subject = new Subject { Id = Guid.NewGuid() },
        };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
        };
        var releaseFile2 = new ReleaseFile
        {
            Name = "Data 2",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data2.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject2.Subject.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };
        var import2 = new DataImport { File = releaseFile2.File, Status = DataImportStatus.COMPLETE };

        var dataBlock1 = new DataBlock
        {
            Name = "Test data block 1",
            Query = new FullTableQuery { SubjectId = releaseSubject1.Subject.Id },
            ReleaseVersion = releaseVersion,
        };
        var featuredTable1 = new FeaturedTable
        {
            DataBlock = dataBlock1,
            Name = "Test featured table name 1",
            Description = "Test featured table description 1",
        };

        var dataBlock2 = new DataBlock
        {
            Name = "Test data block 2",
            Query = new FullTableQuery { SubjectId = releaseSubject2.Subject.Id },
            ReleaseVersion = releaseVersion,
        };
        var featuredTable2 = new FeaturedTable
        {
            DataBlock = dataBlock2,
            Name = "Test featured table name 2",
            Description = "Test featured table description 2",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile1, releaseFile2);
            await contentDbContext.FeaturedTables.AddRangeAsync(featuredTable1, featuredTable2);
            await contentDbContext.DataImports.AddRangeAsync(import1, import2);
            // Order is reversed
            await contentDbContext.ContentBlocks.AddRangeAsync(dataBlock2, dataBlock1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.ListFeaturedTables(releaseVersion.Id);

            var featuredTables = result.AssertRight();

            Assert.Equal(2, featuredTables.Count);

            Assert.Equal(featuredTable1.Id, featuredTables[0].Id);
            Assert.Equal(featuredTable1.Name, featuredTables[0].Name);
            Assert.Equal(featuredTable1.Description, featuredTables[0].Description);
            Assert.Equal(releaseSubject1.SubjectId, featuredTables[0].SubjectId);
            Assert.Equal(dataBlock1.Id, featuredTables[0].DataBlockId);

            Assert.Equal(featuredTable2.Id, featuredTables[1].Id);
            Assert.Equal(featuredTable2.Name, featuredTables[1].Name);
            Assert.Equal(featuredTable2.Description, featuredTables[1].Description);
            Assert.Equal(releaseSubject2.SubjectId, featuredTables[1].SubjectId);
            Assert.Equal(dataBlock2.Id, featuredTables[1].DataBlockId);
        }
    }

    [Fact]
    public async Task ListFeaturedTables_FiltersImportingSubjects()
    {
        var releaseVersionId = Guid.NewGuid();
        var releaseVersion = new ReleaseVersion { Id = releaseVersionId };

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = new Model.ReleaseVersion { Id = releaseVersionId },
            Subject = new Subject { Id = Guid.NewGuid() },
        };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.STAGE_1 };

        var dataBlock1 = new DataBlock
        {
            Name = "Test data block",
            Query = new FullTableQuery { SubjectId = releaseSubject1.Subject.Id },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.AddAsync(releaseFile1);
            await contentDbContext.AddAsync(import1);
            await contentDbContext.AddRangeAsync(dataBlock1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddAsync(releaseSubject1);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.ListFeaturedTables(releaseVersion.Id);

            var featuredTables = result.AssertRight();

            Assert.Empty(featuredTables);
        }
    }

    [Fact]
    public async Task ListFeaturedTables_FiltersNonFeaturedTables()
    {
        var releaseVersionId = Guid.NewGuid();
        var releaseVersion = new ReleaseVersion { Id = releaseVersionId };

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = new Model.ReleaseVersion { Id = releaseVersionId },
            Subject = new Subject { Id = Guid.NewGuid() },
        };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.NOT_FOUND };

        var dataBlock1 = new DataBlock
        {
            Name = "Test data block",
            Query = new FullTableQuery { SubjectId = releaseSubject1.Subject.Id },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.AddAsync(releaseFile1);
            await contentDbContext.AddAsync(import1);
            await contentDbContext.AddRangeAsync(dataBlock1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddAsync(releaseSubject1);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.ListFeaturedTables(releaseVersion.Id);

            var featuredTables = result.AssertRight();

            Assert.Empty(featuredTables);
        }
    }

    [Fact]
    public async Task ListFeaturedTables_FiltersNonMatchingSubjects()
    {
        var releaseVersionId = Guid.NewGuid();
        var releaseVersion = new ReleaseVersion { Id = releaseVersionId };

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = new Model.ReleaseVersion { Id = releaseVersionId },
            Subject = new Subject { Id = Guid.NewGuid() },
        };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject1.Subject.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };

        // Subject does not match
        var dataBlock1 = new DataBlock
        {
            Name = "Test data block",
            Query = new FullTableQuery { SubjectId = Guid.NewGuid() },
        };
        var featuredTable1 = new FeaturedTable
        {
            DataBlock = dataBlock1,
            Name = "Test featured table name",
            Description = "Test featured table description",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile1);
            await contentDbContext.FeaturedTables.AddAsync(featuredTable1);
            await contentDbContext.DataImports.AddAsync(import1);
            await contentDbContext.ContentBlocks.AddRangeAsync(dataBlock1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddAsync(releaseSubject1);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.ListFeaturedTables(releaseVersion.Id);

            var featuredTables = result.AssertRight();

            Assert.Empty(featuredTables);
        }
    }

    private static ReleaseService BuildReleaseService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        StatisticsDbContext? statisticsDbContext = null,
        IUserService? userService = null,
        IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
        ITimePeriodService? timePeriodService = null
    )
    {
        return new ReleaseService(
            contentDbContext,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict)
        );
    }
}
