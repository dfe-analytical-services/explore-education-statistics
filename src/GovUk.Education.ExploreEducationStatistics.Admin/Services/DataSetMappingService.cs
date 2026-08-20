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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetMappingService(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext,
    IUserService userService
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
                await ValidateMapping(
                    releaseVersion,
                    request.OriginalDataFileId,
                    request.ReplacementDataFileId,
                    cancellationToken
                )
            )
            .OnSuccess(async validated =>
            {
                var (mapping, replacementReleaseFile) = validated;

                var replacementFilters = await statisticsDbContext
                    .Filter.AsNoTracking()
                    .Include(f => f.FilterGroups)
                        .ThenInclude(fg => fg.FilterItems)
                    .Where(f => f.SubjectId == replacementReleaseFile.File.SubjectId!.Value)
                    .ToListAsync(cancellationToken);

                // Filters
                var updatedFilterMappings = request
                    .FilterUpdates.Select(filterUpdate =>
                        mapping.UpdateFilterMapping(
                            replacementFilters,
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
                var updatedGroupMappings = request
                    .FilterGroupUpdates.Select(groupUpdate =>
                        mapping.UpdateFilterGroupMapping(
                            replacementFilters,
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
                var updatedItemMappings = request
                    .FilterItemUpdates.Select(itemUpdate =>
                        mapping.UpdateFilterItemMapping(
                            replacementFilters,
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

                // The mutations above all happen on the in-memory object graph beneath FilterMappings. EF can't see
                // through the JSON column conversion on that property, so we must mark it dirty explicitly for the
                // changes to be persisted. ReplacementFilters/ReplacementFilterGroups/ReplacementFilterItems are
                // never mutated here, so they don't need marking.
                contentDbContext.Entry(mapping).Property(x => x.FilterMappings).IsModified = true;

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
                await ValidateMapping(
                    releaseVersion,
                    request.OriginalDataFileId,
                    request.ReplacementDataFileId,
                    cancellationToken
                )
            )
            .OnSuccess(async validated =>
            {
                var (mapping, replacementReleaseFile) = validated;

                var replacementIndicators = await statisticsDbContext
                    .Indicator.AsNoTracking()
                    .Include(i => i.IndicatorGroup)
                    .Where(i => i.IndicatorGroup.SubjectId == replacementReleaseFile.File.SubjectId!.Value)
                    .ToListAsync(cancellationToken);

                var updatedMappings = request
                    .Updates.Select(update =>
                        mapping.UpdateIndicatorMapping(
                            replacementIndicators,
                            update.OriginalId,
                            update.NewReplacementId
                        )
                    )
                    .ToList(); // cannot be async!

                // The mutations above happen on the in-memory object graph beneath IndicatorMappings. EF can't see
                // through the JSON column conversion on that property, so we must mark it dirty explicitly for the
                // changes to be persisted.
                contentDbContext.Entry(mapping).Property(x => x.IndicatorMappings).IsModified = true;

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return updatedMappings
                    .OnSuccessAll()
                    .OnSuccess(_ => mapping.IndicatorMappings.Values.Select(IndicatorMappingDto.FromModel).ToList());
            });
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
                await ValidateMapping(
                    releaseVersion,
                    request.OriginalDataFileId,
                    request.ReplacementDataFileId,
                    cancellationToken
                )
            )
            .OnSuccess(async validated =>
            {
                var mapping = validated.Mapping;

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

    private async Task<Either<ActionResult, (DataSetMapping Mapping, ReleaseFile ReplacementFile)>> ValidateMapping(
        ReleaseVersion releaseVersion,
        Guid originalDataFileId,
        Guid replacementDataFileId,
        CancellationToken cancellationToken
    )
    {
        var mapping = await contentDbContext.DataSetMappings.SingleOrDefaultAsync(
            map => map.OriginalDataFileId == originalDataFileId && map.ReplacementDataFileId == replacementDataFileId,
            cancellationToken
        );

        if (mapping == null)
        {
            return new NotFoundResult();
        }

        // NOTE: We assume that both data files have been validated as FileType.Data previously.

        var originalReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(
            rf => rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == mapping.OriginalDataFileId,
            cancellationToken
        );
        if (!originalReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = nameof(IndicatorMappingUpdatesRequest.OriginalDataFileId),
                    Code = "OriginalDataFileIdNotLinkedToReleaseVersion",
                    Message = "The original data file is not linked to the release version",
                }
            );
        }

        var replacementReleaseFile = await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .SingleOrDefaultAsync(
                rf => rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == mapping.ReplacementDataFileId,
                cancellationToken
            );
        if (replacementReleaseFile == null)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = nameof(IndicatorMappingUpdatesRequest.ReplacementDataFileId),
                    Code = "ReplacementDataFileIdNotLinkedToReleaseVersion",
                    Message = "The replacement data set is not linked to the release version",
                }
            );
        }

        return (mapping, replacementReleaseFile);
    }
}
