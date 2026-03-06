#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
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
                        ReplacementGroupId = Guid.NewGuid(),
                        Status = MapStatus.ManuallySet,
                    }
                },
            },
            CandidateIndicatorReplacements =
            [
                new CandidateIndicator
                {
                    Id = Guid.NewGuid(),
                    Label = "Candidate indicator",
                    GroupId = Guid.NewGuid(),
                    GroupLabel = "Candidate group indicator",
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

            var candidateIndicator = Assert.Single(result.CandidateIndicatorReplacements);
            Assert.Equal(mapping.CandidateIndicatorReplacements.First(), candidateIndicator);
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
                            ReplacementLabel = replacementIndicatorA2.Label,
                            ReplacementGroupId = replacementIndicatorGroupA.Id,
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
                CandidateIndicatorReplacements =
                [
                    new CandidateIndicator
                    {
                        Id = replacementIndicatorA3.Id,
                        Label = replacementIndicatorA3.Label,
                        GroupId = replacementIndicatorGroupA.Id,
                        GroupLabel = replacementIndicatorGroupA.Label,
                    },
                    new CandidateIndicator
                    {
                        Id = replacementIndicatorA4.Id,
                        Label = replacementIndicatorA4.Label,
                        GroupId = replacementIndicatorGroupB.Id,
                        GroupLabel = replacementIndicatorGroupB.Label,
                    },
                ],
            };

            result.AssertDeepEqualTo(expectedMapping, ignoreProperties: [mapping => mapping.Id]);
        }
    }

    private static DataSetMappingService SetupDataSetMappingService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext
    )
    {
        return new DataSetMappingService(contentDbContext, statisticsDbContext);
    }
}
