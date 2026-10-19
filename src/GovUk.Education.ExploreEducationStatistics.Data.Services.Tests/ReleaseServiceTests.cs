#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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
        var subject1 = new Subject();

        var subject2 = new Subject();

        var subject1Filter1 = new Filter { Subject = subject1, Label = "subject 1 filter 1" };

        var subject1Filter2 = new Filter { Subject = subject1, Label = "subject 1 filter 2" };

        var subject1IndicatorGroup1 = new IndicatorGroup
        {
            Subject = subject1,
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
            Subject = subject1,
            Label = "subject 1 indicator group 2",
            Indicators = new List<Indicator>
            {
                new Indicator { Label = "subject 1 indicator group 2 indicator 1" },
                new Indicator { Label = "subject 1 indicator group 2 indicator 2" },
            },
        };

        var subject2Filter1 = new Filter { Subject = subject2, Label = "subject 2 filter 1" };

        var subject2IndicatorGroup1 = new IndicatorGroup
        {
            Subject = subject2,
            Label = "subject 2 indicator group 1",
            Indicators = new List<Indicator> { new Indicator { Label = "subject 2 indicator group 1 indicator 1" } },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject1, subject2);
            await statisticsDbContext.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject2Filter1);
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(
                subject1IndicatorGroup1,
                subject1IndicatorGroup2,
                subject2IndicatorGroup1
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Data set 1",
            File = new File
            {
                Filename = "data1.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                SubjectId = subject1.Id,
                DataSetFileVersionGeographicLevels =
                [
                    new() { GeographicLevel = GeographicLevel.LocalAuthority },
                    new() { GeographicLevel = GeographicLevel.LocalAuthorityDistrict },
                    new() { GeographicLevel = GeographicLevel.School, CsvOnly = true },
                ],
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
                SubjectId = subject2.Id,
                DataSetFileVersionGeographicLevels = [new() { GeographicLevel = GeographicLevel.Country }],
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
            var timePeriodService = new Mock<ITimePeriodService>(Strict);

            timePeriodService
                .Setup(s => s.GetTimePeriodLabels(subject1.Id))
                .ReturnsAsync(new TimePeriodLabels("2020/21", "2021/22"));

            timePeriodService
                .Setup(s => s.GetTimePeriodLabels(subject2.Id))
                .ReturnsAsync(new TimePeriodLabels("2030", "2031"));

            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            MockUtils.VerifyAllMocks(timePeriodService);

            var subjects = result.AssertRight();

            Assert.NotNull(subjects);
            Assert.Equal(2, subjects.Count);
            Assert.Equal(subject1.Id, subjects[0].Id);
            Assert.Equal(releaseFile1.Name, subjects[0].Name);
            Assert.Equal(releaseFile1.Summary, subjects[0].Content);
            Assert.Equal(releaseFile1.File.Id, subjects[0].File.Id);
            Assert.Equal(releaseFile1.File.Filename, subjects[0].File.FileName);
            Assert.Equal("10 Kb", subjects[0].File.Size);
            Assert.Equal("csv", subjects[0].File.Extension);

            Assert.Equal("2020/21", subjects[0].TimePeriods.From);
            Assert.Equal("2021/22", subjects[0].TimePeriods.To);

            Assert.Equal(2, subjects[0].GeographicLevels.Count);
            Assert.Equal("Local authority", subjects[0].GeographicLevels[0]);
            Assert.Equal("Local authority district", subjects[0].GeographicLevels[1]);

            Assert.Single(subjects[0].GeographicLevelsCsvOnly);
            Assert.Equal("School", subjects[0].GeographicLevelsCsvOnly[0]);

            Assert.Equal(2, subjects[0].Filters.Count);
            Assert.Equal("subject 1 filter 1", subjects[0].Filters[0]);
            Assert.Equal("subject 1 filter 2", subjects[0].Filters[1]);

            Assert.Equal(5, subjects[0].Indicators.Count);
            Assert.Equal("subject 1 indicator group 1 indicator 1", subjects[0].Indicators[0]);
            Assert.Equal("subject 1 indicator group 1 indicator 2", subjects[0].Indicators[1]);
            Assert.Equal("subject 1 indicator group 1 indicator 3", subjects[0].Indicators[2]);
            Assert.Equal("subject 1 indicator group 2 indicator 1", subjects[0].Indicators[3]);
            Assert.Equal("subject 1 indicator group 2 indicator 2", subjects[0].Indicators[4]);

            Assert.Equal(subject2.Id, subjects[1].Id);
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

            Assert.Empty(subjects[1].GeographicLevelsCsvOnly);

            Assert.Single(subjects[1].Filters);
            Assert.Equal("subject 2 filter 1", subjects[1].Filters[0]);

            Assert.Single(subjects[1].Indicators);
            Assert.Equal("subject 2 indicator group 1 indicator 1", subjects[1].Indicators[0]);
        }
    }

    [Fact]
    public async Task ListSubjects_NoSubjects()
    {
        var contentReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        await using var statisticsDbContext = InMemoryStatisticsDbContext();
        await using var contentDbContext = InMemoryContentDbContext();

        contentDbContext.ReleaseVersions.Add(contentReleaseVersion);

        var service = BuildReleaseService(contentDbContext: contentDbContext, statisticsDbContext: statisticsDbContext);

        var result = await service.ListSubjects(contentReleaseVersion.Id);

        result.AssertNotFound();
    }

    [Fact]
    public async Task ListSubjects_FiltersPendingReplacementSubjects()
    {
        var subject1 = new Subject();

        var subject2 = new Subject();

        var subject2Replacement = new Subject();
        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(subject1, subject2, subject2Replacement);
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id,
            },
        };

        var file2 = new File
        {
            Filename = "data2.csv",
            Type = FileType.Data,
            SubjectId = subject2.Id,
        };

        var file2Replacement = new File
        {
            Filename = "data2_replacement.csv",
            Type = FileType.Data,
            SubjectId = subject2Replacement.Id,
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

        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            var subjects = result.AssertRight();

            Assert.NotNull(subjects);
            Assert.Equal(2, subjects.Count);
            Assert.Equal(subject1.Id, subjects[0].Id);
            Assert.Equal(releaseFile1.Name, subjects[0].Name);
            Assert.Equal(subject2.Id, subjects[1].Id);
            Assert.Equal(releaseFile2.Name, subjects[1].Name);
        }
    }

    [Fact]
    public async Task ListSubjects_FiltersImportingSubjects()
    {
        var subject1 = new Subject { Id = Guid.NewGuid() };

        var subject2 = new Subject { Id = Guid.NewGuid() };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(subject1, subject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id,
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
                SubjectId = subject2.Id,
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

        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.ListSubjects(contentReleaseVersion.Id);

            var subjects = result.AssertRight();

            Assert.Single(subjects);
            Assert.Equal(subject2.Id, subjects[0].Id);
            Assert.Equal(releaseFile2.Name, subjects[0].Name);
        }
    }

    [Fact]
    public async Task ListSubjects_FiltersSubjectsWithNoImport()
    {
        var subject = new Subject();

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
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
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

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
        var subject1Filter1Id = Guid.NewGuid();
        var subject1Filter2Id = Guid.NewGuid();
        var subject1Filter3Id = Guid.NewGuid();

        var subject1 = new Subject();

        var subject1Filter1 = new Filter
        {
            Id = subject1Filter1Id,
            Subject = subject1,
            Label = "subject 1 filter 1",
        };

        var subject1Filter2 = new Filter
        {
            Id = subject1Filter2Id,
            Subject = subject1,
            Label = "subject 1 filter 2",
        };

        var subject1Filter3 = new Filter
        {
            Id = subject1Filter3Id,
            Subject = subject1,
            Label = "subject 1 filter 3",
        };

        var subject1IndicatorGroup1 = new IndicatorGroup
        {
            Subject = subject1,
            Label = "subject 1 indicator group 1",
            Indicators = new List<Indicator> { new() { Label = "subject 1 indicator group 1 indicator 1" } },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject1);
            await statisticsDbContext.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject1Filter3);
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1);
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                SubjectId = subject1.Id,
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

        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
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
        var subject1Indicator1 = new Indicator { Id = Guid.NewGuid(), Label = "subject 1 indicator 1" };
        var subject1Indicator2 = new Indicator { Id = Guid.NewGuid(), Label = "subject 1 indicator 2" };
        var subject1Indicator3 = new Indicator { Id = Guid.NewGuid(), Label = "subject 1 indicator 3" };

        var subject1 = new Subject();

        var subject1Filter1 = new Filter { Subject = subject1, Label = "subject 1 filter 1" };

        var subject1IndicatorGroup1 = new IndicatorGroup
        {
            Subject = subject1,
            Label = "subject 1 indicator group 1",
            Indicators = new List<Indicator> { subject1Indicator1, subject1Indicator2 },
        };

        var subject1IndicatorGroup2 = new IndicatorGroup
        {
            Subject = subject1,
            Label = "subject 1 indicator group 2",
            Indicators = new List<Indicator> { subject1Indicator3 },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject1);
            await statisticsDbContext.Filter.AddRangeAsync(subject1Filter1);
            await statisticsDbContext.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1, subject1IndicatorGroup2);
            await statisticsDbContext.SaveChangesAsync();
        }

        ReleaseVersion contentReleaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                Filename = "data1.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                SubjectId = subject1.Id,
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

        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        timePeriodService.Setup(s => s.GetTimePeriodLabels(It.IsAny<Guid>())).ReturnsAsync(new TimePeriodLabels());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildReleaseService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
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

        var subject1 = new Subject { Id = Guid.NewGuid() };
        var subject2 = new Subject { Id = Guid.NewGuid() };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id,
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
                SubjectId = subject2.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };
        var import2 = new DataImport { File = releaseFile2.File, Status = DataImportStatus.COMPLETE };

        var dataBlockVersion1 = new DataBlockVersion
        {
            Name = "Test data block 1",
            Query = new FullTableQuery { SubjectId = subject1.Id },
            ReleaseVersion = releaseVersion,
        };
        var featuredTable1 = new FeaturedTable
        {
            DataBlockVersion = dataBlockVersion1,
            Name = "Test featured table name 1",
            Description = "Test featured table description 1",
        };

        var dataBlockVersion2 = new DataBlockVersion
        {
            Name = "Test data block 2",
            Query = new FullTableQuery { SubjectId = subject2.Id },
            ReleaseVersion = releaseVersion,
        };
        var featuredTable2 = new FeaturedTable
        {
            DataBlockVersion = dataBlockVersion2,
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
            await contentDbContext.DataBlockVersions.AddRangeAsync(dataBlockVersion2, dataBlockVersion1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddRangeAsync(subject1, subject2);
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
            Assert.Equal(subject1.Id, featuredTables[0].SubjectId);
            Assert.Equal(dataBlockVersion1.Id, featuredTables[0].DataBlockVersionId);

            Assert.Equal(featuredTable2.Id, featuredTables[1].Id);
            Assert.Equal(featuredTable2.Name, featuredTables[1].Name);
            Assert.Equal(featuredTable2.Description, featuredTables[1].Description);
            Assert.Equal(subject2.Id, featuredTables[1].SubjectId);
            Assert.Equal(dataBlockVersion2.Id, featuredTables[1].DataBlockVersionId);
        }
    }

    [Fact]
    public async Task ListFeaturedTables_FiltersImportingSubjects()
    {
        var releaseVersionId = Guid.NewGuid();
        var releaseVersion = new ReleaseVersion { Id = releaseVersionId };

        var subject1 = new Subject { Id = Guid.NewGuid() };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.STAGE_1 };

        var dataBlockVersion1 = new DataBlockVersion
        {
            Name = "Test data block",
            Query = new FullTableQuery { SubjectId = subject1.Id },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.AddAsync(releaseFile1);
            await contentDbContext.AddAsync(import1);
            await contentDbContext.AddRangeAsync(dataBlockVersion1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddAsync(subject1);
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

        var subject1 = new Subject { Id = Guid.NewGuid() };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.NOT_FOUND };

        var dataBlockVersion1 = new DataBlockVersion
        {
            Name = "Test data block",
            Query = new FullTableQuery { SubjectId = subject1.Id },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.AddAsync(releaseFile1);
            await contentDbContext.AddAsync(import1);
            await contentDbContext.AddRangeAsync(dataBlockVersion1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddAsync(subject1);
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

        var subject1 = new Subject { Id = Guid.NewGuid() };

        var releaseFile1 = new ReleaseFile
        {
            Name = "Data 1",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "data1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id,
            },
        };

        var import1 = new DataImport { File = releaseFile1.File, Status = DataImportStatus.COMPLETE };

        // Subject does not match
        var dataBlockVersion1 = new DataBlockVersion
        {
            Name = "Test data block",
            Query = new FullTableQuery { SubjectId = Guid.NewGuid() },
        };
        var featuredTable1 = new FeaturedTable
        {
            DataBlockVersion = dataBlockVersion1,
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
            await contentDbContext.DataBlockVersions.AddRangeAsync(dataBlockVersion1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.AddAsync(subject1);
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
        ITimePeriodService? timePeriodService = null
    )
    {
        return new ReleaseService(
            contentDbContext,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict)
        );
    }
}
