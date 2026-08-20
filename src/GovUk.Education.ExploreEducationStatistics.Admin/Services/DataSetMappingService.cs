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
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetMappingService(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext,
    ILocationRepository locationRepository,
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
                var (mapping, replacementReleaseFile) = validated;

                var replacementLocations = (
                    await locationRepository.GetDistinctForSubject(replacementReleaseFile.File.SubjectId!.Value)
                ).ToList();

                var updatedMappings = request
                    .Updates.Select(update =>
                        mapping.UpdateLocationMapping(replacementLocations, update.OriginalId, update.NewReplacementId)
                    )
                    .ToList(); // cannot be async!

                // The mutations above happen on the in-memory object graph beneath LocationMappings. EF can't see
                // through the JSON column conversion on that property, so we must mark it dirty explicitly for the
                // changes to be persisted.
                contentDbContext.Entry(mapping).Property(x => x.LocationMappings).IsModified = true;

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return updatedMappings
                    .OnSuccessAll()
                    .OnSuccess(_ => mapping.LocationMappings.Values.Select(LocationMappingDto.FromModel).ToList());
            });
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
