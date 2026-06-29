#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class DataSetMappingServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_GenerateMapping_Indicators_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalSubjectId = Guid.NewGuid();
        var replacementSubjectId = Guid.NewGuid();

        var originalFile = new File
        {
            Id = originalDataFileId,
            SubjectId = originalSubjectId,
            Type = FileType.Data,
        };

        var replacementFile = new File
        {
            Id = replacementDataFileId,
            SubjectId = replacementSubjectId,
            Type = FileType.Data,
            Replacing = originalFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            await contentDbContext.SaveChangesAsync();
        }

        var originalIndicatorA1 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A1 - to be removed",
            Name = "indicator_a1",
        };

        var originalIndicatorA2 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A2",
            Name = "indicator_a2",
        };

        var originalIndicatorA4 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A4 - will move to a different group and so not be automapped",
            Name = "indicator_a4",
        };

        // No original A3, as it will be a new indicator in the replacement

        var originalIndicatorGroupA = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group A",
            SubjectId = originalSubjectId,
            Indicators = [originalIndicatorA1, originalIndicatorA2, originalIndicatorA4],
        };

        var replacementIndicatorA2 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A2",
            Name = "indicator_a2",
        };

        var replacementIndicatorA3 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A3 - new",
            Name = "indicator_a3",
        };

        var replacementIndicatorGroupA = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group A",
            SubjectId = replacementSubjectId,
            Indicators = [replacementIndicatorA2, replacementIndicatorA3],
        };

        var replacementIndicatorA4 = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A4 - moved from Group A to new Group B",
            Name = "indicator_a4",
        };

        var replacementIndicatorGroupB = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group B",
            SubjectId = replacementSubjectId,
            Indicators = [replacementIndicatorA4],
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroupA,
                replacementIndicatorGroupA,
                replacementIndicatorGroupB
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var service = BuildDataSetMappingService(contentDbContextId, statisticsDbContextId);

        await service.CreateInitialDataSetMappingIfReplacement(replacementFileId: replacementDataFileId);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = contentDbContext.DataSetMappings.Single(m =>
                m.OriginalDataFileId == originalDataFileId && m.ReplacementDataFileId == replacementDataFileId
            );

            var expectedMapping = new DataSetMapping
            {
                OriginalDataFileId = originalDataFileId,
                ReplacementDataFileId = replacementDataFileId,
                IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
                {
                    {
                        originalIndicatorA1.Id,
                        CreateIndicatorMapping(originalIndicatorA1, originalIndicatorGroupA, mapStatus: MapStatus.Unset)
                    },
                    {
                        originalIndicatorA2.Id,
                        CreateIndicatorMapping(
                            originalIndicatorA2,
                            originalIndicatorGroupA,
                            replacementIndicatorA2,
                            replacementIndicatorGroupA,
                            mapStatus: MapStatus.AutoSet
                        )
                    },
                    {
                        originalIndicatorA4.Id,
                        CreateIndicatorMapping(originalIndicatorA4, originalIndicatorGroupA, mapStatus: MapStatus.Unset)
                    },
                },
                UnmappedReplacementIndicators =
                [
                    new UnmappedIndicator
                    {
                        Id = replacementIndicatorA3.Id,
                        Label = replacementIndicatorA3.Label,
                        ColumnName = replacementIndicatorA3.Name,
                        GroupId = replacementIndicatorGroupA.Id,
                        GroupLabel = replacementIndicatorGroupA.Label,
                    },
                    new UnmappedIndicator
                    {
                        Id = replacementIndicatorA4.Id,
                        Label = replacementIndicatorA4.Label,
                        ColumnName = replacementIndicatorA4.Name,
                        GroupId = replacementIndicatorGroupB.Id,
                        GroupLabel = replacementIndicatorGroupB.Label,
                    },
                ],
            };

            result.AssertDeepEqualTo(expectedMapping, ignoreProperties: [mapping => mapping.Id]);
        }
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementHasDifferentLocation_LocationMatchedByCode()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Test filter item - not changing" };

        var replacementFilterItem = new FilterItem { Label = "Test filter item - not changing" };

        var originalFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalFilterItem },
        };

        var replacementFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementFilterItem },
        };

        var originalFilter = new Filter
        {
            Label = "Filter - not changing",
            Name = "filter_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup },
        };

        var replacementFilter = new Filter
        {
            Label = "Filter - not changing",
            Name = "filter_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup },
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

        Country england = new("E92000001", "England");
        LocalAuthority derby = new("E06000015", "", "Derby");

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = derby,
        };
        var observationForOriginalLocation = new Observation
        {
            SubjectId = originalReleaseSubject.SubjectId,
            Location = originalLocation,
        };

        // Replacement location has a different id but the primary attribute code remains the same
        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = england,
            LocalAuthority = derby,
        };
        var observationForReplacementLocation = new Observation
        {
            SubjectId = replacementReleaseSubject.SubjectId,
            Location = replacementLocation,
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
                Filters = new[] { originalFilterItem.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = [originalLocation.Id],
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(originalLocation, replacementLocation);
            statisticsDbContext.Observation.AddRange(observationForOriginalLocation, observationForReplacementLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        var dataImportService = BuildDataSetMappingService(contentDbContextId, statisticsDbContextId);

        await dataImportService.CreateInitialDataSetMappingIfReplacement(replacementFile.Id);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var dbDataSetMapping = contentDbContext.DataSetMappings.Single(m =>
                m.OriginalDataFileId == originalFile.Id && m.ReplacementDataFileId == replacementFile.Id
            );

            Assert.Equal(
                new Dictionary<Guid, LocationMapping>
                {
                    {
                        originalLocation.Id,
                        CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                    },
                },
                dbDataSetMapping.LocationMappings
            );

            Assert.Empty(dbDataSetMapping.UnmappedReplacementLocations);
        }
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_NoMappingCreatedIfNotReplacement_Success()
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            Replacing = null, // a DataSetMapping is only created if a replacement is ongoing
            Type = FileType.Data,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Add(file);
            await contentDbContext.SaveChangesAsync();
        }

        var statisticsDbContextId = Guid.NewGuid().ToString();
        var service = BuildDataSetMappingService(
            contentDbContextId: contentDbContextId,
            statisticsDbContextId: statisticsDbContextId
        );

        await service.CreateInitialDataSetMappingIfReplacement(replacementFileId: file.Id);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.False(contentDbContext.DataSetMappings.Any());
        }
    }

    private static DataSetMappingService BuildDataSetMappingService(
        string? contentDbContextId = null,
        string? statisticsDbContextId = null
    )
    {
        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId ?? Guid.NewGuid().ToString(),
            statisticsDbContextId ?? Guid.NewGuid().ToString()
        );

        return new DataSetMappingService(dbContextSupplier);
    }

    private static IndicatorMapping CreateIndicatorMapping(
        Indicator original,
        IndicatorGroup originalGroup,
        Indicator? replacement = null,
        IndicatorGroup? replacementGroup = null,
        MapStatus mapStatus = MapStatus.Unset
    )
    {
        return new IndicatorMapping
        {
            OriginalId = original.Id,
            OriginalColumnName = original.Name,
            OriginalLabel = original.Label,
            OriginalGroupId = originalGroup.Id,
            OriginalGroupLabel = originalGroup.Label,
            ReplacementId = replacement?.Id,
            ReplacementColumnName = replacement?.Name,
            ReplacementLabel = replacement?.Label,
            ReplacementGroupId = replacementGroup?.Id,
            ReplacementGroupLabel = replacementGroup?.Label,
            Status = mapStatus,
        };
    }

    private static LocationMapping CreateLocationMapping(
        Location original,
        Location? replacement = null,
        MapStatus status = MapStatus.Unset
    )
    {
        return new LocationMapping
        {
            OriginalId = original.Id,
            OriginalCode = original.ToLocationAttribute().Code!,
            OriginalName = original.ToLocationAttribute().Name!,
            OriginalGeographicLevel = original.GeographicLevel,
            ReplacementId = replacement?.Id,
            ReplacementCode = replacement?.ToLocationAttribute().Code!,
            ReplacementName = replacement?.ToLocationAttribute().Name!,
            ReplacementGeographicLevel = replacement?.GeographicLevel,
            Status = status,
        };
    }
}
