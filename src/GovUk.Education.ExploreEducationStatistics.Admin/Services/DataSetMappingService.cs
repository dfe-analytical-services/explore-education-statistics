#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<Either<ActionResult, List<IndicatorMappingDto>>> UpdateIndicatorMappings(
        IndicatorMappingUpdatesRequest request
    )
    {
        return await contentDbContext
            .DataSetMappings.Where(map =>
                map.OriginalDataSetId == request.OriginalDataSetId
                && map.ReplacementDataSetId == request.ReplacementDataSetId
            )
            .SingleOrNotFoundAsync()
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateIndicatorMapping(mapping, update.OriginalColumnName, update.NewReplacementColumnName)
                    )
                    .ToList(); // cannot be async!

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync();

                return updatedMappings
                    .OnSuccessAll()
                    .OnSuccess(_ => mapping.IndicatorMappings.Values.Select(IndicatorMappingDto.FromModel).ToList());
            });
    }

    private Either<ActionResult, IndicatorMapping> UpdateIndicatorMapping(
        DataSetMapping dataSetMapping,
        string originalColumnName,
        string? newReplacementColumnName = null
    )
    {
        var indicatorMapping = dataSetMapping.IndicatorMappings.Values.SingleOrDefault(map =>
            map.OriginalColumnName == originalColumnName
        );
        if (indicatorMapping == null)
        {
            return Common.Validators.ValidationUtils.ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(IndicatorMappingUpdateRequest.OriginalColumnName)}",
                    Code = "IndicatorMatchingOriginalColumnNameNotFound",
                    Message =
                        $"Could not find indicator mapping matching original column name \"{originalColumnName}\"",
                }
            );
        }

        if (
            indicatorMapping.ReplacementColumnName == newReplacementColumnName
            && indicatorMapping.Status == MapStatus.ManuallySet
        )
        {
            return indicatorMapping; // it is already mapped, so can skip
        }

        var availableUnmappedIndicator = dataSetMapping.UnmappedReplacementIndicators.SingleOrDefault(
            unmappedIndicator => unmappedIndicator.ColumnName == newReplacementColumnName
        );

        if (newReplacementColumnName != null && availableUnmappedIndicator == null)
        {
            return Common.Validators.ValidationUtils.ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(IndicatorMappingUpdateRequest.NewReplacementColumnName)}",
                    Code = "UnmappedIndicatorMatchingReplacementColumnNameNotFound",
                    Message =
                        $"No available unmapped indicator matching replacement column name \"{newReplacementColumnName}\"",
                }
            );
        }

        if (availableUnmappedIndicator != null)
        {
            // remove availableUnmappedIndicator from UnmappedReplacementIndicators as it's about to become mapped
            dataSetMapping.UnmappedReplacementIndicators.Remove(availableUnmappedIndicator);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementIndicators).IsModified = true;
        }

        if (
            indicatorMapping.ReplacementId != null
            && indicatorMapping.ReplacementColumnName != newReplacementColumnName
        )
        {
            // We need to move the preexisting mapped indicator into UnmappedReplacementIndicators, as it will be overwritten
            var newlyUnmappedIndicator = new UnmappedIndicator
            {
                Id = indicatorMapping.ReplacementId.Value,
                ColumnName = indicatorMapping.ReplacementColumnName!,
                Label = indicatorMapping.ReplacementLabel!,
                GroupId = indicatorMapping.ReplacementGroupId!.Value,
                GroupLabel = indicatorMapping.ReplacementGroupLabel!,
            };
            dataSetMapping.UnmappedReplacementIndicators.Add(newlyUnmappedIndicator);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementIndicators).IsModified = true;
        }

        // mapping.Original* properties should never change
        indicatorMapping.ReplacementId = availableUnmappedIndicator?.Id;
        indicatorMapping.ReplacementColumnName = availableUnmappedIndicator?.ColumnName;
        indicatorMapping.ReplacementLabel = availableUnmappedIndicator?.Label;
        indicatorMapping.ReplacementGroupId = availableUnmappedIndicator?.GroupId;
        indicatorMapping.ReplacementGroupLabel = availableUnmappedIndicator?.GroupLabel;
        indicatorMapping.Status = MapStatus.ManuallySet;

        contentDbContext.Entry(dataSetMapping).Property(x => x.IndicatorMappings).IsModified = true;

        return indicatorMapping;
    }
}
