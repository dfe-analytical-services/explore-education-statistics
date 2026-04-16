#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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
        var originalDataSetId = Guid.NewGuid();
        var replacementDataSetId = Guid.NewGuid();

        var originalIndicatorId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
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
                originalSubjectId: originalDataSetId,
                replacementSubjectId: replacementDataSetId,
                CancellationToken.None
            );

            Assert.Equal(originalDataSetId, result.OriginalDataSetId);
            Assert.Equal(replacementDataSetId, result.ReplacementDataSetId);

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
        var originalDataSetId = Guid.NewGuid();
        var replacementDataSetId = Guid.NewGuid();

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
            SubjectId = originalDataSetId,
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
            SubjectId = replacementDataSetId,
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
            SubjectId = replacementDataSetId,
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
                originalSubjectId: originalDataSetId,
                replacementSubjectId: replacementDataSetId,
                CancellationToken.None
            );

            var expectedMapping = new DataSetMapping
            {
                OriginalDataSetId = originalDataSetId,
                ReplacementDataSetId = replacementDataSetId,
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
        var originalDataSetId = Guid.NewGuid();
        var replacementDataSetId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();
        var originalIndicator2Id = Guid.NewGuid();
        var originalIndicator3Id = Guid.NewGuid();
        var originalIndicator4Id = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
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
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataSetId = originalDataSetId,
                    ReplacementDataSetId = replacementDataSetId,
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
                }
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
    public async Task UpdateIndicatorMapping_NoDataSetMapping_NotFound()
    {
        var originalDataSetId = Guid.NewGuid();
        var replacementDataSetId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
            UnmappedReplacementIndicators = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataSetId = originalDataSetId,
                    ReplacementDataSetId = Guid.NewGuid(),
                    Updates = [],
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateIndicatorMapping_OriginalIndicatorNotFound_Fail()
    {
        var originalDataSetId = Guid.NewGuid();
        var replacementDataSetId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
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
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataSetId = originalDataSetId,
                    ReplacementDataSetId = replacementDataSetId,
                    Updates = [new() { OriginalColumnName = "does_not_exist", NewReplacementColumnName = null }],
                }
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
        var originalDataSetId = Guid.NewGuid();
        var replacementDataSetId = Guid.NewGuid();

        var originalIndicator1Id = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
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
            contentDbContext.DataSetMappings.Add(mapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupDataSetMappingService(contentDbContext);

            var result = await service.UpdateIndicatorMappings(
                new IndicatorMappingUpdatesRequest
                {
                    OriginalDataSetId = originalDataSetId,
                    ReplacementDataSetId = replacementDataSetId,
                    Updates =
                    [
                        new()
                        {
                            OriginalColumnName = "original_indicator_1",
                            NewReplacementColumnName = "replacement_indicator_already_mapped",
                        },
                    ],
                }
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

    private static DataSetMappingService SetupDataSetMappingService(
        ContentDbContext contentDbContext,
        StatisticsDbContext? statisticsDbContext = null
    )
    {
        return new DataSetMappingService(
            contentDbContext,
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(MockBehavior.Strict)
        );
    }
}
