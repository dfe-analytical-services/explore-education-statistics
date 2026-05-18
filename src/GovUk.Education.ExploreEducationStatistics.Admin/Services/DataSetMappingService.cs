#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetMappingService(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext,
    IUserService userService
) : IDataSetMappingService
{
    public async Task<DataSetMapping> GetOrCreateMapping(
        Guid originalSubjectId,
        Guid replacementSubjectId,
        CancellationToken cancellationToken = default
    )
    {
        var mapping =
            await contentDbContext.DataSetMappings.SingleOrDefaultAsync(
                map => map.OriginalDataSetId == originalSubjectId && map.ReplacementDataSetId == replacementSubjectId,
                cancellationToken
            ) ?? await GenerateAndSaveInitialDataSetMapping(originalSubjectId, replacementSubjectId, cancellationToken);

        // TODO EES-7126 Remove this code - it was only needed when first introducing LocationMappings to ensure they were set
        if (mapping.LocationMappings.IsNullOrEmpty() && mapping.UnmappedReplacementLocations.IsNullOrEmpty())
        {
            var (initialLocationMappingDictionary, unmappedReplacementLocations) = await GenerateInitialLocationMapping(
                mapping.OriginalDataSetId,
                mapping.ReplacementDataSetId,
                cancellationToken
            );

            mapping.LocationMappings = initialLocationMappingDictionary;
            mapping.UnmappedReplacementLocations = unmappedReplacementLocations;
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return mapping;
    }

    private async Task<DataSetMapping> GenerateAndSaveInitialDataSetMapping(
        Guid originalDataSetId,
        Guid replacementDataSetId,
        CancellationToken cancellationToken
    )
    {
        var (initialIndicatorMappingDictionary, unmappedReplacementIndicators) = await GenerateInitialIndicatorMapping(
            originalDataSetId,
            replacementDataSetId,
            cancellationToken
        );

        var (initialLocationMappingDictionary, unmappedReplacementLocations) = await GenerateInitialLocationMapping(
            originalDataSetId,
            replacementDataSetId,
            cancellationToken
        );

        var newMapping = new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
            IndicatorMappings = initialIndicatorMappingDictionary,
            UnmappedReplacementIndicators = unmappedReplacementIndicators,
            LocationMappings = initialLocationMappingDictionary,
            UnmappedReplacementLocations = unmappedReplacementLocations,
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
        Guid originalDataSetId,
        Guid replacementDataSetId,
        CancellationToken cancellationToken
    )
    {
        var originalLocations = await statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == originalDataSetId)
            .Select(observation => observation.Location)
            .Distinct()
            .ToListAsync(cancellationToken);

        var replacementLocationMap = await statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == replacementDataSetId)
            .Select(observation => observation.Location)
            .Distinct()
            .ToDictionaryAsync(location => location.Id, location => location, cancellationToken);

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
                    replacementLocation = candidateReplacements.SingleOrDefault();
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

    public async Task<Either<ActionResult, List<IndicatorMappingDto>>> UpdateIndicatorMappings(
        Guid releaseVersionId,
        IndicatorMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .SingleOrNotFound()
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await ValidateMapping(releaseVersion, request.OriginalDataSetId, request.ReplacementDataSetId)
            )
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateIndicatorMapping(mapping, update.OriginalColumnName, update.NewReplacementColumnName)
                    )
                    .ToList(); // cannot be async!

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync(cancellationToken);

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
            return ValidationResult(
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
            return ValidationResult(
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

    public async Task<Either<ActionResult, List<LocationMappingDto>>> UpdateLocationMappings(
        Guid releaseVersionId,
        LocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .SingleOrNotFound()
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await ValidateMapping(releaseVersion, request.OriginalDataSetId, request.ReplacementDataSetId)
            )
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateLocationMapping(mapping, update.OriginalLocationId, update.NewReplacementLocationId)
                    )
                    .ToList(); // cannot be async!

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return updatedMappings
                    .OnSuccessAll()
                    .OnSuccess(_ => mapping.LocationMappings.Values.Select(LocationMappingDto.FromModel).ToList());
            });
    }

    private Either<ActionResult, LocationMapping> UpdateLocationMapping(
        DataSetMapping dataSetMapping,
        Guid originalLocationId,
        Guid? newReplacementLocationId = null
    )
    {
        var locationMapping = dataSetMapping.LocationMappings.Values.SingleOrDefault(map =>
            map.OriginalId == originalLocationId
        );
        if (locationMapping == null)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(LocationMappingUpdateRequest.OriginalLocationId)}",
                    Code = "LocationMatchingOriginalIdNameNotFound",
                    Message = $"Could not find location mapping matching original location id \"{originalLocationId}\"",
                }
            );
        }

        if (
            locationMapping.ReplacementId == newReplacementLocationId
            && locationMapping.Status == MapStatus.ManuallySet
        )
        {
            return locationMapping; // it is already mapped, so can skip
        }

        var availableUnmappedLocation = dataSetMapping.UnmappedReplacementLocations.SingleOrDefault(unmappedLocation =>
            unmappedLocation.Id == newReplacementLocationId
        );

        if (newReplacementLocationId != null && availableUnmappedLocation == null)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(LocationMappingUpdateRequest.NewReplacementLocationId)}",
                    Code = "UnmappedLocationMatchingReplacementLocationIdNotFound",
                    Message = $"No available unmapped location matching replacement id \"{newReplacementLocationId}\"",
                }
            );
        }

        if (
            newReplacementLocationId != null
            && availableUnmappedLocation != null
            && availableUnmappedLocation!.GeographicLevel != locationMapping.OriginalGeographicLevel
        )
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(LocationMappingUpdateRequest.NewReplacementLocationId)}",
                    Code = "UnmappedLocationHasDifferentGeographicLevelAsOriginalLocation",
                    Message =
                        $"The replacement location has a different geographic level than the original location. Replacement id: \"{newReplacementLocationId}\"",
                }
            );
        }

        if (availableUnmappedLocation != null)
        {
            // remove availableUnmappedLocation from UnmappedReplacementLocations as it's about to become mapped
            dataSetMapping.UnmappedReplacementLocations.Remove(availableUnmappedLocation);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementLocations).IsModified = true;
        }

        if (locationMapping.ReplacementId != null && locationMapping.ReplacementId != newReplacementLocationId)
        {
            // We need to move the preexisting mapped location into UnmappedReplacementLocations, as it will be overwritten
            var newlyUnmappedLocation = new UnmappedLocation
            {
                Id = locationMapping.ReplacementId.Value,
                GeographicLevel = locationMapping.ReplacementGeographicLevel!.Value,
                Code = locationMapping.ReplacementCode!,
                Name = locationMapping.ReplacementName!,
            };
            dataSetMapping.UnmappedReplacementLocations.Add(newlyUnmappedLocation);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementLocations).IsModified = true;
        }

        // locationMapping.Original* properties should never change
        locationMapping.ReplacementId = availableUnmappedLocation?.Id;
        locationMapping.ReplacementGeographicLevel = availableUnmappedLocation?.GeographicLevel;
        locationMapping.ReplacementCode = availableUnmappedLocation?.Code;
        locationMapping.ReplacementName = availableUnmappedLocation?.Name;
        locationMapping.Status = MapStatus.ManuallySet;

        contentDbContext.Entry(dataSetMapping).Property(x => x.LocationMappings).IsModified = true;

        return locationMapping;
    }

    private async Task<Either<ActionResult, DataSetMapping>> ValidateMapping(
        ReleaseVersion releaseVersion,
        Guid originalDataSetId,
        Guid replacementDataSetId
    )
    {
        var mapping = await contentDbContext.DataSetMappings.SingleOrDefaultAsync(map =>
            map.OriginalDataSetId == originalDataSetId && map.ReplacementDataSetId == replacementDataSetId
        );

        if (mapping == null)
        {
            return new Either<ActionResult, DataSetMapping>(new NotFoundResult());
        }

        var originalReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(rf =>
            rf.ReleaseVersionId == releaseVersion.Id && rf.File.SubjectId == mapping.OriginalDataSetId
        );
        if (!originalReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(IndicatorMappingUpdatesRequest.OriginalDataSetId)}",
                    Code = "OriginalDataSetIdNotLinkedToReleaseVersion",
                    Message = $"The original data set is not linked to the release version",
                }
            );
        }
        var replacementReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(rf =>
            rf.ReleaseVersionId == releaseVersion.Id && rf.File.SubjectId == mapping.ReplacementDataSetId
        );
        if (!replacementReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(IndicatorMappingUpdatesRequest.ReplacementDataSetId)}",
                    Code = "ReplacementDataSetIdNotLinkedToReleaseVersion",
                    Message = $"The replacement data set is not linked to the release version",
                }
            );
        }

        return mapping;
    }
}
