#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DataSetMappingService(IDbContextSupplier dbContextSupplier) : IDataSetMappingService
{
    public async Task CreateInitialDataSetMappingIfReplacement(Guid replacementFileId)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();
        await using var statisticsDbContext = dbContextSupplier.CreateDbContext<StatisticsDbContext>();

        var replacementFile = await contentDbContext
            .Files.Include(f => f.Replacing)
            .SingleOrDefaultAsync(f => f.Id == replacementFileId && f.Type == FileType.Data);

        if (replacementFile?.Replacing == null)
        {
            return; // it's not an ongoing replacement so we don't need to generate a DataSetMappings entry
        }

        var originalFile = replacementFile.Replacing!;

        var (initialIndicatorMappingDictionary, unmappedReplacementIndicators) = await GenerateInitialIndicatorMapping(
            statisticsDbContext,
            originalFile.SubjectId!.Value,
            replacementFile.SubjectId!.Value
        );

        var (initialLocationMappingDictionary, unmappedReplacementLocations) = await GenerateInitialLocationMapping(
            statisticsDbContext,
            originalFile.SubjectId!.Value,
            replacementFile.SubjectId!.Value
        );

        var newMapping = new DataSetMapping
        {
            OriginalDataFileId = originalFile.Id,
            ReplacementDataFileId = replacementFile.Id,
            IndicatorMappings = initialIndicatorMappingDictionary,
            UnmappedReplacementIndicators = unmappedReplacementIndicators,
            LocationMappings = initialLocationMappingDictionary,
            UnmappedReplacementLocations = unmappedReplacementLocations,
        };

        contentDbContext.DataSetMappings.Add(newMapping);
        await contentDbContext.SaveChangesAsync();
    }

    private async Task<(Dictionary<Guid, IndicatorMapping>, List<UnmappedIndicator>)> GenerateInitialIndicatorMapping(
        StatisticsDbContext statisticsDbContext,
        Guid originalSubjectId,
        Guid replacementSubjectId
    )
    {
        var originalIndicators = await statisticsDbContext
            .Indicator.Include(i => i.IndicatorGroup)
            .Where(i => i.IndicatorGroup.SubjectId == originalSubjectId)
            .ToListAsync();

        var replacementIndicatorNameToIndicatorMap = await statisticsDbContext
            .Indicator.Include(i => i.IndicatorGroup)
            .Where(i => i.IndicatorGroup.SubjectId == replacementSubjectId)
            .ToDictionaryAsync(i => i.Name, i => i);

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

        var unmappedReplacements = replacementIndicatorNameToIndicatorMap
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

        return (initialMappingDictionary, unmappedReplacements);
    }

    private async Task<(Dictionary<Guid, LocationMapping>, List<UnmappedLocation>)> GenerateInitialLocationMapping(
        StatisticsDbContext statisticsDbContext,
        Guid originalSubjectId,
        Guid replacementSubjectId
    )
    {
        var originalLocations = await statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == originalSubjectId)
            .Select(observation => observation.Location)
            .Distinct()
            .ToListAsync();

        var replacementLocationMap = await statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == replacementSubjectId)
            .Select(observation => observation.Location)
            .Distinct()
            .ToDictionaryAsync(location => location.Id, location => location);

        var initialMappingDictionary = originalLocations.ToDictionary(
            originalLocation => originalLocation.Id,
            originalLocation =>
            {
                replacementLocationMap.TryGetValue(originalLocation.Id, out var replacementLocation);
                if (replacementLocation == null)
                {
                    // If none matching by Id, check if any matching by GeogLvl + Code. We don't check by Name to
                    // preserve behaviour from before location mapping was introduced (which allowed analysts to
                    // change/fix location names with replacements).
                    var candidateReplacements = replacementLocationMap
                        .Values.Where(location =>
                            location.GeographicLevel == originalLocation.GeographicLevel
                            && location.ToLocationAttribute().GetCodeOrFallback()
                                == originalLocation.ToLocationAttribute().GetCodeOrFallback()
                        )
                        .ToList();
                    replacementLocation = candidateReplacements.Count == 1 ? candidateReplacements[0] : null;
                }

                return new LocationMapping
                {
                    OriginalId = originalLocation.Id,
                    OriginalCode = originalLocation.ToLocationAttribute().GetCodeOrFallback(),
                    OriginalName = originalLocation.ToLocationAttribute().Name ?? "",
                    OriginalGeographicLevel = originalLocation.GeographicLevel,
                    ReplacementId = replacementLocation?.Id,
                    ReplacementCode = replacementLocation?.ToLocationAttribute().GetCodeOrFallback(),
                    ReplacementName = replacementLocation?.ToLocationAttribute().Name ?? "",
                    ReplacementGeographicLevel = replacementLocation?.GeographicLevel,
                    Status = replacementLocation == null ? MapStatus.Unset : MapStatus.AutoSet,
                };
            }
        );

        var mappedReplacementIds = initialMappingDictionary
            .Values.Where(map => map.ReplacementId.HasValue)
            .Select(map => map.ReplacementId!.Value);

        var unmappedReplacements = replacementLocationMap
            .Values.ExceptBy(mappedReplacementIds, location => location.Id)
            .Select(location => new UnmappedLocation
            {
                Id = location.Id,
                Code = location.ToLocationAttribute().GetCodeOrFallback(),
                Name = location.ToLocationAttribute().Name ?? "",
                GeographicLevel = location.GeographicLevel,
            })
            .ToList();

        return (initialMappingDictionary, unmappedReplacements);
    }
}
