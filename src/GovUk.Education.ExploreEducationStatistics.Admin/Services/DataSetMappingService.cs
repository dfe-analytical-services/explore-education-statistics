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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetMappingService(
    ContentDbContext contentDbContext,
    IUserService userService,
    IFilterMappingService filterMappingService,
    IFilterGroupMappingService filterGroupMappingService,
    IFilterItemMappingService filterItemMappingService
) : IDataSetMappingService
{
    public async Task<Either<ActionResult, FiltersMappingDto>> UpdateFilterMappings(
        Guid releaseVersionId,
        FilterMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .SingleOrNotFound()
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await ValidateMapping(releaseVersion, request.OriginalDataFileId, request.ReplacementDataFileId)
            )
            .OnSuccess(async mapping =>
            {
                // Filters
                var updatedFilterMappings = request
                    .FilterUpdates.Select(filterUpdate =>
                        filterMappingService.UpdateFilterMapping(
                            mapping,
                            filterUpdate.OriginalId,
                            filterUpdate.NewReplacementId
                        )
                    )
                    .ToList();

                var filterErrors = updatedFilterMappings
                    .Where(updated => updated.Error != null)
                    .Select(updated => updated.Error!)
                    .ToList();
                if (!filterErrors.IsNullOrEmpty())
                {
                    return new Either<ActionResult, FiltersMappingDto>(ValidationResult(filterErrors));
                }

                var filterMappingDto = new FiltersMappingDto
                {
                    Filters = updatedFilterMappings
                        .Select(updated => FilterMappingDto.FromModel(updated.FilterMapping!))
                        .ToList(),
                };

                // FilterGroups
                var originalGroupIdToGroupsMap = mapping
                    .FilterMappings.Values.SelectMany(
                        fm => fm.FilterGroupMappings.Values,
                        (fm, gm) => (FilterMap: fm, GroupMap: gm)
                    )
                    .ToDictionary(x => x.GroupMap.OriginalId);

                var updatedGroupMappings = request
                    .FilterGroupUpdates.Select(groupUpdate =>
                        filterGroupMappingService.UpdateFilterGroupMapping(
                            mapping,
                            originalGroupIdToGroupsMap,
                            groupUpdate.OriginalId,
                            groupUpdate.NewReplacementId
                        )
                    )
                    .ToList();

                var groupErrors = updatedGroupMappings
                    .Where(updated => updated.Error != null)
                    .Select(updated => updated.Error!)
                    .ToList();
                if (!groupErrors.IsNullOrEmpty())
                {
                    return ValidationResult(groupErrors);
                }

                filterMappingDto.FilterGroups = updatedGroupMappings
                    .Select(updated => FilterGroupMappingDto.FromModel(updated.FilterGroupMapping!))
                    .ToList();

                // FilterItems
                var originalItemIdToItem = mapping
                    .FilterMappings.Values.SelectMany(fm => fm.FilterGroupMappings.Values)
                    .SelectMany(
                        fg => fg.FilterItemMappings.Values,
                        (group, item) => (FilterGroup: group, FilterItem: item)
                    )
                    .ToDictionary(x => x.FilterItem.OriginalId);

                var updatedItemMappings = request
                    .FilterItemUpdates.Select(itemUpdate =>
                        filterItemMappingService.UpdateFilterItemMapping(
                            mapping,
                            originalItemIdToItem,
                            itemUpdate.OriginalId,
                            itemUpdate.NewReplacementId
                        )
                    )
                    .ToList();

                var itemErrors = updatedItemMappings
                    .Where(updated => updated.Error != null)
                    .Select(updated => updated.Error!)
                    .ToList();
                if (!itemErrors.IsNullOrEmpty())
                {
                    return ValidationResult(itemErrors);
                }

                filterMappingDto.FilterItems = updatedItemMappings
                    .Select(updated => FilterItemMappingDto.FromModel(updated.FilterItemMapping!))
                    .ToList();

                await contentDbContext.SaveChangesAsync(cancellationToken);

                return filterMappingDto;
            });
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
                await ValidateMapping(releaseVersion, request.OriginalDataFileId, request.ReplacementDataFileId)
            )
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateIndicatorMapping(mapping, update.OriginalId, update.NewReplacementId)
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
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!dataSetMapping.IndicatorMappings.TryGetValue(originalId, out var indicatorMapping))
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "IndicatorMatchingOriginalIdNotFound",
                    Message = $"Could not find indicator mapping matching original id \"{originalId}\"",
                }
            );
        }

        if (indicatorMapping.ReplacementId == newReplacementId && indicatorMapping.Status == MapStatus.ManuallySet)
        {
            return indicatorMapping; // it is already mapped, so can skip
        }

        var availableUnmappedIndicator = dataSetMapping.UnmappedReplacementIndicators.SingleOrDefault(
            unmappedIndicator => unmappedIndicator.Id == newReplacementId
        );

        if (newReplacementId != null && availableUnmappedIndicator == null)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedIndicatorMatchingReplacementIdNotFound",
                    Message =
                        $"No available unmapped indicator matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (availableUnmappedIndicator != null)
        {
            // remove availableUnmappedIndicator from UnmappedReplacementIndicators as it's about to become mapped
            dataSetMapping.UnmappedReplacementIndicators.Remove(availableUnmappedIndicator);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementIndicators).IsModified = true;
        }

        if (indicatorMapping.ReplacementId != null && indicatorMapping.ReplacementId != newReplacementId)
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
                await ValidateMapping(releaseVersion, request.OriginalDataFileId, request.ReplacementDataFileId)
            )
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateLocationMapping(mapping, update.OriginalId, update.NewReplacementId)
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
        if (!dataSetMapping.LocationMappings.TryGetValue(originalLocationId, out var locationMapping))
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
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
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedLocationMatchingReplacementLocationIdNotFound",
                    Message = $"No available unmapped location matching replacement id \"{newReplacementLocationId}\"",
                }
            );
        }

        if (
            newReplacementLocationId != null
            && availableUnmappedLocation != null
            && availableUnmappedLocation.GeographicLevel != locationMapping.OriginalGeographicLevel
        )
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
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
        Guid originalDataFileId,
        Guid replacementDataFileId
    )
    {
        var mapping = await contentDbContext.DataSetMappings.SingleOrDefaultAsync(map =>
            map.OriginalDataFileId == originalDataFileId && map.ReplacementDataFileId == replacementDataFileId
        );

        if (mapping == null)
        {
            return new Either<ActionResult, DataSetMapping>(new NotFoundResult());
        }

        var originalReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(rf =>
            rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == mapping.OriginalDataFileId
        );
        if (!originalReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(IndicatorMappingUpdatesRequest.OriginalDataFileId)}",
                    Code = "OriginalDataFileIdNotLinkedToReleaseVersion",
                    Message = $"The original data file is not linked to the release version",
                }
            );
        }
        var replacementReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(rf =>
            rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == mapping.ReplacementDataFileId
        );
        if (!replacementReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(IndicatorMappingUpdatesRequest.ReplacementDataFileId)}",
                    Code = "ReplacementDataFileIdNotLinkedToReleaseVersion",
                    Message = $"The replacement data set is not linked to the release version",
                }
            );
        }

        return mapping;
    }
}
