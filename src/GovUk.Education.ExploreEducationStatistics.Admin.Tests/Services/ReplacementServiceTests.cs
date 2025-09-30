#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Semver;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using IReleaseVersionService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionService;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReplacementServiceTests
{
    private readonly Country _england = new("E92000001", "England");
    private readonly LocalAuthority _derby = new("E06000015", "", "Derby");

    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Replace_ReplacementPlanInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };
        var table = new TableBuilderConfiguration
        {
            TableHeaders = new TableHeaders
            {
                ColumnGroups = new List<List<TableHeader>>(),
                Columns = new List<TableHeader>
                {
                    new("2019_CY", TableHeaderType.TimePeriod),
                    new("2020_CY", TableHeaderType.TimePeriod),
                },
                RowGroups = new List<List<TableHeader>>
                {
                    new()
                    {
                        TableHeader.NewLocationHeader(
                            GeographicLevel.Country,
                            originalLocation.Id.ToString()
                        ),
                    },
                },
                Rows = new List<TableHeader>(),
            },
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new Guid[] { },
                Indicators = new Guid[] { },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
            },
            Table = table,
            ReleaseVersion = releaseVersion,
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Location.AddRange(originalLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(locationRepository, timePeriodService);

            result.AssertBadRequest(ReplacementMustBeValid);
        }
    }

    [Fact]
    public async Task Replace_ReplacementImportNotComplete()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
            Summary = "Original data set guidance",
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
            Summary = null,
        };

        var originalFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem1 },
        };

        var originalFilterGroup2 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem2 },
        };

        var replacementFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem1 },
        };

        var replacementFilterGroup2 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem2 },
        };

        var originalFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup1 },
        };

        var originalFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup2 },
        };

        var replacementFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup1 },
        };

        var replacementFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup2 },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = _england,
            LocalAuthority = _derby,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalFilterItem1.Id, originalFilterItem2.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
            },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            TableHeader.NewLocationHeader(
                                GeographicLevel.LocalAuthority,
                                originalLocation.Id.ToString()
                            ),
                        },
                    },
                    Columns = new List<TableHeader>
                    {
                        new("2019_CY", TableHeaderType.TimePeriod),
                        new("2020_CY", TableHeaderType.TimePeriod),
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            new TableHeader(
                                originalFilterItem1.Id.ToString(),
                                TableHeaderType.Filter
                            ),
                            new TableHeader(
                                originalFilterItem2.Id.ToString(),
                                TableHeaderType.Filter
                            ),
                        },
                    },
                    Rows = new List<TableHeader>
                    {
                        new(originalIndicator.Id.ToString(), TableHeaderType.Indicator),
                    },
                },
            },
            Charts = new List<IChart>
            {
                new LineChart
                {
                    Axes = new Dictionary<string, ChartAxisConfiguration>
                    {
                        {
                            "major",
                            new ChartAxisConfiguration
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Filters = new List<Guid> { originalFilterItem1.Id },
                                        Indicator = originalIndicator.Id,
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = GeographicLevel
                                                .LocalAuthority.ToString()
                                                .CamelCase(),
                                            Value = originalLocation.Id,
                                        },
                                    },
                                },
                            }
                        },
                    },
                    Legend = new ChartLegend
                    {
                        Items = new List<ChartLegendItem>
                        {
                            new()
                            {
                                DataSet = new ChartBaseDataSet
                                {
                                    Filters = new List<Guid> { originalFilterItem1.Id },
                                    Indicator = originalIndicator.Id,
                                    Location = new ChartDataSetLocation
                                    {
                                        Level = GeographicLevel
                                            .LocalAuthority.ToString()
                                            .CamelCase(),
                                        Value = originalLocation.Id,
                                    },
                                },
                            },
                        },
                    },
                },
            },
            ReleaseVersion = releaseVersion,
        };

        var dataBlockVersion = new DataBlockVersion { Id = dataBlock.Id, ContentBlock = dataBlock };

        var footnoteForFilter = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter",
            filterFootnotes: new List<FilterFootnote> { new() { Filter = originalFilter1 } }
        );

        var footnoteForFilterGroup = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter group",
            filterGroupFootnotes: new List<FilterGroupFootnote>
            {
                new() { FilterGroup = originalFilterGroup1 },
            }
        );

        var footnoteForFilterItem = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            filterItemFootnotes: new List<FilterItemFootnote>
            {
                new() { FilterItem = originalFilterItem1 },
            }
        );

        var footnoteForIndicator = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote>
            {
                new() { Indicator = originalIndicator },
            }
        );

        var footnoteForSubject = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Subject",
            subject: originalReleaseSubject.Subject
        );

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location> { replacementLocation });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear),
                }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var dataImport = new DataImport
        {
            FileId = replacementFile.Id,
            Status = DataImportStatus.STAGE_2,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlockVersions.Add(dataBlockVersion);
            contentDbContext.DataImports.Add(dataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Filter.AddRange(
                originalFilter1,
                originalFilter2,
                replacementFilter1,
                replacementFilter2
            );
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroup,
                replacementIndicatorGroup
            );
            statisticsDbContext.Location.AddRange(originalLocation);
            statisticsDbContext.Footnote.AddRange(
                footnoteForFilter,
                footnoteForFilterGroup,
                footnoteForFilterItem,
                footnoteForIndicator,
                footnoteForSubject
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            result.AssertBadRequest(ReplacementImportMustBeComplete);

            VerifyAllMocks(locationRepository, timePeriodService);
        }
    }

    [Fact]
    public async Task Replace_LinkedFilesForReplacementNotFound()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();
        var originalFileId = Guid.NewGuid();

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFileId
                )
            )
            .ReturnsAsync(new NotFoundResult());

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementService = BuildReplacementService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFileId
            );

            result.AssertNotFound();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Replace_FileIsLinkedToPublicApiDataSet_SuccessIfFeatureFlagIsOnOrValidationProblemIfNot(
        bool enableReplacementOfPublicApiDataSets
    )
    {
        DataSet dataSet = _fixture.DefaultDataSet();

        DataSetVersion dataSetVersion = _fixture
            .DefaultDataSetVersion()
            .WithVersionNumber(major: 1, minor: 1, patch: 1)
            .WithDataSet(dataSet);

        Content.Model.ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        File replacementFile = _fixture
            .DefaultFile()
            .WithSubjectId(replacementReleaseSubject.SubjectId)
            .WithType(FileType.Data);

        File originalFile = _fixture
            .DefaultFile()
            .WithReplacedBy(replacementFile)
            .WithSubjectId(originalReleaseSubject.SubjectId)
            .WithType(FileType.Data);

        replacementFile.Replacing = originalFile;

        var (originalReleaseFile, replacementReleaseFile) = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(
                0,
                rv =>
                    rv.SetFile(originalFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion())
            )
            .ForIndex(
                1,
                rv =>
                {
                    rv.SetFile(replacementFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion());
                }
            )
            .GenerateTuple2();

        var replacementDataImport = _fixture
            .DefaultDataImport()
            .WithFile(replacementFile)
            .WithStatus(DataImportStatus.COMPLETE);

        var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);
        dataSetVersionService
            .Setup(mock =>
                mock.GetDataSetVersion(
                    originalReleaseFile.PublicApiDataSetId!.Value,
                    It.IsAny<SemVersion>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(dataSetVersion);

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var dataSetVersionMappingService = new Mock<IDataSetVersionMappingService>(Strict);
        dataSetVersionMappingService
            .Setup(service =>
                service.GetMappingStatus(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new MappingStatusViewModel
                {
                    FiltersComplete = true,
                    LocationsComplete = true,
                    HasDeletionChanges = false,
                    FiltersHaveMajorChange = false,
                    LocationsHaveMajorChange = false,
                }
            );
        var options = Microsoft.Extensions.Options.Options.Create(
            new FeatureFlagsOptions()
            {
                EnableReplacementOfPublicApiDataSets = enableReplacementOfPublicApiDataSets,
            }
        );

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        if (enableReplacementOfPublicApiDataSets)
        {
            releaseVersionService
                .Setup(service => service.RemoveDataFiles(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(Unit.Instance);
        }

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    dataSetVersionService: dataSetVersionService.Object,
                    dataSetVersionMappingService: dataSetVersionMappingService.Object,
                    featureFlags: options
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            if (enableReplacementOfPublicApiDataSets)
            {
                VerifyAllMocks(
                    locationRepository,
                    timePeriodService,
                    dataSetVersionService,
                    dataSetVersionMappingService
                );
                result.AssertRight();
            }
            else
            {
                VerifyAllMocks(locationRepository, timePeriodService, dataSetVersionService);
                result.AssertBadRequest(ReplacementMustBeValid);
            }
        }
    }

    [Fact]
    public async Task Replace()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
            Summary = "Original data set guidance",
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
            Summary = null,
        };

        var originalFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem1 },
        };

        var originalFilterGroup2 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem2 },
        };

        var replacementFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem1 },
        };

        var replacementFilterGroup2 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem2 },
        };

        var originalFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup1 },
        };

        var originalFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup2 },
        };

        var replacementFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup1 },
        };

        var replacementFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup2 },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = _england,
            LocalAuthority = _derby,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalFilterItem1.Id, originalFilterItem2.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
                FilterHierarchiesOptions = null, // it is null by default, but included to be visible to you, dear test reader
            },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            TableHeader.NewLocationHeader(
                                GeographicLevel.LocalAuthority,
                                originalLocation.Id.ToString()
                            ),
                        },
                    },
                    Columns = new List<TableHeader>
                    {
                        new("2019_CY", TableHeaderType.TimePeriod),
                        new("2020_CY", TableHeaderType.TimePeriod),
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            new TableHeader(
                                originalFilterItem1.Id.ToString(),
                                TableHeaderType.Filter
                            ),
                            new TableHeader(
                                originalFilterItem2.Id.ToString(),
                                TableHeaderType.Filter
                            ),
                        },
                    },
                    Rows = new List<TableHeader>
                    {
                        new(originalIndicator.Id.ToString(), TableHeaderType.Indicator),
                    },
                },
            },
            Charts = new List<IChart>
            {
                new LineChart
                {
                    Axes = new Dictionary<string, ChartAxisConfiguration>
                    {
                        {
                            "major",
                            new ChartAxisConfiguration
                            {
                                DataSets = new List<ChartDataSet>
                                {
                                    new()
                                    {
                                        Filters = new List<Guid> { originalFilterItem1.Id },
                                        Indicator = originalIndicator.Id,
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = GeographicLevel
                                                .LocalAuthority.ToString()
                                                .CamelCase(),
                                            Value = originalLocation.Id,
                                        },
                                    },
                                },
                            }
                        },
                    },
                    Legend = new ChartLegend
                    {
                        Items = new List<ChartLegendItem>
                        {
                            new()
                            {
                                DataSet = new ChartBaseDataSet
                                {
                                    Filters = new List<Guid> { originalFilterItem1.Id },
                                    Indicator = originalIndicator.Id,
                                    Location = new ChartDataSetLocation
                                    {
                                        Level = GeographicLevel
                                            .LocalAuthority.ToString()
                                            .CamelCase(),
                                        Value = originalLocation.Id,
                                    },
                                },
                            },
                        },
                    },
                },
            },
            ReleaseVersion = releaseVersion,
        };

        var dataBlockVersion = new DataBlockVersion { Id = dataBlock.Id, ContentBlock = dataBlock };

        var footnoteForFilter = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter",
            filterFootnotes: new List<FilterFootnote> { new() { Filter = originalFilter1 } }
        );

        var footnoteForFilterGroup = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter group",
            filterGroupFootnotes: new List<FilterGroupFootnote>
            {
                new() { FilterGroup = originalFilterGroup1 },
            }
        );

        var footnoteForFilterItem = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            filterItemFootnotes: new List<FilterItemFootnote>
            {
                new() { FilterItem = originalFilterItem1 },
            }
        );

        var footnoteForIndicator = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote>
            {
                new() { Indicator = originalIndicator },
            }
        );

        var footnoteForSubject = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Subject",
            subject: originalReleaseSubject.Subject
        );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location> { replacementLocation });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear),
                }
            );

        var replacementDataImport = new DataImport
        {
            File = replacementFile,
            Status = DataImportStatus.COMPLETE,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlockVersions.Add(dataBlockVersion);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Filter.AddRange(
                originalFilter1,
                originalFilter2,
                replacementFilter1,
                replacementFilter2
            );
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroup,
                replacementIndicatorGroup
            );
            statisticsDbContext.Location.AddRange(originalLocation);
            statisticsDbContext.Footnote.AddRange(
                footnoteForFilter,
                footnoteForFilterGroup,
                footnoteForFilterItem,
                footnoteForIndicator,
                footnoteForSubject
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        releaseVersionService
            .Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync(Unit.Instance);

        var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);
        var cacheKeyService = new Mock<ICacheKeyService>(Strict);
        cacheKeyService
            .Setup(service =>
                service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id)
            )
            .ReturnsAsync(cacheKey);

        var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
        privateBlobCacheService
            .Setup(service => service.DeleteItemAsync(cacheKey))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                privateBlobCacheService: privateBlobCacheService.Object,
                cacheKeyService: cacheKeyService.Object,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    releaseFileRepository: releaseFileRepository.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            result.AssertRight();

            VerifyAllMocks(
                privateBlobCacheService,
                cacheKeyService,
                locationRepository,
                releaseVersionService,
                timePeriodService
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            // Check that the original file was unlinked from the replacement before the mock call to remove it.
            var originalFileUpdated = await contentDbContext.Files.FindAsync(originalFile.Id);
            Assert.NotNull(originalFileUpdated);
            Assert.Null(originalFileUpdated.ReplacedById);

            // Check that the replacement file was unlinked from the original.
            var replacementFileUpdated = await contentDbContext.Files.FindAsync(replacementFile.Id);
            Assert.NotNull(replacementFileUpdated);
            Assert.Null(replacementFileUpdated.ReplacingId);

            var replacedDataBlock = await contentDbContext.DataBlocks.FirstAsync(db =>
                db.Id == dataBlock.Id
            );
            Assert.Equal(dataBlock.Name, replacedDataBlock.Name);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacedDataBlock.Query.SubjectId);

            Assert.Single(replacedDataBlock.Query.Indicators);
            Assert.Equal(replacementIndicator.Id, replacedDataBlock.Query.Indicators.First());

            var replacedFilterItemIds = replacedDataBlock.Query.GetFilterItemIds().ToList(); // all filterItems including those in FilterHierarchiesOptions
            Assert.Equal(2, replacedFilterItemIds.Count);
            Assert.Equal(2, replacedDataBlock.Query.GetNonHierarchicalFilterItemIds().Count()); // all filter items for the query are in `Filters` - there are no hierarchical filter items in the query
            Assert.Equal(replacementFilterItem1.Id, replacedFilterItemIds[0]);
            Assert.Equal(replacementFilterItem2.Id, replacedFilterItemIds[1]);

            Assert.Null(replacedDataBlock.Query.FilterHierarchiesOptions);

            var replacedLocationId = Assert.Single(replacedDataBlock.Query.LocationIds);
            Assert.Equal(replacementLocation.Id, replacedLocationId);

            Assert.NotNull(replacedDataBlock.Query.TimePeriod);
            timePeriod.AssertDeepEqualTo(replacedDataBlock.Query.TimePeriod);

            Assert.Equal(2, replacedDataBlock.Table.TableHeaders.Columns.Count);
            Assert.Equal(
                TableHeaderType.TimePeriod,
                replacedDataBlock.Table.TableHeaders.Columns.First().Type
            );
            Assert.Equal("2019_CY", replacedDataBlock.Table.TableHeaders.Columns.First().Value);
            Assert.Equal(
                TableHeaderType.TimePeriod,
                replacedDataBlock.Table.TableHeaders.Columns.ElementAt(1).Type
            );
            Assert.Equal(
                "2020_CY",
                replacedDataBlock.Table.TableHeaders.Columns.ElementAt(1).Value
            );
            Assert.Single(replacedDataBlock.Table.TableHeaders.ColumnGroups);
            Assert.Single(replacedDataBlock.Table.TableHeaders.ColumnGroups.First());
            Assert.Equal(
                TableHeaderType.Location,
                replacedDataBlock.Table.TableHeaders.ColumnGroups.First().First().Type
            );
            Assert.Equal(
                replacementLocation.Id.ToString(),
                replacedDataBlock.Table.TableHeaders.ColumnGroups.First().First().Value
            );
            Assert.Single(replacedDataBlock.Table.TableHeaders.Rows);
            Assert.Equal(
                TableHeaderType.Indicator,
                replacedDataBlock.Table.TableHeaders.Rows.First().Type
            );
            Assert.Equal(
                replacementIndicator.Id.ToString(),
                replacedDataBlock.Table.TableHeaders.Rows.First().Value
            );

            Assert.Single(replacedDataBlock.Table.TableHeaders.RowGroups);
            var replacementRowGroup = replacedDataBlock
                .Table.TableHeaders.RowGroups.First()
                .ToList();
            Assert.Equal(2, replacementRowGroup.Count);
            Assert.Equal(TableHeaderType.Filter, replacementRowGroup[0].Type);
            Assert.Equal(replacementFilterItem1.Id.ToString(), replacementRowGroup[0].Value);
            Assert.Equal(TableHeaderType.Filter, replacementRowGroup[1].Type);
            Assert.Equal(replacementFilterItem2.Id.ToString(), replacementRowGroup[1].Value);

            var chartMajorAxis = replacedDataBlock.Charts[0].Axes?["major"];
            Assert.NotNull(chartMajorAxis);
            var dataSet = Assert.Single(chartMajorAxis.DataSets);
            Assert.NotNull(dataSet);
            Assert.Single(dataSet.Filters);
            Assert.Equal(replacementFilterItem1.Id, dataSet.Filters[0]);
            Assert.Equal(replacementIndicator.Id, dataSet.Indicator);
            Assert.NotNull(dataSet.Location);
            Assert.Equal(replacementLocation.Id, dataSet.Location.Value);

            var chartLegendItems = replacedDataBlock.Charts[0].Legend?.Items;
            Assert.NotNull(chartLegendItems);
            var chartLegendItem = Assert.Single(chartLegendItems);
            Assert.NotNull(chartLegendItem);
            var filter = Assert.Single(chartLegendItem.DataSet.Filters);
            Assert.Equal(replacementFilterItem1.Id, filter);
            Assert.Equal(replacementIndicator.Id, chartLegendItem.DataSet.Indicator);
            Assert.NotNull(chartLegendItem.DataSet.Location);
            Assert.Equal(replacementLocation.Id, chartLegendItem.DataSet.Location.Value);

            var replacedFootnoteForFilter = await GetFootnoteById(
                statisticsDbContext,
                footnoteForFilter.Id
            );
            Assert.NotNull(replacedFootnoteForFilter);
            Assert.Equal(footnoteForFilter.Content, replacedFootnoteForFilter.Content);
            Assert.Single(replacedFootnoteForFilter.Filters);
            Assert.Empty(replacedFootnoteForFilter.FilterGroups);
            Assert.Empty(replacedFootnoteForFilter.FilterItems);
            Assert.Empty(replacedFootnoteForFilter.Indicators);
            Assert.Empty(replacedFootnoteForFilter.Subjects);

            Assert.Equal(
                replacementFilter1.Id,
                replacedFootnoteForFilter.Filters.First().Filter.Id
            );
            Assert.Equal(
                replacementFilter1.Label,
                replacedFootnoteForFilter.Filters.First().Filter.Label
            );
            Assert.Equal(
                replacementFilter1.Name,
                replacedFootnoteForFilter.Filters.First().Filter.Name
            );

            var replacedFootnoteForFilterGroup = await GetFootnoteById(
                statisticsDbContext,
                footnoteForFilterGroup.Id
            );
            Assert.NotNull(replacedFootnoteForFilterGroup);
            Assert.Equal(footnoteForFilterGroup.Content, replacedFootnoteForFilterGroup.Content);
            Assert.Single(replacedFootnoteForFilterGroup.FilterGroups);
            Assert.Empty(replacedFootnoteForFilterGroup.Filters);
            Assert.Single(replacedFootnoteForFilterGroup.FilterGroups);
            Assert.Empty(replacedFootnoteForFilterGroup.FilterItems);
            Assert.Empty(replacedFootnoteForFilterGroup.Indicators);
            Assert.Empty(replacedFootnoteForFilterGroup.Subjects);

            Assert.Equal(
                replacementFilterGroup1.Id,
                replacedFootnoteForFilterGroup.FilterGroups.First().FilterGroup.Id
            );
            Assert.Equal(
                replacementFilterGroup1.Label,
                replacedFootnoteForFilterGroup.FilterGroups.First().FilterGroup.Label
            );

            var replacedFootnoteForFilterItem = await GetFootnoteById(
                statisticsDbContext,
                footnoteForFilterItem.Id
            );
            Assert.NotNull(replacedFootnoteForFilterItem);
            Assert.Equal(footnoteForFilterItem.Content, replacedFootnoteForFilterItem.Content);
            Assert.Empty(replacedFootnoteForFilterItem.Filters);
            Assert.Empty(replacedFootnoteForFilterItem.FilterGroups);
            Assert.Single(replacedFootnoteForFilterItem.FilterItems);
            Assert.Empty(replacedFootnoteForFilterItem.Indicators);
            Assert.Empty(replacedFootnoteForFilterItem.Subjects);

            Assert.Equal(
                replacementFilterItem1.Id,
                replacedFootnoteForFilterItem.FilterItems.First().FilterItem.Id
            );
            Assert.Equal(
                replacementFilterItem1.Label,
                replacedFootnoteForFilterItem.FilterItems.First().FilterItem.Label
            );

            var replacedFootnoteForIndicator = await GetFootnoteById(
                statisticsDbContext,
                footnoteForIndicator.Id
            );
            Assert.NotNull(replacedFootnoteForIndicator);
            Assert.Equal(footnoteForIndicator.Content, replacedFootnoteForIndicator.Content);
            Assert.Empty(replacedFootnoteForIndicator.Filters);
            Assert.Empty(replacedFootnoteForIndicator.FilterGroups);
            Assert.Empty(replacedFootnoteForIndicator.FilterItems);
            Assert.Single(replacedFootnoteForIndicator.Indicators);
            Assert.Empty(replacedFootnoteForIndicator.Subjects);

            Assert.Equal(
                replacementIndicator.Id,
                replacedFootnoteForIndicator.Indicators.First().Indicator.Id
            );
            Assert.Equal(
                replacementIndicator.Label,
                replacedFootnoteForIndicator.Indicators.First().Indicator.Label
            );
            Assert.Equal(
                replacementIndicator.Name,
                replacedFootnoteForIndicator.Indicators.First().Indicator.Name
            );

            var replacedFootnoteForSubject = await GetFootnoteById(
                statisticsDbContext,
                footnoteForSubject.Id
            );
            Assert.NotNull(replacedFootnoteForSubject);
            Assert.Equal(footnoteForSubject.Content, replacedFootnoteForSubject.Content);
            Assert.Empty(replacedFootnoteForSubject.Filters);
            Assert.Empty(replacedFootnoteForSubject.FilterGroups);
            Assert.Empty(replacedFootnoteForSubject.FilterItems);
            Assert.Empty(replacedFootnoteForSubject.Indicators);
            Assert.Single(replacedFootnoteForSubject.Subjects);

            Assert.Equal(
                replacementReleaseSubject.SubjectId,
                replacedFootnoteForSubject.Subjects.First().Subject.Id
            );

            // Check the original data guidance has been retained on the replacement
            var updatedReleaseFile = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == replacementFile.Id
            );
            Assert.Equal("Original data set guidance", updatedReleaseFile.Summary);

            Assert.Null(updatedReleaseFile.FilterSequence);
            Assert.Null(updatedReleaseFile.IndicatorSequence);
        }
    }

    [Fact]
    public async Task Replace_ReplacesFilterHierarchiesOptions()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFilter1Id = Guid.NewGuid();
        var originalFilter2Id = Guid.NewGuid();

        var originalFilterItem1Id = Guid.NewGuid();
        var originalFilterItem2Id = Guid.NewGuid();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
            FilterHierarchies =
            [
                new DataSetFileFilterHierarchy(
                    FilterIds: [originalFilter1Id, originalFilter2Id],
                    Tiers:
                    [
                        new Dictionary<Guid, List<Guid>>
                        {
                            { originalFilterItem1Id, [originalFilterItem2Id] },
                        },
                    ]
                ),
            ],
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
            Summary = "Original data set guidance",
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
            Summary = null,
        };

        var originalFilter1 = new Filter
        {
            Id = originalFilter1Id,
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    Label = "Default group - not changing",
                    FilterItems = new List<FilterItem>
                    {
                        new()
                        {
                            Id = originalFilterItem1Id,
                            Label = "Test filter item - not changing",
                        },
                    },
                },
            },
        };

        var originalFilter2 = new Filter
        {
            Id = originalFilter2Id,
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    Label = "Default group - not changing",
                    FilterItems = new List<FilterItem>
                    {
                        new()
                        {
                            Id = originalFilterItem2Id,
                            Label = "Test filter item - not changing",
                        },
                    },
                },
            },
        };

        var replacementFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    Label = "Default group - not changing",
                    FilterItems = new List<FilterItem>
                    {
                        new() { Id = Guid.NewGuid(), Label = "Test filter item - not changing" },
                    },
                },
            },
        };

        var replacementFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    Label = "Default group - not changing",
                    FilterItems = new List<FilterItem>
                    {
                        new() { Id = Guid.NewGuid(), Label = "Test filter item - not changing" },
                    },
                },
            },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = _england,
            LocalAuthority = _derby,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = [],
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
                FilterHierarchiesOptions = new List<FilterHierarchyOptions>
                {
                    new()
                    {
                        LeafFilterId = originalFilter2.Id,
                        // This would actually be an invalid data set, as there should also be two
                        // additional Total filterItems for both filters in a filter hierarchy
                        Options =
                        [
                            [originalFilterItem1Id, originalFilterItem2Id],
                        ],
                    },
                },
            },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            TableHeader.NewLocationHeader(
                                GeographicLevel.LocalAuthority,
                                originalLocation.Id.ToString()
                            ),
                        },
                    },
                    Columns = new List<TableHeader>
                    {
                        new("2019_CY", TableHeaderType.TimePeriod),
                        new("2020_CY", TableHeaderType.TimePeriod),
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            new TableHeader(
                                originalFilterItem1Id.ToString(),
                                TableHeaderType.Filter
                            ),
                            new TableHeader(
                                originalFilterItem2Id.ToString(),
                                TableHeaderType.Filter
                            ),
                        },
                    },
                    Rows = new List<TableHeader>
                    {
                        new(originalIndicator.Id.ToString(), TableHeaderType.Indicator),
                    },
                },
            },
            Charts = [],
            ReleaseVersion = releaseVersion,
        };

        var dataBlockVersion = new DataBlockVersion { Id = dataBlock.Id, ContentBlock = dataBlock };

        var replacementDataImport = new DataImport
        {
            File = replacementFile,
            Status = DataImportStatus.COMPLETE,
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location> { replacementLocation });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear),
                }
            );

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlockVersions.AddRange(dataBlockVersion);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Filter.AddRange(
                originalFilter1,
                originalFilter2,
                replacementFilter1,
                replacementFilter2
            );
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroup,
                replacementIndicatorGroup
            );
            statisticsDbContext.Location.AddRange(originalLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        releaseVersionService
            .Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync(Unit.Instance);

        var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

        var cacheKeyService = new Mock<ICacheKeyService>(Strict);
        cacheKeyService
            .Setup(service =>
                service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id)
            )
            .ReturnsAsync(cacheKey);

        var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
        privateBlobCacheService
            .Setup(service => service.DeleteItemAsync(cacheKey))
            .Returns(Task.CompletedTask);

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                privateBlobCacheService: privateBlobCacheService.Object,
                cacheKeyService: cacheKeyService.Object,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(
                privateBlobCacheService,
                cacheKeyService,
                locationRepository,
                releaseVersionService,
                timePeriodService
            );

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Check that the original file was unlinked from the replacement before the mock call to remove it.
            var originalFileUpdated = await contentDbContext.Files.FindAsync(originalFile.Id);
            Assert.NotNull(originalFileUpdated);
            Assert.Null(originalFileUpdated.ReplacedById);

            // Check that the replacement file was unlinked from the original.
            var replacementFileUpdated = await contentDbContext.Files.FindAsync(replacementFile.Id);
            Assert.NotNull(replacementFileUpdated);
            Assert.Null(replacementFileUpdated.ReplacingId);

            var replacedDataBlock = await contentDbContext.DataBlocks.FirstAsync(db =>
                db.Id == dataBlock.Id
            );
            Assert.Equal(dataBlock.Name, replacedDataBlock.Name);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacedDataBlock.Query.SubjectId);

            Assert.Single(replacedDataBlock.Query.Indicators);
            Assert.Equal(replacementIndicator.Id, replacedDataBlock.Query.Indicators.First());

            var replacedLocationId = Assert.Single(replacedDataBlock.Query.LocationIds);
            Assert.Equal(replacementLocation.Id, replacedLocationId);

            Assert.NotNull(replacedDataBlock.Query.TimePeriod);
            timePeriod.AssertDeepEqualTo(replacedDataBlock.Query.TimePeriod);

            Assert.Empty(replacedDataBlock.Query.GetNonHierarchicalFilterItemIds());

            var hierarchiesOptions = replacedDataBlock.Query.FilterHierarchiesOptions;
            Assert.NotNull(hierarchiesOptions);

            var hierarchyOptions = Assert.Single(hierarchiesOptions);

            Assert.Equal(replacementFilter2.Id, hierarchyOptions.LeafFilterId);
            Assert.Equal(
                [
                    [
                        replacementFilter1.FilterGroups[0].FilterItems[0].Id,
                        replacementFilter2.FilterGroups[0].FilterItems[0].Id,
                    ],
                ],
                hierarchyOptions.Options
            );
        }
    }

    [Fact]
    public async Task Replace_MapChart_ReplacesChartDataSetConfigs()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
        };

        var originalFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem1 },
        };

        var originalFilterGroup2 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem2 },
        };

        var replacementFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem1 },
        };

        var replacementFilterGroup2 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem2 },
        };

        var originalFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup1 },
        };

        var originalFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup2 },
        };

        var replacementFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup1 },
        };

        var replacementFilter2 = new Filter
        {
            Label = "Test filter 2 - not changing",
            Name = "test_filter_2_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup2 },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = _england,
            LocalAuthority = _derby,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalFilterItem1.Id, originalFilterItem2.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
            },
            Table = new TableBuilderConfiguration(),
            Charts = new List<IChart>
            {
                new MapChart
                {
                    Axes = new Dictionary<string, ChartAxisConfiguration>(),
                    Map = new MapChartConfig
                    {
                        DataSetConfigs = new List<ChartDataSetConfig>
                        {
                            new()
                            {
                                DataSet = new ChartBaseDataSet
                                {
                                    Filters = new List<Guid>
                                    {
                                        originalFilterItem1.Id,
                                        originalFilterItem2.Id,
                                    },
                                    Indicator = originalIndicator.Id,
                                    Location = new ChartDataSetLocation
                                    {
                                        Level = GeographicLevel
                                            .LocalAuthority.ToString()
                                            .CamelCase(),
                                        Value = originalLocation.Id,
                                    },
                                },
                            },
                        },
                    },
                },
            },
            ReleaseVersion = releaseVersion,
        };

        var dataBlockVersion = new DataBlockVersion { Id = dataBlock.Id, ContentBlock = dataBlock };

        var replacementDataImport = new DataImport
        {
            File = replacementFile,
            Status = DataImportStatus.COMPLETE,
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location> { replacementLocation });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear),
                }
            );

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Filter.AddRange(
                originalFilter1,
                originalFilter2,
                replacementFilter1,
                replacementFilter2
            );
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroup,
                replacementIndicatorGroup
            );
            statisticsDbContext.Location.AddRange(originalLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

        var cacheKeyService = new Mock<ICacheKeyService>(Strict);
        cacheKeyService
            .Setup(service =>
                service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id)
            )
            .ReturnsAsync(cacheKey);

        var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
        privateBlobCacheService
            .Setup(service => service.DeleteItemAsync(cacheKey))
            .Returns(Task.CompletedTask);

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        releaseVersionService
            .Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync(Unit.Instance);

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                privateBlobCacheService: privateBlobCacheService.Object,
                cacheKeyService: cacheKeyService.Object,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            result.AssertRight();

            VerifyAllMocks(
                privateBlobCacheService,
                cacheKeyService,
                locationRepository,
                releaseVersionService,
                timePeriodService
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var replacedDataBlock = await contentDbContext.DataBlocks.FirstAsync(db =>
                db.Id == dataBlock.Id
            );

            var mapChart = Assert.IsType<MapChart>(replacedDataBlock.Charts[0]);

            var chartDataSetConfigs = mapChart.Map.DataSetConfigs;
            Assert.NotNull(chartDataSetConfigs);
            var chartDataSetConfig = Assert.Single(chartDataSetConfigs);

            Assert.Equal(2, chartDataSetConfig.DataSet.Filters.Count);
            Assert.Equal(replacementFilterItem1.Id, chartDataSetConfig.DataSet.Filters[0]);
            Assert.Equal(replacementFilterItem2.Id, chartDataSetConfig.DataSet.Filters[1]);

            Assert.Equal(replacementIndicator.Id, chartDataSetConfig.DataSet.Indicator);

            Assert.NotNull(chartDataSetConfig.DataSet.Location);
            Assert.Equal(replacementLocation.Id, chartDataSetConfig.DataSet.Location!.Value);
        }
    }

    [Fact]
    public async Task Replace_MapChart_ReplacesChartDataSetConfigsWithNullLocation()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
        };

        var originalFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var replacementFilterItem1 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem1 },
        };

        var replacementFilterGroup1 = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem1 },
        };

        var originalFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup1 },
        };

        var replacementFilter1 = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup1 },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = _england,
            LocalAuthority = _derby,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalFilterItem1.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = new List<Guid>(),
                TimePeriod = timePeriod,
            },
            Table = new TableBuilderConfiguration(),
            Charts = new List<IChart>
            {
                new MapChart
                {
                    Axes = new Dictionary<string, ChartAxisConfiguration>(),
                    Map = new MapChartConfig
                    {
                        DataSetConfigs = new List<ChartDataSetConfig>
                        {
                            new()
                            {
                                DataSet = new ChartBaseDataSet
                                {
                                    Filters = new List<Guid> { originalFilterItem1.Id },
                                    Indicator = originalIndicator.Id,
                                    Location = null,
                                },
                            },
                        },
                    },
                },
            },
            ReleaseVersion = releaseVersion,
        };

        var dataBlockVersion = new DataBlockVersion { Id = dataBlock.Id, ContentBlock = dataBlock };

        var replacementDataImport = new DataImport
        {
            File = replacementFile,
            Status = DataImportStatus.COMPLETE,
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location> { replacementLocation });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear),
                }
            );

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Filter.AddRange(originalFilter1, replacementFilter1);
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroup,
                replacementIndicatorGroup
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

        var cacheKeyService = new Mock<ICacheKeyService>(Strict);
        cacheKeyService
            .Setup(service =>
                service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id)
            )
            .ReturnsAsync(cacheKey);

        var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
        privateBlobCacheService
            .Setup(service => service.DeleteItemAsync(cacheKey))
            .Returns(Task.CompletedTask);

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        releaseVersionService
            .Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync(Unit.Instance);

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                privateBlobCacheService: privateBlobCacheService.Object,
                cacheKeyService: cacheKeyService.Object,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(
                privateBlobCacheService,
                cacheKeyService,
                locationRepository,
                releaseVersionService,
                timePeriodService
            );

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var replacedDataBlock = await contentDbContext.DataBlocks.SingleAsync(db =>
                db.Id == dataBlock.Id
            );

            var mapChart = Assert.IsType<MapChart>(replacedDataBlock.Charts[0]);

            var chartDataSetConfigs = mapChart.Map.DataSetConfigs;
            Assert.NotNull(chartDataSetConfigs);
            var chartDataSetConfig = Assert.Single(chartDataSetConfigs);

            var filterId = Assert.Single(chartDataSetConfig.DataSet.Filters);
            Assert.Equal(replacementFilterItem1.Id, filterId);

            Assert.Equal(replacementIndicator.Id, chartDataSetConfig.DataSet.Indicator);

            Assert.Null(chartDataSetConfig.DataSet.Location);
        }
    }

    [Fact]
    public async Task Replace_FilterSequenceIsReplaced()
    {
        // Basic test replacing a filter sequence, exercising the service with in-memory data and dependencies.
        // See ReplaceServiceHelperTests.ReplaceFilterSequence for a more comprehensive test of the actual replacement.

        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile,
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile,
        };

        // Define a set of filters, filter groups and filter items belonging to the original subject
        var originalFilters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter a",
                Name = "filter_a",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group a",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item a" },
                            new() { Id = Guid.NewGuid(), Label = "Item b" },
                        },
                    },
                },
            },
        };

        // Define a sequence for the original subject which is expected to be updated after the replacement
        originalReleaseFile.FilterSequence = new List<FilterSequenceEntry>
        {
            // Filter a
            new(
                originalFilters[0].Id,
                new List<FilterGroupSequenceEntry>
                {
                    // Group a
                    new(
                        originalFilters[0].FilterGroups[0].Id,
                        new List<Guid>
                        {
                            // Item b, Indicator a
                            originalFilters[0].FilterGroups[0].FilterItems[1].Id,
                            originalFilters[0].FilterGroups[0].FilterItems[0].Id,
                        }
                    ),
                }
            ),
        };

        // Define the set of filters, filter groups and filter items belonging to the replacement subject
        var replacementFilters = new List<Filter>
        {
            // 'Filter a' is identical
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter a",
                Name = "filter_a",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    // 'Group a' is identical
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group a",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item a" },
                            new() { Id = Guid.NewGuid(), Label = "Item b" },
                        },
                    },
                },
            },
        };

        var replacementDataImport = new DataImport
        {
            File = replacementFile,
            Status = DataImportStatus.COMPLETE,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.Filter.AddRange(originalFilters);
            statisticsDbContext.Filter.AddRange(replacementFilters);
            await statisticsDbContext.SaveChangesAsync();
        }

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        releaseVersionService
            .Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync(Unit.Instance);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            result.AssertRight();

            VerifyAllMocks(locationRepository, releaseVersionService, timePeriodService);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var replacedReleaseFile = await contentDbContext.ReleaseFiles.SingleAsync(rf =>
                rf.ReleaseVersionId == statsReleaseVersion.Id
                && rf.File.SubjectId == replacementReleaseSubject.SubjectId
            );

            // Verify the updated sequence of filters on the replacement subject
            var updatedSequence = replacedReleaseFile.FilterSequence;
            Assert.NotNull(updatedSequence);

            // 'Filter a' should be the only filter in the sequence
            var filterA = Assert.Single(updatedSequence);
            Assert.Equal(replacementFilters[0].Id, filterA.Id);
            var filterAGroups = filterA.ChildSequence;

            // 'Group a' should be the only group in the sequence
            var filterAGroupA = Assert.Single(filterAGroups);
            Assert.Equal(replacementFilters[0].FilterGroups[0].Id, filterAGroupA.Id);

            // 'Group a' should still have two filter items in the same order as the original sequence
            Assert.Equal(2, filterAGroupA.ChildSequence.Count);
            Assert.Equal(
                replacementFilters[0].FilterGroups[0].FilterItems[1].Id,
                filterAGroupA.ChildSequence[0]
            );
            Assert.Equal(
                replacementFilters[0].FilterGroups[0].FilterItems[0].Id,
                filterAGroupA.ChildSequence[1]
            );
        }
    }

    [Fact]
    public async Task Replace_IndicatorSequenceIsReplaced()
    {
        // Basic test replacing an indicator sequence, exercising the service with in-memory data and dependencies.
        // See ReplaceServiceHelperTests.ReplaceIndicatorSequence for a more comprehensive test of the actual replacement.

        var contentReleaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(contentReleaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = originalFile,
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = replacementFile,
        };

        // Define a set of indicator groups and indicators belonging to the original subject
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b",
                    },
                },
            },
        };

        // Define a sequence for the original subject which is expected to be updated after the replacement
        originalReleaseFile.IndicatorSequence = new List<IndicatorGroupSequenceEntry>
        {
            // Group a
            new(
                originalGroups[0].Id,
                new List<Guid>
                {
                    // Indicator b, Indicator a
                    originalGroups[0].Indicators[1].Id,
                    originalGroups[0].Indicators[0].Id,
                }
            ),
        };

        // Define the set of indicator groups and indicators belonging to the replacement subject
        var replacementGroups = new List<IndicatorGroup>
        {
            // 'Group a' is identical
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b",
                    },
                },
            },
        };

        var replacementDataImport = new DataImport
        {
            File = replacementFile,
            Status = DataImportStatus.COMPLETE,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(contentReleaseVersion);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataImports.Add(replacementDataImport);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            statisticsDbContext.IndicatorGroup.AddRange(originalGroups);
            statisticsDbContext.IndicatorGroup.AddRange(replacementGroups);
            statisticsDbContext.ReleaseSubject.AddRange(
                originalReleaseSubject,
                replacementReleaseSubject
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository
            .Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    contentReleaseVersion.Id,
                    originalFile.Id
                )
            )
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
        releaseVersionService
            .Setup(service => service.RemoveDataFiles(contentReleaseVersion.Id, originalFile.Id))
            .ReturnsAsync(Unit.Instance);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var filterRepository = new FilterRepository(statisticsDbContext);
            var replacementService = BuildReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterRepository: filterRepository,
                releaseVersionService: releaseVersionService.Object,
                releaseFileRepository: releaseFileRepository.Object,
                replacementPlanService: BuildReplacementPlanService(
                    contentDbContext,
                    statisticsDbContext,
                    filterRepository: filterRepository,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                )
            );

            var result = await replacementService.Replace(
                releaseVersionId: contentReleaseVersion.Id,
                originalFileId: originalFile.Id
            );

            result.AssertRight();

            VerifyAllMocks(locationRepository, releaseVersionService, timePeriodService);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var replacedReleaseFile = await contentDbContext.ReleaseFiles.SingleAsync(rf =>
                rf.ReleaseVersionId == statsReleaseVersion.Id
                && rf.File.SubjectId == replacementReleaseSubject.SubjectId
            );

            // Verify the updated sequence of indicators on the replacement subject
            var updatedSequence = replacedReleaseFile.IndicatorSequence;
            Assert.NotNull(updatedSequence);

            // 'Group a' should be the only group in the sequence
            var groupA = Assert.Single(updatedSequence);
            Assert.Equal(replacementGroups[0].Id, groupA.Id);

            // 'Group a' should still have two indicators in the same order as the original sequence
            Assert.Equal(2, groupA.ChildSequence.Count);
            Assert.Equal(replacementGroups[0].Indicators[1].Id, groupA.ChildSequence[0]);
            Assert.Equal(replacementGroups[0].Indicators[0].Id, groupA.ChildSequence[1]);
        }
    }

    private static Footnote CreateFootnote(
        ReleaseVersion releaseVersion,
        string content,
        List<FilterFootnote>? filterFootnotes = null,
        List<FilterGroupFootnote>? filterGroupFootnotes = null,
        List<FilterItemFootnote>? filterItemFootnotes = null,
        List<IndicatorFootnote>? indicatorFootnotes = null,
        Subject? subject = null
    )
    {
        return new Footnote
        {
            Content = content,
            Filters = filterFootnotes ?? new List<FilterFootnote>(),
            FilterGroups = filterGroupFootnotes ?? new List<FilterGroupFootnote>(),
            FilterItems = filterItemFootnotes ?? new List<FilterItemFootnote>(),
            Indicators = indicatorFootnotes ?? new List<IndicatorFootnote>(),
            Subjects =
                subject != null
                    ? new List<SubjectFootnote> { new() { Subject = subject } }
                    : new List<SubjectFootnote>(),
            Releases = new List<ReleaseFootnote> { new() { ReleaseVersion = releaseVersion } },
        };
    }

    private static async Task<Footnote> GetFootnoteById(StatisticsDbContext context, Guid id)
    {
        return await context
            .Footnote.Include(footnote => footnote.Filters)
            .ThenInclude(filterFootnote => filterFootnote.Filter)
            .Include(footnote => footnote.FilterGroups)
            .ThenInclude(filterGroupFootnote => filterGroupFootnote.FilterGroup)
            .Include(footnote => footnote.FilterItems)
            .ThenInclude(filterItemFootnote => filterItemFootnote.FilterItem)
            .Include(footnote => footnote.Indicators)
            .ThenInclude(indicatorFootnote => indicatorFootnote.Indicator)
            .Include(footnote => footnote.Subjects)
            .ThenInclude(subjectFootnote => subjectFootnote.Subject)
            .SingleAsync(footnote => footnote.Id == id);
    }

    private static ReplacementPlanService BuildReplacementPlanService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IFilterRepository? filterRepository = null,
        ILocationRepository? locationRepository = null,
        IDataSetVersionService? dataSetVersionService = null,
        ITimePeriodService? timePeriodService = null,
        IDataSetVersionMappingService? dataSetVersionMappingService = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IOptions<FeatureFlagsOptions>? featureFlags = null
    )
    {
        featureFlags ??= Microsoft.Extensions.Options.Options.Create(
            new FeatureFlagsOptions() { EnableReplacementOfPublicApiDataSets = false }
        );

        return new ReplacementPlanService(
            contentDbContext,
            statisticsDbContext,
            filterRepository ?? Mock.Of<IFilterRepository>(Strict),
            new IndicatorRepository(statisticsDbContext),
            locationRepository ?? Mock.Of<ILocationRepository>(Strict),
            new FootnoteRepository(statisticsDbContext),
            dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict),
            AlwaysTrueUserService().Object,
            dataSetVersionMappingService ?? Mock.Of<IDataSetVersionMappingService>(Strict),
            releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(Strict),
            featureFlags
        );
    }

    private static ReplacementService BuildReplacementService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IFilterRepository? filterRepository = null,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IReplacementPlanService? replacementPlanService = null,
        ICacheKeyService? cacheKeyService = null,
        IPrivateBlobCacheService? privateBlobCacheService = null
    )
    {
        return new ReplacementService(
            contentDbContext,
            statisticsDbContext,
            filterRepository ?? new FilterRepository(statisticsDbContext),
            new IndicatorGroupRepository(statisticsDbContext),
            releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(Strict),
            replacementPlanService ?? Mock.Of<IReplacementPlanService>(Strict),
            cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict),
            privateBlobCacheService ?? Mock.Of<IPrivateBlobCacheService>(Strict)
        );
    }
}
