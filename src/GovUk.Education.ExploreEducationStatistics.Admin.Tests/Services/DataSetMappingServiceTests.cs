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
                () => Assert.Equal(MapStatus.ManuallySet, originalIndicator1Mapping.Status),
                () => Assert.Equal("replacement_indicator_5", originalIndicator2Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.ManuallySet, originalIndicator2Mapping.Status),
                () => Assert.Null(originalIndicator3Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.ManuallySet, originalIndicator3Mapping.Status),
                () => Assert.Equal("replacement_indicator_3", originalIndicator4Mapping.ReplacementColumnName),
                () => Assert.Equal(MapStatus.AutoSet, originalIndicator4Mapping.Status)
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
                expectedCode: "UnmappedIndicatorMatchingReplacementIdNotFound"
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
                        new() { OriginalId = loc1Id, NewReplacementId = replacementLocId },
                        new() { OriginalId = loc2Id, NewReplacementId = null },
                    ],
                },
                CancellationToken.None
            );

            var locationMappingList = result.AssertRight();
            Assert.Equal(3, locationMappingList.Count);

            var map1 = locationMappingList.Single(m => m.OriginalId == loc1Id);
            Assert.Equal(replacementLocId, map1.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, map1.Status);

            var map2 = locationMappingList.Single(m => m.OriginalId == loc2Id);
            Assert.Null(map2.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, map2.Status);

            var map3 = locationMappingList.Single(m => m.OriginalId == loc3Id);
            Assert.Equal(loc3ReplacementId, map3.ReplacementId);
            Assert.Equal(MapStatus.Unset, map3.Status);
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
                    Updates = [new() { OriginalId = badId }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
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
                    Updates = [new() { OriginalId = locId, NewReplacementId = Guid.NewGuid() }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
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
                    Updates = [new() { OriginalId = locId, NewReplacementId = replacementLocId }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasError(
                $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                "UnmappedLocationHasDifferentGeographicLevelAsOriginalLocation"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalFilter1Id = Guid.NewGuid();
        var originalFilter1Group1Id = Guid.NewGuid();
        var originalFilter1Group1Item1Id = Guid.NewGuid();

        var originalFilter2Id = Guid.NewGuid();
        var originalFilter2Group1Id = Guid.NewGuid();
        var originalFilter2Group1Item1Id = Guid.NewGuid();

        var replacementFilter1Id = Guid.NewGuid();
        var replacementFilter1Group1Id = Guid.NewGuid();
        var replacementFilter1Group1Item1Id = Guid.NewGuid();

        var previouslyMappedReplacementFilter2Id = Guid.NewGuid();
        var previouslyMappedReplacementFilter2Group1Id = Guid.NewGuid();
        var previouslyMappedReplacementFilter2Group1Item1Id = Guid.NewGuid();

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
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilter1Id,
                    new FilterMapping
                    {
                        OriginalId = originalFilter1Id,
                        OriginalLabel = "Original filter 1",
                        OriginalColumnName = "original_filter_1",
                        Status = MapStatus.Unset,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilter1Group1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilter1Group1Id,
                                    OriginalLabel = "Original filter 1 group 1",
                                    Status = MapStatus.ParentNotMapped,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilter1Group1Item1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilter1Group1Item1Id,
                                                OriginalLabel = "Original filter 1 group 1 item 1",
                                                Status = MapStatus.ParentNotMapped,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
                {
                    originalFilter2Id,
                    new FilterMapping
                    {
                        OriginalId = originalFilter2Id,
                        OriginalLabel = "Original filter 2",
                        OriginalColumnName = "original_filter_2",
                        ReplacementId = previouslyMappedReplacementFilter2Id,
                        ReplacementLabel = "Replacement filter 2",
                        ReplacementColumnName = "replacement_filter_2",
                        Status = MapStatus.AutoSet,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilter2Group1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilter2Group1Id,
                                    OriginalLabel = "Original filter 2 group 1",
                                    ReplacementId = previouslyMappedReplacementFilter2Group1Id,
                                    ReplacementLabel = "Replacement filter 2 group 1",
                                    Status = MapStatus.AutoSet,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilter2Group1Item1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilter2Group1Item1Id,
                                                OriginalLabel = "Original filter 2 group 1 item 1",
                                                ReplacementId = previouslyMappedReplacementFilter2Group1Item1Id,
                                                ReplacementLabel = "Replacement filter 2 group 1 item 1",
                                                Status = MapStatus.AutoSet,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            // ReplacementFilters/Groups/Items are the complete replacement-side catalogue - it includes both the
            // still-unclaimed replacementFilter1Id subtree, and the previouslyMappedReplacementFilter2Id subtree
            // that filter 2's mapping currently claims.
            ReplacementFilters =
            [
                new ReplacementFilter
                {
                    Id = replacementFilter1Id,
                    Label = "Replacement filter 1",
                    ColumnName = "replacement_filter_1",
                },
                new ReplacementFilter
                {
                    Id = previouslyMappedReplacementFilter2Id,
                    Label = "Replacement filter 2",
                    ColumnName = "replacement_filter_2",
                },
            ],
            ReplacementFilterGroups =
            [
                new ReplacementFilterGroup
                {
                    Id = replacementFilter1Group1Id,
                    FilterId = replacementFilter1Id,
                    Label = "Original filter 1 group 1",
                },
                new ReplacementFilterGroup
                {
                    Id = previouslyMappedReplacementFilter2Group1Id,
                    FilterId = previouslyMappedReplacementFilter2Id,
                    Label = "Replacement filter 2 group 1",
                },
            ],
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = replacementFilter1Group1Item1Id,
                    FilterGroupId = replacementFilter1Group1Id,
                    Label = "Original filter 1 group 1 item 1",
                },
                new ReplacementFilterItem
                {
                    Id = previouslyMappedReplacementFilter2Group1Item1Id,
                    FilterGroupId = previouslyMappedReplacementFilter2Group1Id,
                    Label = "Replacement filter 2 group 1 item 1",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates =
                    [
                        new() { OriginalId = originalFilter1Id, NewReplacementId = replacementFilter1Id },
                        new() { OriginalId = originalFilter2Id, NewReplacementId = null },
                    ],
                    FilterGroupUpdates = [],
                    FilterItemUpdates = [],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            Assert.Equal(2, dto.Filters.Count);
            Assert.Empty(dto.FilterGroups);
            Assert.Empty(dto.FilterItems);

            var filter1Dto = dto.Filters.Single(f => f.OriginalId == originalFilter1Id);
            Assert.Equal(replacementFilter1Id, filter1Dto.ReplacementId);
            Assert.Equal("Replacement filter 1", filter1Dto.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, filter1Dto.Status);

            var filter2Dto = dto.Filters.Single(f => f.OriginalId == originalFilter2Id);
            Assert.Null(filter2Dto.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, filter2Dto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();

            var filter1 = dbMapping.FilterMappings[originalFilter1Id];
            Assert.Equal(replacementFilter1Id, filter1.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, filter1.Status);

            var filter1Group1 = filter1.FilterGroupMappings[originalFilter1Group1Id];
            Assert.Equal(replacementFilter1Group1Id, filter1Group1.ReplacementId);
            Assert.Equal(MapStatus.AutoSet, filter1Group1.Status);

            var filter1Group1Item1 = filter1Group1.FilterItemMappings[originalFilter1Group1Item1Id];
            Assert.Equal(replacementFilter1Group1Item1Id, filter1Group1Item1.ReplacementId);
            Assert.Equal(MapStatus.AutoSet, filter1Group1Item1.Status);

            var filter2 = dbMapping.FilterMappings[originalFilter2Id];
            Assert.Null(filter2.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, filter2.Status);

            var filter2Group1 = filter2.FilterGroupMappings[originalFilter2Group1Id];
            Assert.Null(filter2Group1.ReplacementId);
            Assert.Equal(MapStatus.ParentNotMapped, filter2Group1.Status);

            var filter2Group1Item1 = filter2Group1.FilterItemMappings[originalFilter2Group1Item1Id];
            Assert.Null(filter2Group1Item1.ReplacementId);
            Assert.Equal(MapStatus.ParentNotMapped, filter2Group1Item1.Status);

            // previouslyMappedReplacementFilter2Id is available again simply because no live mapping claims it any
            // more (asserted above) - the candidate catalogue itself is immutable and stays as it was.
            Assert.Equal(2, dbMapping.ReplacementFilters.Count);
            Assert.Contains(dbMapping.ReplacementFilters, f => f.Id == previouslyMappedReplacementFilter2Id);
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_ChangeExistingReplacementFilter_Success()
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

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroup1Id = Guid.NewGuid();
        var originalFilterGroup1Item1Id = Guid.NewGuid();

        var oldReplacementFilterId = Guid.NewGuid();
        var oldReplacementFilterGroup1Id = Guid.NewGuid();
        var oldReplacementFilterGroup1Item1Id = Guid.NewGuid();

        var newReplacementFilterId = Guid.NewGuid();
        var newReplacementFilterGroup1Id = Guid.NewGuid();
        var newReplacementFilterGroup1Item1Id = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        OriginalLabel = "Original filter",
                        OriginalColumnName = "original_filter",
                        ReplacementId = oldReplacementFilterId,
                        ReplacementLabel = "Old replacement filter",
                        ReplacementColumnName = "old_replacement_filter",
                        Status = MapStatus.ManuallySet,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroup1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroup1Id,
                                    OriginalLabel = "Original filter group 1",
                                    ReplacementId = oldReplacementFilterGroup1Id,
                                    ReplacementLabel = "Old replacement filter group 1",
                                    Status = MapStatus.AutoSet,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterGroup1Item1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterGroup1Item1Id,
                                                OriginalLabel = "Original filter group 1 item 1",
                                                ReplacementId = oldReplacementFilterGroup1Item1Id,
                                                ReplacementLabel = "Old replacement filter group 1 item 1",
                                                Status = MapStatus.AutoSet,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            // The catalogue is the complete replacement-side data: both the still-claimed oldReplacementFilterId
            // subtree, and the new, currently-unclaimed newReplacementFilterId subtree.
            ReplacementFilters =
            [
                new ReplacementFilter
                {
                    Id = oldReplacementFilterId,
                    Label = "Old replacement filter",
                    ColumnName = "old_replacement_filter",
                },
                new ReplacementFilter
                {
                    Id = newReplacementFilterId,
                    Label = "New replacement filter",
                    ColumnName = "new_replacement_filter",
                },
            ],
            ReplacementFilterGroups =
            [
                new ReplacementFilterGroup
                {
                    Id = oldReplacementFilterGroup1Id,
                    FilterId = oldReplacementFilterId,
                    Label = "Old replacement filter group 1",
                },
                new ReplacementFilterGroup
                {
                    Id = newReplacementFilterGroup1Id,
                    FilterId = newReplacementFilterId,
                    Label = "Original filter group 1",
                },
            ],
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = oldReplacementFilterGroup1Item1Id,
                    FilterGroupId = oldReplacementFilterGroup1Id,
                    Label = "Old replacement filter group 1 item 1",
                },
                new ReplacementFilterItem
                {
                    Id = newReplacementFilterGroup1Item1Id,
                    FilterGroupId = newReplacementFilterGroup1Id,
                    Label = "Original filter group 1 item 1",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates =
                    [
                        new() { OriginalId = originalFilterId, NewReplacementId = newReplacementFilterId },
                    ],
                    FilterGroupUpdates = [],
                    FilterItemUpdates = [],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            Assert.Single(dto.Filters);

            var filterDto = dto.Filters.Single();
            Assert.Equal(originalFilterId, filterDto.OriginalId);
            Assert.Equal(newReplacementFilterId, filterDto.ReplacementId);
            Assert.Equal("New replacement filter", filterDto.ReplacementLabel);
            Assert.Equal("new_replacement_filter", filterDto.ReplacementColumnName);
            Assert.Equal(MapStatus.ManuallySet, filterDto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();

            var filter = dbMapping.FilterMappings[originalFilterId];
            Assert.Equal(newReplacementFilterId, filter.ReplacementId);
            Assert.Equal("New replacement filter", filter.ReplacementLabel);
            Assert.Equal("new_replacement_filter", filter.ReplacementColumnName);
            Assert.Equal(MapStatus.ManuallySet, filter.Status);

            var group1 = filter.FilterGroupMappings[originalFilterGroup1Id];
            Assert.Equal(newReplacementFilterGroup1Id, group1.ReplacementId);
            Assert.Equal("Original filter group 1", group1.ReplacementLabel);
            Assert.Equal(MapStatus.AutoSet, group1.Status);

            var item1 = group1.FilterItemMappings[originalFilterGroup1Item1Id];
            Assert.Equal(newReplacementFilterGroup1Item1Id, item1.ReplacementId);
            Assert.Equal("Original filter group 1 item 1", item1.ReplacementLabel);
            Assert.Equal(MapStatus.AutoSet, item1.Status);

            // oldReplacementFilterId is available again simply because no live mapping claims it any more (asserted
            // above) - the candidate catalogue itself is immutable and stays as it was.
            Assert.Equal(2, dbMapping.ReplacementFilters.Count);
            Assert.Contains(dbMapping.ReplacementFilters, f => f.Id == oldReplacementFilterId);
            Assert.Equal(2, dbMapping.ReplacementFilterGroups.Count);
            Assert.Contains(dbMapping.ReplacementFilterGroups, g => g.Id == oldReplacementFilterGroup1Id);
            Assert.Equal(2, dbMapping.ReplacementFilterItems.Count);
            Assert.Contains(dbMapping.ReplacementFilterItems, i => i.Id == oldReplacementFilterGroup1Item1Id);
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_NoReleaseVersion_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

        var service = SetupDataSetMappingService(contentDbContext);
        var result = await service.UpdateFilterMappings(
            Guid.NewGuid(),
            new FilterMappingUpdatesRequest(),
            CancellationToken.None
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateFilterMappings_NoDataSetMapping_NotFound()
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
            FilterMappings = new Dictionary<Guid, FilterMapping>(),
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = Guid.NewGuid(),
                    FilterUpdates = [],
                },
                CancellationToken.None
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_OriginalFilterNotFound_Fail()
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

        var filterDoesNotExistId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>(),
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates = [new() { OriginalId = filterDoesNotExistId, NewReplacementId = null }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                expectedCode: "FilterMatchingOriginalIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_UnmappedReplacementFilterNotFound_Fail()
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

        var originalFilterId = Guid.NewGuid();
        var replacementFilterDoesNotExistId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping { OriginalId = originalFilterId }
                },
            },
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates =
                    [
                        new() { OriginalId = originalFilterId, NewReplacementId = replacementFilterDoesNotExistId },
                    ],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                expectedCode: "UnmappedFilterMatchingReplacementIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_FilterGroupParentNotMapped_Fail()
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

        var originalFilterId = Guid.NewGuid();
        var originalGroupId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        Status = MapStatus.Unset,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalGroupId,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalGroupId,
                                    Status = MapStatus.ParentNotMapped,
                                }
                            },
                        },
                    }
                },
            },
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterGroupUpdates = [new() { OriginalId = originalGroupId, NewReplacementId = Guid.NewGuid() }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                expectedCode: "FilterGroupParentNotMapped"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_FilterItemParentNotMapped_Fail()
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

        var originalFilterId = Guid.NewGuid();
        var originalGroupId = Guid.NewGuid();
        var originalItemId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        Status = MapStatus.Unset,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalGroupId,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalGroupId,
                                    Status = MapStatus.ParentNotMapped,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalItemId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalItemId,
                                                Status = MapStatus.ParentNotMapped,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterItemUpdates = [new() { OriginalId = originalItemId, NewReplacementId = Guid.NewGuid() }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                expectedCode: "FilterItemParentNotMapped"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_FilterGroup_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroup1Id = Guid.NewGuid();
        var originalFilterGroup1Item1Id = Guid.NewGuid();

        var originalFilterGroup2Id = Guid.NewGuid();
        var originalFilterGroup2Item1Id = Guid.NewGuid();

        var replacementFilterId = Guid.NewGuid();
        var newReplacementFilterGroup1Id = Guid.NewGuid();
        var newReplacementFilterGroup1Item1Id = Guid.NewGuid();

        var oldReplacementFilterGroup2Id = Guid.NewGuid();
        var oldReplacementFilterGroup2Item1Id = Guid.NewGuid();

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
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        OriginalLabel = "Original filter",
                        ReplacementId = replacementFilterId,
                        ReplacementLabel = "Replacement filter",
                        Status = MapStatus.AutoSet,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroup1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroup1Id,
                                    OriginalLabel = "Original filter group 1",
                                    Status = MapStatus.Unset,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterGroup1Item1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterGroup1Item1Id,
                                                OriginalLabel = "Original filter group 1 item 1",
                                                Status = MapStatus.ParentNotMapped,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                originalFilterGroup2Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroup2Id,
                                    OriginalLabel = "Original filter group 2",
                                    ReplacementId = oldReplacementFilterGroup2Id,
                                    ReplacementLabel = "Old replacement filter group 2",
                                    Status = MapStatus.AutoSet,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterGroup2Item1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterGroup2Item1Id,
                                                OriginalLabel = "Original filter group 2 item 1",
                                                ReplacementId = oldReplacementFilterGroup2Item1Id,
                                                ReplacementLabel = "Old replacement filter group 2 item 1",
                                                Status = MapStatus.AutoSet,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            // The catalogue is scoped under the (already-claimed) replacementFilterId and includes both the
            // still-claimed oldReplacementFilterGroup2Id subtree, and the new, currently-unclaimed
            // newReplacementFilterGroup1Id subtree.
            ReplacementFilters = [new ReplacementFilter { Id = replacementFilterId, Label = "Replacement filter" }],
            ReplacementFilterGroups =
            [
                new ReplacementFilterGroup
                {
                    Id = newReplacementFilterGroup1Id,
                    FilterId = replacementFilterId,
                    Label = "New replacement filter group 1",
                },
                new ReplacementFilterGroup
                {
                    Id = oldReplacementFilterGroup2Id,
                    FilterId = replacementFilterId,
                    Label = "Old replacement filter group 2",
                },
            ],
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = newReplacementFilterGroup1Item1Id,
                    FilterGroupId = newReplacementFilterGroup1Id,
                    Label = "Original filter group 1 item 1",
                },
                new ReplacementFilterItem
                {
                    Id = oldReplacementFilterGroup2Item1Id,
                    FilterGroupId = oldReplacementFilterGroup2Id,
                    Label = "Old replacement filter group 2 item 1",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates = [],
                    FilterGroupUpdates =
                    [
                        new() { OriginalId = originalFilterGroup1Id, NewReplacementId = newReplacementFilterGroup1Id },
                        new() { OriginalId = originalFilterGroup2Id, NewReplacementId = null },
                    ],
                    FilterItemUpdates = [],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            Assert.Empty(dto.Filters);
            Assert.Equal(2, dto.FilterGroups.Count);
            Assert.Empty(dto.FilterItems);

            var group1Dto = dto.FilterGroups.Single(g => g.OriginalId == originalFilterGroup1Id);
            Assert.Equal(newReplacementFilterGroup1Id, group1Dto.ReplacementId);
            Assert.Equal("New replacement filter group 1", group1Dto.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, group1Dto.Status);

            var group2Dto = dto.FilterGroups.Single(g => g.OriginalId == originalFilterGroup2Id);
            Assert.Null(group2Dto.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, group2Dto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();
            var filter = dbMapping.FilterMappings[originalFilterId];

            var group1 = filter.FilterGroupMappings[originalFilterGroup1Id];
            Assert.Equal(newReplacementFilterGroup1Id, group1.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, group1.Status);
            var item1 = group1.FilterItemMappings[originalFilterGroup1Item1Id];
            Assert.Equal(newReplacementFilterGroup1Item1Id, item1.ReplacementId);
            Assert.Equal(MapStatus.AutoSet, item1.Status);

            var group2 = filter.FilterGroupMappings[originalFilterGroup2Id];
            Assert.Null(group2.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, group2.Status);
            var group2Item1 = group2.FilterItemMappings[originalFilterGroup2Item1Id];
            Assert.Null(group2Item1.ReplacementId);
            Assert.Equal(MapStatus.ParentNotMapped, group2Item1.Status);

            // oldReplacementFilterGroup2Id is available again simply because no live mapping claims it any more
            // (asserted above) - the candidate catalogue itself is immutable and stays as it was.
            Assert.Equal(2, dbMapping.ReplacementFilterGroups.Count);
            Assert.Contains(dbMapping.ReplacementFilterGroups, g => g.Id == oldReplacementFilterGroup2Id);
            Assert.Equal(2, dbMapping.ReplacementFilterItems.Count);
            Assert.Contains(dbMapping.ReplacementFilterItems, i => i.Id == oldReplacementFilterGroup2Item1Id);
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_ChangeExistingReplacementFilterGroup_Success()
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

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroup1Id = Guid.NewGuid();
        var originalFilterGroup1Item1Id = Guid.NewGuid();

        var oldReplacementFilterGroup1Id = Guid.NewGuid();
        var oldReplacementFilterGroup1Item1Id = Guid.NewGuid();

        var newReplacementFilterGroup1Id = Guid.NewGuid();
        var newReplacementFilterGroup1Item1Id = Guid.NewGuid();

        var replacementFilterId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        OriginalLabel = "Original filter",
                        ReplacementId = replacementFilterId,
                        ReplacementLabel = "Replacement filter",
                        Status = MapStatus.AutoSet,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroup1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroup1Id,
                                    OriginalLabel = "Original filter group 1",
                                    ReplacementId = oldReplacementFilterGroup1Id,
                                    ReplacementLabel = "Old replacement filter group 1",
                                    Status = MapStatus.ManuallySet,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterGroup1Item1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterGroup1Item1Id,
                                                OriginalLabel = "Original filter group 1 item 1",
                                                ReplacementId = oldReplacementFilterGroup1Item1Id,
                                                ReplacementLabel = "Old replacement filter group 1 item 1",
                                                Status = MapStatus.AutoSet,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            // The catalogue is scoped under the (already-claimed) replacementFilterId and includes both the
            // still-claimed oldReplacementFilterGroup1Id subtree, and the new, currently-unclaimed
            // newReplacementFilterGroup1Id subtree.
            ReplacementFilters = [new ReplacementFilter { Id = replacementFilterId, Label = "Replacement filter" }],
            ReplacementFilterGroups =
            [
                new ReplacementFilterGroup
                {
                    Id = newReplacementFilterGroup1Id,
                    FilterId = replacementFilterId,
                    Label = "New replacement filter group 1",
                },
                new ReplacementFilterGroup
                {
                    Id = oldReplacementFilterGroup1Id,
                    FilterId = replacementFilterId,
                    Label = "Old replacement filter group 1",
                },
            ],
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = newReplacementFilterGroup1Item1Id,
                    FilterGroupId = newReplacementFilterGroup1Id,
                    Label = "Original filter group 1 item 1",
                },
                new ReplacementFilterItem
                {
                    Id = oldReplacementFilterGroup1Item1Id,
                    FilterGroupId = oldReplacementFilterGroup1Id,
                    Label = "Old replacement filter group 1 item 1",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterGroupUpdates =
                    [
                        new() { OriginalId = originalFilterGroup1Id, NewReplacementId = newReplacementFilterGroup1Id },
                    ],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            var groupDto = Assert.Single(dto.FilterGroups);
            Assert.Equal(newReplacementFilterGroup1Id, groupDto.ReplacementId);
            Assert.Equal("New replacement filter group 1", groupDto.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, groupDto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();
            var filter = dbMapping.FilterMappings[originalFilterId];

            var group1 = filter.FilterGroupMappings[originalFilterGroup1Id];
            Assert.Equal(newReplacementFilterGroup1Id, group1.ReplacementId);
            Assert.Equal("New replacement filter group 1", group1.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, group1.Status);

            var item1 = group1.FilterItemMappings[originalFilterGroup1Item1Id];
            Assert.Equal(newReplacementFilterGroup1Item1Id, item1.ReplacementId);
            Assert.Equal(MapStatus.AutoSet, item1.Status);

            // oldReplacementFilterGroup1Id is available again simply because no live mapping claims it any more
            // (asserted above) - the candidate catalogue itself is immutable and stays as it was.
            Assert.Equal(2, dbMapping.ReplacementFilterGroups.Count);
            Assert.Contains(dbMapping.ReplacementFilterGroups, g => g.Id == oldReplacementFilterGroup1Id);
            Assert.Equal(2, dbMapping.ReplacementFilterItems.Count);
            Assert.Contains(dbMapping.ReplacementFilterItems, i => i.Id == oldReplacementFilterGroup1Item1Id);
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_OriginalFilterGroupNotFound_Fail()
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

        var groupDoesNotExistId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>(),
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterGroupUpdates = [new() { OriginalId = groupDoesNotExistId, NewReplacementId = null }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                expectedCode: "FilterGroupMatchingOriginalIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_UnmappedReplacementFilterGroupNotFound_Fail()
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

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroupId = Guid.NewGuid();
        var replacementGroupDoesNotExistId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        Status = MapStatus.AutoSet,
                        ReplacementId = Guid.NewGuid(),
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroupId,
                                new FilterGroupMapping { OriginalId = originalFilterGroupId, Status = MapStatus.Unset }
                            },
                        },
                    }
                },
            },
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterGroupUpdates =
                    [
                        new() { OriginalId = originalFilterGroupId, NewReplacementId = replacementGroupDoesNotExistId },
                    ],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                expectedCode: "UnmappedFilterGroupMatchingReplacementIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_FilterItem_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroupId = Guid.NewGuid();
        var originalFilterItem1Id = Guid.NewGuid();
        var originalFilterItem2Id = Guid.NewGuid();

        var replacementFilterId = Guid.NewGuid();
        var replacementFilterGroupId = Guid.NewGuid();
        var newReplacementFilterItem1Id = Guid.NewGuid();
        var oldReplacementFilterItem2Id = Guid.NewGuid();

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
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        OriginalLabel = "Original filter",
                        ReplacementId = replacementFilterId,
                        ReplacementLabel = "Replacement filter",
                        Status = MapStatus.AutoSet,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroupId,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroupId,
                                    OriginalLabel = "Original filter group",
                                    ReplacementId = replacementFilterGroupId,
                                    ReplacementLabel = "Replacement filter group",
                                    Status = MapStatus.AutoSet,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterItem1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterItem1Id,
                                                OriginalLabel = "Original filter item 1",
                                                Status = MapStatus.Unset,
                                            }
                                        },
                                        {
                                            originalFilterItem2Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterItem2Id,
                                                OriginalLabel = "Original filter item 2",
                                                ReplacementId = oldReplacementFilterItem2Id,
                                                ReplacementLabel = "Old replacement filter item 2",
                                                Status = MapStatus.AutoSet,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            // The catalogue is scoped under the (already-claimed) replacementFilterId/replacementFilterGroupId and
            // includes both the still-claimed oldReplacementFilterItem2Id, and the new, currently-unclaimed
            // newReplacementFilterItem1Id.
            ReplacementFilters = [new ReplacementFilter { Id = replacementFilterId, Label = "Replacement filter" }],
            ReplacementFilterGroups =
            [
                new ReplacementFilterGroup
                {
                    Id = replacementFilterGroupId,
                    FilterId = replacementFilterId,
                    Label = "Replacement filter group",
                },
            ],
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = newReplacementFilterItem1Id,
                    FilterGroupId = replacementFilterGroupId,
                    Label = "New replacement filter item 1",
                },
                new ReplacementFilterItem
                {
                    Id = oldReplacementFilterItem2Id,
                    FilterGroupId = replacementFilterGroupId,
                    Label = "Old replacement filter item 2",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates = [],
                    FilterGroupUpdates = [],
                    FilterItemUpdates =
                    [
                        new() { OriginalId = originalFilterItem1Id, NewReplacementId = newReplacementFilterItem1Id },
                        new() { OriginalId = originalFilterItem2Id, NewReplacementId = null },
                    ],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            Assert.Empty(dto.Filters);
            Assert.Empty(dto.FilterGroups);
            Assert.Equal(2, dto.FilterItems.Count);

            var item1Dto = dto.FilterItems.Single(i => i.OriginalId == originalFilterItem1Id);
            Assert.Equal(newReplacementFilterItem1Id, item1Dto.ReplacementId);
            Assert.Equal("New replacement filter item 1", item1Dto.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, item1Dto.Status);

            var item2Dto = dto.FilterItems.Single(i => i.OriginalId == originalFilterItem2Id);
            Assert.Null(item2Dto.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, item2Dto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();
            var filter = dbMapping.FilterMappings[originalFilterId];
            var group = filter.FilterGroupMappings[originalFilterGroupId];

            var item1 = group.FilterItemMappings[originalFilterItem1Id];
            Assert.Equal(newReplacementFilterItem1Id, item1.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, item1.Status);

            var item2 = group.FilterItemMappings[originalFilterItem2Id];
            Assert.Null(item2.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, item2.Status);

            // oldReplacementFilterItem2Id is available again simply because no live mapping claims it any more
            // (asserted above) - the candidate catalogue itself is immutable and stays as it was.
            Assert.Equal(2, dbMapping.ReplacementFilterItems.Count);
            Assert.Contains(dbMapping.ReplacementFilterItems, i => i.Id == oldReplacementFilterItem2Id);
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_ChangeExistingReplacementFilterItem_Success()
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

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroupId = Guid.NewGuid();
        var originalFilterItem1Id = Guid.NewGuid();

        var oldReplacementFilterItem1Id = Guid.NewGuid();
        var newReplacementFilterItem1Id = Guid.NewGuid();

        var replacementFilterGroupId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        OriginalLabel = "Original filter",
                        ReplacementId = Guid.NewGuid(),
                        ReplacementLabel = "Replacement filter",
                        Status = MapStatus.AutoSet,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroupId,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroupId,
                                    OriginalLabel = "Original filter group",
                                    ReplacementId = replacementFilterGroupId,
                                    ReplacementLabel = "Replacement filter group",
                                    Status = MapStatus.AutoSet,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterItem1Id,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterItem1Id,
                                                OriginalLabel = "Original filter item 1",
                                                ReplacementId = oldReplacementFilterItem1Id,
                                                ReplacementLabel = "Old replacement filter item 1",
                                                Status = MapStatus.ManuallySet,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            // The catalogue is scoped under the (already-claimed) replacementFilterGroupId and includes both the
            // still-claimed oldReplacementFilterItem1Id, and the new, currently-unclaimed newReplacementFilterItem1Id.
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = newReplacementFilterItem1Id,
                    FilterGroupId = replacementFilterGroupId,
                    Label = "New replacement filter item 1",
                },
                new ReplacementFilterItem
                {
                    Id = oldReplacementFilterItem1Id,
                    FilterGroupId = replacementFilterGroupId,
                    Label = "Old replacement filter item 1",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterItemUpdates =
                    [
                        new() { OriginalId = originalFilterItem1Id, NewReplacementId = newReplacementFilterItem1Id },
                    ],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            var itemDto = Assert.Single(dto.FilterItems);
            Assert.Equal(newReplacementFilterItem1Id, itemDto.ReplacementId);
            Assert.Equal("New replacement filter item 1", itemDto.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, itemDto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();
            var filter = dbMapping.FilterMappings[originalFilterId];
            var group = filter.FilterGroupMappings[originalFilterGroupId];

            var item1 = group.FilterItemMappings[originalFilterItem1Id];
            Assert.Equal(newReplacementFilterItem1Id, item1.ReplacementId);
            Assert.Equal("New replacement filter item 1", item1.ReplacementLabel);
            Assert.Equal(MapStatus.ManuallySet, item1.Status);

            // oldReplacementFilterItem1Id is available again simply because no live mapping claims it any more
            // (asserted above) - the candidate catalogue itself is immutable and stays as it was.
            Assert.Equal(2, dbMapping.ReplacementFilterItems.Count);
            Assert.Contains(dbMapping.ReplacementFilterItems, i => i.Id == oldReplacementFilterItem1Id);
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_OriginalFilterItemNotFound_Fail()
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

        var itemDoesNotExistId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>(),
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterItemUpdates = [new() { OriginalId = itemDoesNotExistId, NewReplacementId = null }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                expectedCode: "FilterItemMatchingOriginalIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_UnmappedReplacementFilterItemNotFound_Fail()
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

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroupId = Guid.NewGuid();
        var originalFilterItemId = Guid.NewGuid();
        var replacementItemDoesNotExistId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        Status = MapStatus.AutoSet,
                        ReplacementId = Guid.NewGuid(),
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroupId,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroupId,
                                    Status = MapStatus.AutoSet,
                                    ReplacementId = Guid.NewGuid(),
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterItemId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterItemId,
                                                Status = MapStatus.Unset,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterItemUpdates =
                    [
                        new() { OriginalId = originalFilterItemId, NewReplacementId = replacementItemDoesNotExistId },
                    ],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                expectedCode: "UnmappedFilterItemMatchingReplacementIdNotFound"
            );
        }
    }

    [Fact]
    public async Task UpdateFilterMappings_MapFilterThenGroupThenItemInSingleRequest_Success()
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

        var originalFilterId = Guid.NewGuid();
        var originalFilterGroupId = Guid.NewGuid();
        var originalFilterItemId = Guid.NewGuid();

        var replacementFilterId = Guid.NewGuid();
        var replacementFilterGroupId = Guid.NewGuid();
        var replacementFilterItemId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            FilterMappings = new Dictionary<Guid, FilterMapping>
            {
                {
                    originalFilterId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterId,
                        OriginalLabel = "Original filter label",
                        Status = MapStatus.Unset,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalFilterGroupId,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalFilterGroupId,
                                    OriginalLabel = "Original filter group label",
                                    Status = MapStatus.ParentNotMapped,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalFilterItemId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalFilterItemId,
                                                OriginalLabel = "Original filter item label",
                                                Status = MapStatus.ParentNotMapped,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
            ReplacementFilters =
            [
                new ReplacementFilter
                {
                    Id = replacementFilterId,
                    Label = "Replacement filter label",
                    ColumnName = "replacement_filter",
                },
            ],
            ReplacementFilterGroups =
            [
                new ReplacementFilterGroup
                {
                    Id = replacementFilterGroupId,
                    FilterId = replacementFilterId,
                    Label = "Different group label",
                },
            ],
            ReplacementFilterItems =
            [
                new ReplacementFilterItem
                {
                    Id = replacementFilterItemId,
                    FilterGroupId = replacementFilterGroupId,
                    Label = "Different item label",
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

            var result = await service.UpdateFilterMappings(
                releaseVersion.Id,
                new FilterMappingUpdatesRequest
                {
                    OriginalDataFileId = originalDataFileId,
                    ReplacementDataFileId = replacementDataFileId,
                    FilterUpdates = [new() { OriginalId = originalFilterId, NewReplacementId = replacementFilterId }],
                    FilterGroupUpdates =
                    [
                        new() { OriginalId = originalFilterGroupId, NewReplacementId = replacementFilterGroupId },
                    ],
                    FilterItemUpdates =
                    [
                        new() { OriginalId = originalFilterItemId, NewReplacementId = replacementFilterItemId },
                    ],
                },
                CancellationToken.None
            );

            var dto = result.AssertRight();
            var filterDto = Assert.Single(dto.Filters);
            Assert.Equal(replacementFilterId, filterDto.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, filterDto.Status);

            var groupDto = Assert.Single(dto.FilterGroups);
            Assert.Equal(replacementFilterGroupId, groupDto.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, groupDto.Status);

            var itemDto = Assert.Single(dto.FilterItems);
            Assert.Equal(replacementFilterItemId, itemDto.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, itemDto.Status);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbMapping = contentDbContext.DataSetMappings.Single();
            var filter = dbMapping.FilterMappings[originalFilterId];
            Assert.Equal(replacementFilterId, filter.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, filter.Status);

            var group = filter.FilterGroupMappings[originalFilterGroupId];
            Assert.Equal(replacementFilterGroupId, group.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, group.Status);

            var item = group.FilterItemMappings[originalFilterItemId];
            Assert.Equal(replacementFilterItemId, item.ReplacementId);
            Assert.Equal(MapStatus.ManuallySet, item.Status);

            // The candidate catalogue itself is immutable and unaffected by the mappings above - it still contains
            // exactly the entries it started with.
            Assert.Equal([replacementFilterId], dbMapping.ReplacementFilters.Select(f => f.Id));
            Assert.Equal([replacementFilterGroupId], dbMapping.ReplacementFilterGroups.Select(g => g.Id));
            Assert.Equal([replacementFilterItemId], dbMapping.ReplacementFilterItems.Select(i => i.Id));
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
