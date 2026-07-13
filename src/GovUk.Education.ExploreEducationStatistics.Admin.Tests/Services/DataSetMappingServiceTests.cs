#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetMappingServiceTests
{
    [Fact]
    public async Task UpdateIndicatorMapping_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();
        var originalIndicator2Id = Guid.NewGuid();
        var originalIndicator3Id = Guid.NewGuid();
        var originalIndicator4Id = Guid.NewGuid();

        var replacementIndicator4Id = Guid.NewGuid();
        var replacementIndicator5Id = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicator1Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicator1Id,
                        OriginalLabel = "Original indicator 1 label",
                        OriginalColumnName = "original_indicator_1",
                        OriginalGroupId = Guid.NewGuid(),
                        OriginalGroupLabel = "Original indicator 1 group label",
                        Status = MapStatus.Unset,
                    }
                },
                {
                    originalIndicator2Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicator2Id,
                        OriginalLabel = "Original indicator 2 label",
                        OriginalColumnName = "original_indicator_2",
                        OriginalGroupId = Guid.NewGuid(),
                        OriginalGroupLabel = "Original indicator 2 group label",
                        ReplacementId = Guid.NewGuid(),
                        ReplacementLabel = "Replacement indicator 1 - that will be unset",
                        ReplacementColumnName = "replacement_indicator_1",
                        ReplacementGroupId = Guid.NewGuid(),
                        ReplacementGroupLabel = "Replacement indicator 1 group label",
                        Status = MapStatus.AutoSet,
                    }
                },
                {
                    originalIndicator3Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicator3Id,
                        OriginalLabel = "Original indicator 3 label",
                        OriginalColumnName = "original_indicator_3",
                        OriginalGroupId = Guid.NewGuid(),
                        OriginalGroupLabel = "Original indicator 3 group label",
                        ReplacementId = Guid.NewGuid(),
                        ReplacementLabel = "Replacement indicator 2 - that will be unset",
                        ReplacementColumnName = "replacement_indicator_2",
                        ReplacementGroupId = Guid.NewGuid(),
                        ReplacementGroupLabel = "Replacement indicator 2 group label",
                        Status = MapStatus.ManuallySet,
                    }
                },
                {
                    originalIndicator4Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicator4Id,
                        OriginalLabel = "Original indicator 4 label - to not change",
                        OriginalColumnName = "original_indicator_4",
                        OriginalGroupId = Guid.NewGuid(),
                        OriginalGroupLabel = "Original indicator 4 group label",
                        ReplacementId = Guid.NewGuid(),
                        ReplacementLabel = "Replacement indicator 3 - that will remain",
                        ReplacementColumnName = "replacement_indicator_3",
                        ReplacementGroupId = Guid.NewGuid(),
                        ReplacementGroupLabel = "Replacement indicator 3 group label",
                        Status = MapStatus.AutoSet,
                    }
                },
            },
            UnmappedReplacementIndicators =
            [
                new UnmappedIndicator
                {
                    Id = replacementIndicator4Id,
                    Label = "Replacement indicator 4 - that will be mapped to Original indicator 1",
                    ColumnName = "replacement_indicator_4",
                    GroupId = Guid.NewGuid(),
                    GroupLabel = "Replacement indicator 4 group label",
                },
                new UnmappedIndicator
                {
                    Id = replacementIndicator5Id,
                    Label = "Replacement indicator 5 - that will be mapped to Original indicator 2",
                    ColumnName = "replacement_indicator_5",
                    GroupId = Guid.NewGuid(),
                    GroupLabel = "Replacement indicator 5 group label",
                },
                new UnmappedIndicator
                {
                    Id = Guid.NewGuid(),
                    Label = "Replacement indicator 6 - that will be remain unmapped",
                    ColumnName = "replacement_indicator_6",
                    GroupId = Guid.NewGuid(),
                    GroupLabel = "Replacement indicator 6 group label",
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                releaseVersion.Id,
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates =
                    [
                        new() { OriginalId = originalIndicator1Id, NewReplacementId = replacementIndicator4Id },
                        new() { OriginalId = originalIndicator2Id, NewReplacementId = replacementIndicator5Id },
                        new() { OriginalId = originalIndicator3Id, NewReplacementId = null },
                    ],
                },
                CancellationToken.None
            );

            var indicatorMappingList = result.AssertRight();

            Assert.Equal(4, indicatorMappingList.Count);

            var originalIndicator1Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_1"
            );
            var originalIndicator2Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_2"
            );
            var originalIndicator3Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_3"
            );
            var originalIndicator4Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_4"
            );

            Assert.Multiple(
                () => Assert.Equal("replacement_indicator_4", originalIndicator1Mapping.ReplacementColumnName),
                () => Assert.Equal(nameof(MapStatus.ManuallySet), originalIndicator1Mapping.Status),
                () => Assert.Equal("replacement_indicator_5", originalIndicator2Mapping.ReplacementColumnName),
                () => Assert.Equal(nameof(MapStatus.ManuallySet), originalIndicator2Mapping.Status),
                () => Assert.Null(originalIndicator3Mapping.ReplacementColumnName),
                () => Assert.Equal(nameof(MapStatus.ManuallySet), originalIndicator3Mapping.Status),
                () => Assert.Equal("replacement_indicator_3", originalIndicator4Mapping.ReplacementColumnName),
                () => Assert.Equal(nameof(MapStatus.AutoSet), originalIndicator4Mapping.Status)
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = Assert.Single(contentDbContext.DataSetMappings.ToList());

            var indicatorMappingList = dbMapping.IndicatorMappings.Values.ToList();
            var originalIndicator1Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_1"
            );
            var originalIndicator2Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_2"
            );
            var originalIndicator3Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_3"
            );
            var originalIndicator4Mapping = indicatorMappingList.Single(indMap =>
                indMap.OriginalColumnName == "original_indicator_4"
            );

            Assert.Multiple(
                () => Assert.Equal("replacement_indicator_4", originalIndicator1Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.ManuallySet, originalIndicator1Mapping.Status),
                () => Assert.Equal("replacement_indicator_5", originalIndicator2Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.ManuallySet, originalIndicator2Mapping.Status),
                () => Assert.Null(originalIndicator3Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.ManuallySet, originalIndicator3Mapping.Status),
                () => Assert.Equal("replacement_indicator_3", originalIndicator4Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.AutoSet, originalIndicator4Mapping.Status)
            );

            var unmappedReplacementIndicators = dbMapping.UnmappedReplacementIndicators.ToList();
            Assert.Multiple(
                () => Assert.Equal(3, unmappedReplacementIndicators.Count),
                () =>
                    Assert.NotNull(
                        unmappedReplacementIndicators.FirstOrDefault(x => x.ColumnName == "replacement_indicator_1")
                    ),
                () =>
                    Assert.NotNull(
                        unmappedReplacementIndicators.FirstOrDefault(x => x.ColumnName == "replacement_indicator_2")
                    ),
                () =>
                    Assert.NotNull(
                        unmappedReplacementIndicators.FirstOrDefault(x => x.ColumnName == "replacement_indicator_6")
                    )
            );
        }
    }

    [Fact]
    public async Task UpdateIndicatorMappings_NoReleaseVersion_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

        var service = SetupDataSetMappingService(contentDbContext);
        var result = await service.UpdateIndicatorMappings(
            Guid.NewGuid(),
            new IndicatorMappingUpdatesRequest(),
            CancellationToken.None
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateIndicatorMapping_NoDataSetMapping_NotFound()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
            UnmappedReplacementIndicators = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                releaseVersion.Id,
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = Guid.NewGuid(),
                    Updates = [],
                },
                CancellationToken.None
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateIndicatorMappings_OriginalDataFile_NotFound()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var result = await service.UpdateIndicatorMappings(
                releaseVersion.Id,
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(IndicatorMappingUpdatesRequest.OriginalDataFileId)}",
                "OriginalDataFileIdNotLinkedToReleaseVersion"
            );
        }
    }

    [Fact]
    public async Task UpdateIndicatorMappings_ReplacementDataFile_NotFound()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
        };

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataSetMappings.Add(mapping);
            contentDbContext.ReleaseFiles.Add(originalReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var result = await service.UpdateIndicatorMappings(
                releaseVersion.Id,
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(IndicatorMappingUpdatesRequest.ReplacementDataFileId)}",
                "ReplacementDataFileIdNotLinkedToReleaseVersion"
            );
        }
    }

    [Fact]
    public async Task UpdateIndicatorMapping_OriginalIndicatorNotFound_Fail()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();

        var indicatorDoesNotExistId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicator1Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicator1Id,
                        OriginalColumnName = "original_indicator_1",
                        ReplacementId = Guid.NewGuid(),
                        ReplacementColumnName = "replacement_indicator_already_mapped",
                    }
                },
            },
            UnmappedReplacementIndicators = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                releaseVersion.Id,
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates = [new() { OriginalId = indicatorDoesNotExistId, NewReplacementId = null }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasError(
                expectedPath: "Updates.OriginalId",
                expectedCode: "IndicatorMatchingOriginalIdNotFound",
                expectedMessage: $"Could not find indicator mapping matching original id \"{indicatorDoesNotExistId}\""
            );
        }
    }

    [Fact]
    public async Task UpdateIndicatorMapping_UnmappedReplacementIndicatorNotFound_Fail()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();

        var replacementIndicatorAlreadyMappedId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicator1Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicator1Id,
                        OriginalColumnName = "original_indicator_1",
                        ReplacementId = replacementIndicatorAlreadyMappedId,
                        ReplacementColumnName = "replacement_indicator_already_mapped",
                    }
                },
            },
            UnmappedReplacementIndicators = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                releaseVersion.Id,
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates =
                    [
                        new()
                        {
                            OriginalId = originalIndicator1Id,
                            NewReplacementId = replacementIndicatorAlreadyMappedId,
                        },
                    ],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasError(
                expectedPath: "Updates.NewReplacementId",
                expectedCode: "UnmappedIndicatorMatchingReplacementIdNotFound",
                expectedMessage: $"No available unmapped indicator matching replacement id \"{replacementIndicatorAlreadyMappedId}\""
            );
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var loc1Id = Guid.NewGuid();
        var loc2Id = Guid.NewGuid();
        var loc3Id = Guid.NewGuid();
        var replacementLocId = Guid.NewGuid();
        var newlyUnmappedLocId = Guid.NewGuid();
        var loc3ReplacementId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                {
                    loc1Id,
                    new LocationMapping
                    {
                        OriginalId = loc1Id,
                        OriginalGeographicLevel = GeographicLevel.LocalAuthority,
                        Status = MapStatus.Unset,
                    }
                },
                {
                    loc2Id,
                    new LocationMapping
                    {
                        OriginalId = loc2Id,
                        OriginalGeographicLevel = GeographicLevel.Country,
                        ReplacementId = newlyUnmappedLocId,
                        ReplacementName = "Old Country Name",
                        ReplacementCode = "E9200002",
                        ReplacementGeographicLevel = GeographicLevel.Country,
                        Status = MapStatus.AutoSet,
                    }
                },
                {
                    loc3Id,
                    new LocationMapping
                    {
                        OriginalId = loc3Id,
                        OriginalGeographicLevel = GeographicLevel.Region,
                        ReplacementId = loc3ReplacementId,
                        Status = MapStatus.Unset,
                    }
                },
            },
            UnmappedReplacementLocations =
            [
                new UnmappedLocation
                {
                    Id = replacementLocId,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Name = "New LA",
                    Code = "301",
                },
            ],
        };

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataSetMappings.Add(mapping);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates =
                    [
                        new() { OriginalLocationId = loc1Id, NewReplacementLocationId = replacementLocId },
                        new() { OriginalLocationId = loc2Id, NewReplacementLocationId = null },
                    ],
                },
                CancellationToken.None
            );

            var locationMappingList = result.AssertRight();
            Assert.Equal(3, locationMappingList.Count);

            var map1 = locationMappingList.Single(m => m.OriginalId == loc1Id);
            Assert.Equal(replacementLocId, map1.ReplacementId);
            Assert.Equal(nameof(MapStatus.ManuallySet), map1.Status);

            var map2 = locationMappingList.Single(m => m.OriginalId == loc2Id);
            Assert.Null(map2.ReplacementId);
            Assert.Equal(nameof(MapStatus.ManuallySet), map2.Status);

            var map3 = locationMappingList.Single(m => m.OriginalId == loc3Id);
            Assert.Equal(loc3ReplacementId, map3.ReplacementId);
            Assert.Equal(nameof(MapStatus.Unset), map3.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();

            Assert.Single(dbMapping.UnmappedReplacementLocations);

            // Check that the old replacement from loc2 was moved back to unmapped
            Assert.Contains(dbMapping.UnmappedReplacementLocations, l => l.Id == newlyUnmappedLocId);
            // Check that the new replacement was removed from unmapped
            Assert.DoesNotContain(dbMapping.UnmappedReplacementLocations, l => l.Id == replacementLocId);
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_NoReleaseVersion_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

        var service = SetupDataSetMappingService(contentDbContext);
        var result = await service.UpdateLocationMappings(
            Guid.NewGuid(),
            new LocationMappingUpdatesRequest(),
            CancellationToken.None
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateLocationMappings_NoDataSetMapping_NotFound()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest(),
                CancellationToken.None
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_OriginalDataFile_NotFound()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.OriginalDataFileId)}",
                "OriginalDataFileIdNotLinkedToReleaseVersion"
            );
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_ReplacementDataFile_NotFound()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
        };

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataSetMappings.Add(mapping);
            contentDbContext.ReleaseFiles.Add(originalReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.ReplacementDataFileId)}",
                "ReplacementDataFileIdNotLinkedToReleaseVersion"
            );
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_OriginalLocationNotFound_Fail()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            LocationMappings = new(),
        };

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataSetMappings.Add(mapping);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var badId = Guid.NewGuid();
            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates = [new() { OriginalLocationId = badId }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(LocationMappingUpdateRequest.OriginalLocationId)}",
                "LocationMatchingOriginalIdNameNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_UnmappedLocationNotFound_Fail()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var locId = Guid.NewGuid();
        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                {
                    locId,
                    new LocationMapping { OriginalId = locId }
                },
            },
            UnmappedReplacementLocations = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates = [new() { OriginalLocationId = locId, NewReplacementLocationId = Guid.NewGuid() }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(LocationMappingUpdateRequest.NewReplacementLocationId)}",
                "UnmappedLocationMatchingReplacementLocationIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateLocationMappings_DifferentGeographicLevel_Fail()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };
        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = originalDataFileId },
        };
        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            File = new Content.Model.File { Id = replacementDataFileId },
        };

        var locId = Guid.NewGuid();
        var replacementLocId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                {
                    locId,
                    new LocationMapping { OriginalId = locId, OriginalGeographicLevel = GeographicLevel.Region }
                },
            },
            UnmappedReplacementLocations =
            [
                new UnmappedLocation { Id = replacementLocId, GeographicLevel = GeographicLevel.LocalAuthority },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);
            var result = await service.UpdateLocationMappings(
                releaseVersion.Id,
                new LocationMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    Updates = [new() { OriginalLocationId = locId, NewReplacementLocationId = replacementLocId }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(LocationMappingUpdateRequest.NewReplacementLocationId)}",
                "UnmappedLocationHasDifferentGeographicLevelAsOriginalLocation"
            );
        }
    }

    private static DataSetMappingService SetupDataSetMappingService(
        ContentDbContext contentDbContext,
        IUserService? userService = null
    )
    {
        return new DataSetMappingService(contentDbContext, userService ?? MockUtils.AlwaysTrueUserService().Object);
    }
}
