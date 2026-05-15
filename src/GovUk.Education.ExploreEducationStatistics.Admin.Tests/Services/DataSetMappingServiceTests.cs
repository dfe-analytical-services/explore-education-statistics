#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetMappingServiceTests
{
    [Fact]
    public async Task GetOrCreateMapping_FetchesDbMapping_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalIndicatorId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicatorId,
                    new IndicatorMapping
                    {
                        OriginalId = originalIndicatorId,
                        OriginalLabel = "Original label",
                        OriginalColumnName = "original_column",
                        OriginalGroupId = Guid.NewGuid(),
                        OriginalGroupLabel = "Original group label",
                        ReplacementId = Guid.NewGuid(),
                        ReplacementLabel = "Replacement label",
                        ReplacementColumnName = "replacement_column",
                        ReplacementGroupId = Guid.NewGuid(),
                        ReplacementGroupLabel = "Replacement group label",
                        Status = MapStatus.ManuallySet,
                    }
                },
            },
            UnmappedReplacementIndicators =
            [
                new UnmappedIndicator
                {
                    Id = Guid.NewGuid(),
                    Label = "Unmapped indicator",
                    ColumnName = "unmapped_indicator",
                    GroupId = Guid.NewGuid(),
                    GroupLabel = "Unmapped indicator group",
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupDataSetMappingService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.GetOrCreateMapping(
                originalDataFileId: originalDataFileId,
                replacementDataFileId: replacementDataFileId,
                originalSubjectId: Guid.NewGuid(),
                replacementSubjectId: Guid.NewGuid(),
                CancellationToken.None
            );

            Assert.Equal(originalDataFileId, result.OriginalDataFileId);
            Assert.Equal(replacementDataFileId, result.ReplacementDataFileId);

            var indicatorMappingKeyPair = Assert.Single(result.IndicatorMappings);
            Assert.Equal(mapping.IndicatorMappings.Keys.First(), indicatorMappingKeyPair.Key);
            Assert.Equal(mapping.IndicatorMappings.Values.First(), indicatorMappingKeyPair.Value);

            var unmappedIndicator = Assert.Single(result.UnmappedReplacementIndicators);
            Assert.Equal(mapping.UnmappedReplacementIndicators.First(), unmappedIndicator);
        }
    }

    [Fact]
    public async Task GetOrCreateMapping_GenerateMapping_Indicators_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalSubjectId = Guid.NewGuid();
        var replacementSubjectId = Guid.NewGuid();

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
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.IndicatorGroup.AddRange(
                originalIndicatorGroupA,
                replacementIndicatorGroupA,
                replacementIndicatorGroupB
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupDataSetMappingService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            var result = await service.GetOrCreateMapping(
                originalDataFileId: originalDataFileId,
                replacementDataFileId: replacementDataFileId,
                originalSubjectId: originalSubjectId,
                replacementSubjectId: replacementSubjectId,
                CancellationToken.None
            );

            var expectedMapping = new DataSetMapping
            {
                OriginalDataFileId = originalDataFileId,
                ReplacementDataFileId = replacementDataFileId,
                IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
                {
                    {
                        originalIndicatorA1.Id,
                        new IndicatorMapping
                        {
                            OriginalId = originalIndicatorA1.Id,
                            OriginalLabel = originalIndicatorA1.Label,
                            OriginalColumnName = originalIndicatorA1.Name,
                            OriginalGroupId = originalIndicatorGroupA.Id,
                            OriginalGroupLabel = originalIndicatorGroupA.Label,
                            Status = MapStatus.Unset,
                        }
                    },
                    {
                        originalIndicatorA2.Id,
                        new IndicatorMapping
                        {
                            OriginalId = originalIndicatorA2.Id,
                            OriginalLabel = originalIndicatorA2.Label,
                            OriginalColumnName = originalIndicatorA2.Name,
                            OriginalGroupId = originalIndicatorGroupA.Id,
                            OriginalGroupLabel = originalIndicatorGroupA.Label,
                            ReplacementId = replacementIndicatorA2.Id,
                            ReplacementColumnName = replacementIndicatorA2.Name,
                            ReplacementLabel = replacementIndicatorA2.Label,
                            ReplacementGroupId = replacementIndicatorGroupA.Id,
                            ReplacementGroupLabel = replacementIndicatorGroupA.Label,
                            Status = MapStatus.AutoSet,
                        }
                    },
                    {
                        originalIndicatorA4.Id,
                        new IndicatorMapping
                        {
                            OriginalId = originalIndicatorA4.Id,
                            OriginalLabel = originalIndicatorA4.Label,
                            OriginalColumnName = originalIndicatorA4.Name,
                            OriginalGroupId = originalIndicatorGroupA.Id,
                            OriginalGroupLabel = originalIndicatorGroupA.Label,
                            Status = MapStatus.Unset,
                        }
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
    public async Task UpdateIndicatorMapping_Success()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();
        var originalIndicator2Id = Guid.NewGuid();
        var originalIndicator3Id = Guid.NewGuid();
        var originalIndicator4Id = Guid.NewGuid();

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
                    Id = Guid.NewGuid(),
                    Label = "Replacement indicator 4 - that will be mapped to Original indicator 1",
                    ColumnName = "replacement_indicator_4",
                    GroupId = Guid.NewGuid(),
                    GroupLabel = "Replacement indicator 4 group label",
                },
                new UnmappedIndicator
                {
                    Id = Guid.NewGuid(),
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
                        new()
                        {
                            OriginalColumnName = "original_indicator_1",
                            NewReplacementColumnName = "replacement_indicator_4",
                        },
                        new()
                        {
                            OriginalColumnName = "original_indicator_2",
                            NewReplacementColumnName = "replacement_indicator_5",
                        },
                        new() { OriginalColumnName = "original_indicator_3", NewReplacementColumnName = null },
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

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
                    Updates = [new() { OriginalColumnName = "does_not_exist", NewReplacementColumnName = null }],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasError(
                expectedPath: "Updates.OriginalColumnName",
                expectedCode: "IndicatorMatchingOriginalColumnNameNotFound",
                expectedMessage: $"Could not find indicator mapping matching original column name \"does_not_exist\""
            );
        }
    }

    [Fact]
    public async Task UpdateIndicatorMapping_UnmappedReplacementIndicatorNotFound_Fail()
    {
        var originalDataFileId = Guid.NewGuid();
        var replacementDataFileId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
                            OriginalColumnName = "original_indicator_1",
                            NewReplacementColumnName = "replacement_indicator_already_mapped",
                        },
                    ],
                },
                CancellationToken.None
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasError(
                expectedPath: "Updates.NewReplacementColumnName",
                expectedCode: "UnmappedIndicatorMatchingReplacementColumnNameNotFound",
                expectedMessage: $"No available unmapped indicator matching replacement column name \"replacement_indicator_already_mapped\""
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

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };

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
            UnmappedReplacementLocations = new List<UnmappedLocation>
            {
                new()
                {
                    Id = replacementLocId,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Name = "New LA",
                    Code = "301",
                },
            },
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
        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };

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
        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };

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

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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

        var releaseVersion = new Content.Model.ReleaseVersion { Id = Guid.NewGuid() };
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
        StatisticsDbContext? statisticsDbContext = null,
        IUserService? userService = null
    )
    {
        return new DataSetMappingService(
            contentDbContext,
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object // @MarkFix add permission tests
        );
    }
}
