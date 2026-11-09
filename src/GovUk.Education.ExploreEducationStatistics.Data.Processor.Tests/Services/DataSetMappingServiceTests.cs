#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class DataSetMappingServiceTests
{
    private static readonly Country England = new("E92000001", "England");
    private static readonly LocalAuthority Derby = new("E06000015", "", "Derby");

    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_GenerateMapping_Filters_Success()
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

        var originalFilterItem1A1 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 1A1" };
        var originalFilterItem1A2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Item 1A2", // no matching replacement
        };
        var originalFilterGroup1A = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group 1A",
            FilterItems = [originalFilterItem1A1, originalFilterItem1A2],
        };

        var originalFilterItem1B1 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 1B1" };
        var originalFilterGroup1B = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group 1B", // no matching replacement
            FilterItems = [originalFilterItem1B1],
        };

        var originalFilter1 = new Filter
        {
            Id = Guid.NewGuid(),
            Name = "filter1",
            Label = "Filter 1",
            FilterGroups = [originalFilterGroup1A, originalFilterGroup1B],
            SubjectId = originalFile.SubjectId!.Value,
        };

        var originalFilterItem2A1 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 2A1" };
        var originalFilterGroup2A = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group 2A",
            FilterItems = [originalFilterItem2A1],
        };
        var originalFilter2 = new Filter
        {
            Id = Guid.NewGuid(),
            Name = "filter2", // no matching replacement
            Label = "Filter 2",
            FilterGroups = [originalFilterGroup2A],
            SubjectId = originalFile.SubjectId!.Value,
        };

        var replacementFilterItem1A1 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 1A1" };
        var replacementFilterItem1A2 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 1A2 updated" };
        var replacementFilterGroup1A = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group 1A",
            FilterItems = [replacementFilterItem1A1, replacementFilterItem1A2],
        };

        var replacementFilterItem1B1 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 1B1" };
        var replacementFilterGroup1B = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group 1B updated",
            FilterItems = [replacementFilterItem1B1],
        };

        var replacementFilter1 = new Filter
        {
            Id = Guid.NewGuid(),
            Name = "filter1",
            Label = "Filter 1",
            FilterGroups = [replacementFilterGroup1A, replacementFilterGroup1B],
            SubjectId = replacementFile.SubjectId!.Value,
        };

        var replacementFilterItem2A1 = new FilterItem { Id = Guid.NewGuid(), Label = "Item 2A1" };
        var replacementFilterGroup2A = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Group 2A",
            FilterItems = [replacementFilterItem2A1],
        };
        var replacementFilter2 = new Filter
        {
            Id = Guid.NewGuid(),
            Name = "filter2-updated",
            Label = "Filter 2 updated",
            FilterGroups = [replacementFilterGroup2A],
            SubjectId = replacementFile.SubjectId!.Value,
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.Filter.AddRange(
                originalFilter1,
                originalFilter2,
                replacementFilter1,
                replacementFilter2
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
                FilterMappings = new Dictionary<Guid, FilterMapping>
                {
                    {
                        originalFilter1.Id,
                        CreateFilterMapping(
                            original: originalFilter1,
                            replacement: replacementFilter1,
                            status: MapStatus.AutoSet,
                            filterGroupMappings: new Dictionary<Guid, FilterGroupMapping>
                            {
                                {
                                    originalFilterGroup1A.Id,
                                    CreateFilterGroupMapping(
                                        original: originalFilterGroup1A,
                                        replacement: replacementFilterGroup1A,
                                        status: MapStatus.AutoSet,
                                        filterItemMappings: new Dictionary<Guid, FilterItemMapping>
                                        {
                                            {
                                                originalFilterItem1A1.Id,
                                                new FilterItemMapping
                                                {
                                                    OriginalId = originalFilterItem1A1.Id,
                                                    OriginalLabel = originalFilterItem1A1.Label,
                                                    ReplacementId = replacementFilterItem1A1.Id,
                                                    ReplacementLabel = replacementFilterItem1A1.Label,
                                                    Status = MapStatus.AutoSet,
                                                }
                                            },
                                            {
                                                originalFilterItem1A2.Id,
                                                new FilterItemMapping
                                                {
                                                    OriginalId = originalFilterItem1A2.Id,
                                                    OriginalLabel = originalFilterItem1A2.Label,
                                                    Status = MapStatus.Unset,
                                                }
                                            },
                                        }
                                    )
                                },
                                {
                                    originalFilterGroup1B.Id,
                                    CreateFilterGroupMapping(
                                        original: originalFilterGroup1B,
                                        status: MapStatus.Unset,
                                        filterItemMappings: new Dictionary<Guid, FilterItemMapping>
                                        {
                                            {
                                                originalFilterItem1B1.Id,
                                                new FilterItemMapping
                                                {
                                                    OriginalId = originalFilterItem1B1.Id,
                                                    OriginalLabel = originalFilterItem1B1.Label,
                                                    Status = MapStatus.ParentNotMapped,
                                                }
                                            },
                                        }
                                    )
                                },
                            }
                        )
                    },
                    {
                        originalFilter2.Id,
                        CreateFilterMapping(
                            original: originalFilter2,
                            status: MapStatus.Unset,
                            filterGroupMappings: new Dictionary<Guid, FilterGroupMapping>
                            {
                                {
                                    originalFilterGroup2A.Id,
                                    CreateFilterGroupMapping(
                                        original: originalFilterGroup2A,
                                        status: MapStatus.ParentNotMapped,
                                        filterItemMappings: new Dictionary<Guid, FilterItemMapping>
                                        {
                                            {
                                                originalFilterItem2A1.Id,
                                                new FilterItemMapping
                                                {
                                                    OriginalId = originalFilterItem2A1.Id,
                                                    OriginalLabel = originalFilterItem2A1.Label,
                                                    Status = MapStatus.ParentNotMapped,
                                                }
                                            },
                                        }
                                    )
                                },
                            }
                        )
                    },
                },
            };
            result.AssertDeepEqualTo(expectedMapping, ignoreProperties: [mapping => mapping.Id]);
        }
    }

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
            };

            result.AssertDeepEqualTo(expectedMapping, ignoreProperties: [mapping => mapping.Id]);
        }
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementHasSameLocation_LocationAutoMapped()
    {
        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = England,
            LocalAuthority = Derby,
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [location],
            replacementLocations: [location]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping>
            {
                { location.Id, CreateLocationMapping(location, location, MapStatus.AutoSet) },
            },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementHasIdenticalLocationWithDifferentId_LocationAutoMapped()
    {
        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = England,
            LocalAuthority = Derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = England,
            LocalAuthority = Derby,
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocation]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping>
            {
                {
                    originalLocation.Id,
                    CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                },
            },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementLocationHasDifferentName_LocationMatchedByCode()
    {
        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby with { Name = "Derby City" },
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocation]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping>
            {
                {
                    originalLocation.Id,
                    CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                },
            },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementLocationHasAdditionalAttribute_LocationMatchedByCode()
    {
        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = England,
            LocalAuthority = Derby,
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocation]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping>
            {
                {
                    originalLocation.Id,
                    CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                },
            },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_SchoolsLocalAuthorityHasDifferentName_SchoolMatchedByCode()
    {
        School school = new("100000", "Test school");

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.School,
            LocalAuthority = Derby,
            School = school,
        };

        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.School,
            LocalAuthority = Derby with { Name = "Derby City" },
            School = school,
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocation]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping>
            {
                {
                    originalLocation.Id,
                    CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                },
            },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementLocationsHaveDifferentNameAndSameCode_LocationUnset()
    {
        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var replacementLocation1 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby with { Name = "Derby City" },
        };

        var replacementLocation2 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = England,
            LocalAuthority = Derby with { Name = "Derby City" },
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocation1, replacementLocation2]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping> { { originalLocation.Id, CreateLocationMapping(originalLocation) } },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_SameLocalAuthorityInDifferentRegions_LocationsUnset()
    {
        Region eastMidlands = new("E12000004", "East Midlands");
        Region westMidlands = new("E12000005", "West Midlands");

        var originalLocationInEastMidlands = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Region = eastMidlands,
            LocalAuthority = Derby,
        };

        var originalLocationInWestMidlands = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Region = westMidlands,
            LocalAuthority = Derby,
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocationInEastMidlands, originalLocationInWestMidlands],
            replacementLocations: [originalLocationInWestMidlands, originalLocationInEastMidlands]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping>
            {
                { originalLocationInEastMidlands.Id, CreateLocationMapping(originalLocationInEastMidlands) },
                { originalLocationInWestMidlands.Id, CreateLocationMapping(originalLocationInWestMidlands) },
            },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementHasMultipleIdenticalLocations_LocationUnset()
    {
        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var replacementLocation1 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var replacementLocation2 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocation1, replacementLocation2]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping> { { originalLocation.Id, CreateLocationMapping(originalLocation) } },
            locationMappings
        );
    }

    [Fact]
    public async Task CreateInitialDataSetMappingIfReplacement_ReplacementHasNoLocationWithSameLevelAndCode_LocationUnset()
    {
        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = Derby,
        };

        var replacementLocationWithDifferentCode = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = new LocalAuthority("E06000018", "", "Nottingham"),
        };

        var replacementLocationWithDifferentLevel = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
            LocalAuthorityDistrict = new LocalAuthorityDistrict(Derby.Code, Derby.Name),
        };

        var locationMappings = await GenerateLocationMappings(
            originalLocations: [originalLocation],
            replacementLocations: [replacementLocationWithDifferentCode, replacementLocationWithDifferentLevel]
        );

        Assert.Equal(
            new Dictionary<Guid, LocationMapping> { { originalLocation.Id, CreateLocationMapping(originalLocation) } },
            locationMappings
        );
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

    private async Task<Dictionary<Guid, LocationMapping>> GenerateLocationMappings(
        List<Location> originalLocations,
        List<Location> replacementLocations
    )
    {
        var (originalSubject, replacementSubject) = _fixture.DefaultSubject().GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalSubject.Id,
        };

        var replacementFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementSubject.Id,
            Replacing = originalFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.Location.AddRange(originalLocations.Union(replacementLocations));
            statisticsDbContext.Observation.AddRange(
                originalLocations.Select(location => new Observation
                {
                    SubjectId = originalSubject.Id,
                    LocationId = location.Id,
                })
            );
            statisticsDbContext.Observation.AddRange(
                replacementLocations.Select(location => new Observation
                {
                    SubjectId = replacementSubject.Id,
                    LocationId = location.Id,
                })
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var service = BuildDataSetMappingService(contentDbContextId, statisticsDbContextId);

        await service.CreateInitialDataSetMappingIfReplacement(replacementFile.Id);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            return contentDbContext
                .DataSetMappings.Single(mapping =>
                    mapping.OriginalDataFileId == originalFile.Id && mapping.ReplacementDataFileId == replacementFile.Id
                )
                .LocationMappings;
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

    private static FilterMapping CreateFilterMapping(
        Filter original,
        Filter? replacement = null,
        Dictionary<Guid, FilterGroupMapping>? filterGroupMappings = null,
        MapStatus status = MapStatus.Unset
    )
    {
        return new FilterMapping
        {
            OriginalId = original.Id,
            OriginalColumnName = original.Name,
            OriginalLabel = original.Label,
            ReplacementId = replacement?.Id,
            ReplacementColumnName = replacement?.Name,
            ReplacementLabel = replacement?.Label,
            FilterGroupMappings = filterGroupMappings ?? [],
            Status = status,
        };
    }

    private static FilterGroupMapping CreateFilterGroupMapping(
        FilterGroup original,
        FilterGroup? replacement = null,
        Dictionary<Guid, FilterItemMapping>? filterItemMappings = null,
        MapStatus status = MapStatus.Unset
    )
    {
        return new FilterGroupMapping
        {
            OriginalId = original.Id,
            OriginalLabel = original.Label,
            ReplacementId = replacement?.Id,
            ReplacementLabel = replacement?.Label,
            FilterItemMappings = filterItemMappings ?? [],
            Status = status,
        };
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
            ReplacementCode = replacement?.ToLocationAttribute().Code ?? "",
            ReplacementName = replacement?.ToLocationAttribute().Name ?? "",
            ReplacementGeographicLevel = replacement?.GeographicLevel,
            Status = status,
        };
    }
}
