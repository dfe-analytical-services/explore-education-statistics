#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetMappingService(ContentDbContext contentDbContext, StatisticsDbContext statisticsDbContext)
    : IDataSetMappingService
{
    public async Task<DataSetMapping> GetOrCreateMapping(
        Guid originalSubjectId,
        Guid replacementSubjectId,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext.DataSetMappings.SingleOrDefaultAsync(
                map => map.OriginalDataSetId == originalSubjectId && map.ReplacementDataSetId == replacementSubjectId,
                cancellationToken
            ) ?? await GenerateAndSaveInitialDataSetMapping(originalSubjectId, replacementSubjectId, cancellationToken);
    }

    private async Task<DataSetMapping> GenerateAndSaveInitialDataSetMapping(
        Guid originalDataSetId,
        Guid replacementDataSetId,
        CancellationToken cancellationToken
    )
    {
        var (initialIndicatorMappingDictionary, unmappedReplacementIndicatorIds) =
            await GenerateInitialIndicatorMapping(originalDataSetId, replacementDataSetId, cancellationToken);

        var newMapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
            IndicatorMappings = initialIndicatorMappingDictionary,
            UnmappedReplacementIndicators = unmappedReplacementIndicatorIds,
        };

        contentDbContext.DataSetMappings.Add(newMapping);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newMapping;
    }

    private async Task<(Dictionary<Guid, IndicatorMapping>, List<UnmappedIndicator>)> GenerateInitialIndicatorMapping(
        Guid originalDataSetId,
        Guid replacementDataSetId,
        CancellationToken cancellationToken
    )
    {
        var originalIndicators = await statisticsDbContext
            .Indicator.Include(i => i.IndicatorGroup)
            .Where(i => i.IndicatorGroup.SubjectId == originalDataSetId)
            .ToListAsync(cancellationToken);

        var replacementIndicatorNameToIndicatorMap = await statisticsDbContext
            .Indicator.Include(i => i.IndicatorGroup)
            .Where(i => i.IndicatorGroup.SubjectId == replacementDataSetId)
            .ToDictionaryAsync(i => i.Name, i => i, cancellationToken);

        var initialMappingDictionary = originalIndicators.ToDictionary(
            originalIndicator => originalIndicator.Id,
            originalIndicator =>
            {
                // Only if a replacement indicator has the same column name as an original indicator AND the same group
                // label, we auto map it.
                if (
                    replacementIndicatorNameToIndicatorMap.TryGetValue(
                        originalIndicator.Name,
                        out var replacementIndicator
                    )
                )
                {
                    if (replacementIndicator.IndicatorGroup.Label != originalIndicator.IndicatorGroup.Label)
                    {
                        replacementIndicator = null;
                    }
                }

                return new IndicatorMapping
                {
                    OriginalId = originalIndicator.Id,
                    OriginalLabel = originalIndicator.Label,
                    OriginalColumnName = originalIndicator.Name,
                    OriginalGroupId = originalIndicator.IndicatorGroupId,
                    OriginalGroupLabel = originalIndicator.IndicatorGroup.Label,
                    ReplacementId = replacementIndicator?.Id,
                    ReplacementLabel = replacementIndicator?.Label,
                    ReplacementColumnName = replacementIndicator?.Name,
                    ReplacementGroupId = replacementIndicator?.IndicatorGroupId,
                    ReplacementGroupLabel = replacementIndicator?.IndicatorGroup.Label,
                    Status = replacementIndicator == null ? MapStatus.Unset : MapStatus.AutoSet,
                };
            }
        );

        var mappedReplacementIds = initialMappingDictionary
            .Values.Where(m => m.ReplacementId.HasValue)
            .Select(m => m.ReplacementId!.Value);

        var unmappedReplacementIds = replacementIndicatorNameToIndicatorMap
            .Values.ExceptBy(mappedReplacementIds, indicator => indicator.Id)
            .Select(i => new UnmappedIndicator
            {
                Id = i.Id,
                Label = i.Label,
                ColumnName = i.Name,
                GroupId = i.IndicatorGroupId,
                GroupLabel = i.IndicatorGroup.Label,
            })
            .ToList();

        return (initialMappingDictionary, unmappedReplacementIds);
    }
}
