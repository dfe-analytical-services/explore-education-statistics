#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data.PublicDataApiClient;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semver;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IProcessorClient processorClient,
    IPublicDataApiClient publicDataApiClient,
    IUserService userService,
    IMapper mapper)
    : IDataSetVersionService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>>
        ListLiveVersions(
            Guid dataSetId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(() => publicDataDbContext.DataSets
                .AsNoTracking()
                .SingleOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken))
            .OnSuccess(async dataSet =>
            {
                var dataSetVersionsQueryable = publicDataDbContext.DataSetVersions
                    .AsNoTracking()
                    .Where(ds => ds.DataSetId == dataSet.Id)
                    .WherePublicStatus();

                var dataSetVersions = await dataSetVersionsQueryable
                    .OrderByDescending(dsv => dsv.Published)
                    .Paginate(page: page, pageSize: pageSize)
                    .ToListAsync(cancellationToken);

                var releasesVersionsByDataSetVersionId =
                    await GetReleaseFilesByDataSetVersionId(dataSetVersions, cancellationToken);

                var results = dataSetVersions
                    .Select(dsv => MapLiveVersionSummary(dsv, releasesVersionsByDataSetVersionId[dsv.Id]))
                    .ToList();

                return new PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>(
                    results,
                    totalResults: await dataSetVersionsQueryable.CountAsync(cancellationToken: cancellationToken),
                    page: page,
                    pageSize: pageSize);
            });
    }

    public async Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        var releaseFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.Id)
            .ToListAsync(cancellationToken);

        return await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.Release.ReleaseFileId))
            .Include(dataSetVersion => dataSetVersion.DataSet)
            .Select(dataSetVersion => new DataSetVersionStatusSummary(
                dataSetVersion.Id,
                dataSetVersion.DataSet.Title,
                dataSetVersion.Status)
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CreateNextVersion(
        Guid releaseFileId,
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async _ => await processorClient.CreateNextDataSetVersionMappings(
                dataSetId: dataSetId,
                releaseFileId: releaseFileId,
                cancellationToken: cancellationToken))
            .OnSuccess(async processorResponse => await publicDataDbContext
                .DataSetVersions
                .SingleAsync(
                    dataSetVersion => dataSetVersion.Id == processorResponse.DataSetVersionId,
                    cancellationToken))
            .OnSuccess(async dataSetVersion => await MapDraftVersionSummary(dataSetVersion, cancellationToken));
    }

    public async Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CompleteNextVersionImport(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async _ => await processorClient.CompleteNextDataSetVersionImport(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken))
            .OnSuccess(async processorResponse => await publicDataDbContext
                .DataSetVersions
                .SingleAsync(
                    dataSetVersion => dataSetVersion.Id == processorResponse.DataSetVersionId,
                    cancellationToken))
            .OnSuccess(async dataSetVersion => await MapDraftVersionSummary(dataSetVersion, cancellationToken));
    }

    public async Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(
        Guid dataSetId,
        SemVersion version,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccessVoid(async () => await processorClient.DeleteDataSetVersion(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken));
    }

    public async Task<Either<ActionResult, DataSetVersionChangesViewModel>> GetVersionChanges(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(() => publicDataDbContext.DataSetVersions
                .AsNoTracking()
                .Include(dsv => dsv.DataSet)
                .Where(dsv => dsv.Id == dataSetVersionId)
                .SingleOrNotFoundAsync(cancellationToken: cancellationToken))
            .OnSuccessCombineWith(dataSetVersion => publicDataApiClient.GetDataSetVersionChanges(
                dataSetId: dataSetVersion.DataSetId,
                dataSetVersion: dataSetVersion.PublicVersion,
                cancellationToken: cancellationToken
            ))
            .OnSuccess(tuple =>
            {
                var (dataSetVersion, dataSetVersionChanges) = tuple;

                return MapVersionChanges(dataSetVersion, dataSetVersionChanges);
            });
    }

    public async Task<Either<ActionResult, DataSetDraftVersionViewModel>> UpdateVersion(
        Guid dataSetVersionId,
        DataSetVersionUpdateRequest updateRequest,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async () => await GetDataSetVersion(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken))
            .OnSuccessDo(dataSetVersion => CheckCanUpdateVersion(dataSetVersion, updateRequest))
            .OnSuccess(async dataSetVersion => await UpdateVersion(
                dataSetVersion: dataSetVersion,
                updateRequest: updateRequest,
                cancellationToken: cancellationToken))
            .OnSuccess(async dataSetVersion => await MapDraftVersion(dataSetVersion, cancellationToken));
    }

    public async Task UpdateVersionsForReleaseVersion(
        Guid releaseVersionId,
        string releaseSlug,
        string releaseTitle,
        CancellationToken cancellationToken = default)
    {
        var releaseFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.Id)
            .ToListAsync(cancellationToken);

        var dataSetVersions = await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.Release.ReleaseFileId))
            .ToListAsync(cancellationToken);

        if (dataSetVersions.Count > 0)
        {
            foreach (var dsv in dataSetVersions)
            {
                dsv.Release.Slug = releaseSlug;
                dsv.Release.Title = releaseTitle;
            }

            await publicDataDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<IReadOnlyDictionary<Guid, ReleaseFile>> GetReleaseFilesByDataSetVersionId(
        IReadOnlyList<DataSetVersion> dataSetVersions,
        CancellationToken cancellationToken)
    {
        var dataSetVersionsByReleaseFileId = dataSetVersions
            .ToDictionary(dsv => dsv.Release.ReleaseFileId);

        return await contentDbContext
            .ReleaseFiles
            .Include(rf => rf.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .Include(rf => rf.File)
            .Where(releaseFile => dataSetVersionsByReleaseFileId.Keys.Contains(releaseFile.Id))
            .ToDictionaryAsync(
                rf => dataSetVersionsByReleaseFileId[rf.Id].Id,
                rf => rf,
                cancellationToken);
    }

    private static DataSetLiveVersionSummaryViewModel MapLiveVersionSummary(
        DataSetVersion dataSetVersion,
        ReleaseFile releaseFile)
    {
        return new DataSetLiveVersionSummaryViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.PublicVersion,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
            File = MapVersionFile(releaseFile),
            Published = dataSetVersion.Published!.Value
        };
    }

    private async Task<DataSetVersionSummaryViewModel> MapDraftVersionSummary(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        var releaseFile = await GetReleaseFile(dataSetVersion, cancellationToken);

        return new DataSetVersionSummaryViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.PublicVersion,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
            File = MapVersionFile(releaseFile)
        };
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Where(dsv => dsv.Id == dataSetVersionId)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private static Either<ActionResult, Unit> CheckCanUpdateVersion(
        DataSetVersion dataSetVersion,
        DataSetVersionUpdateRequest updateRequest)
    {
        if (!dataSetVersion.CanBeUpdated)
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.DataSetVersionCannotBeUpdated.Code,
                Message = ValidationMessages.DataSetVersionCannotBeUpdated.Message,
                Detail = new InvalidErrorDetail<Guid>(dataSetVersion.Id),
                Path = "dataSetVersionId"
            });
        }

        if (updateRequest.Notes is not null && dataSetVersion.IsFirstVersion)
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.DataSetVersionCannotHaveNotes.Code,
                Message = ValidationMessages.DataSetVersionCannotHaveNotes.Message,
                Path = nameof(DataSetVersionUpdateRequest.Notes).ToLowerFirst()
            });
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, DataSetVersion>> UpdateVersion(
        DataSetVersion dataSetVersion,
        DataSetVersionUpdateRequest updateRequest,
        CancellationToken cancellationToken)
    {
        if (updateRequest.Notes is not null)
        {
            dataSetVersion.Notes = updateRequest.Notes;
        }

        publicDataDbContext.DataSetVersions.Update(dataSetVersion);

        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return dataSetVersion;
    }

    private async Task<DataSetDraftVersionViewModel> MapDraftVersion(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        var releaseFile = await GetReleaseFile(dataSetVersion, cancellationToken);

        return new DataSetDraftVersionViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.PublicVersion,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            File = MapVersionFile(releaseFile),
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
            TotalResults = dataSetVersion.TotalResults,
            Notes = dataSetVersion.Notes,
            GeographicLevels = dataSetVersion.MetaSummary?.GeographicLevels
                .Select(l => l.GetEnumLabel())
                .ToList() ?? null,
            TimePeriods = dataSetVersion.MetaSummary?.TimePeriodRange is not null
                ? TimePeriodRangeViewModel.Create(dataSetVersion.MetaSummary.TimePeriodRange)
                : null,
            Filters = dataSetVersion.MetaSummary?.Filters ?? null,
            Indicators = dataSetVersion.MetaSummary?.Indicators ?? null
        };
    }

    private async Task<ReleaseFile> GetReleaseFile(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Where(rf => rf.Id == dataSetVersion.Release.ReleaseFileId)
            .Include(rf => rf.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .Include(rf => rf.File)
            .SingleAsync(cancellationToken);
    }

    private static IdTitleViewModel MapVersionFile(ReleaseFile releaseFile)
    {
        return new IdTitleViewModel
        {
            Id = releaseFile.File.DataSetFileId!.Value,
            Title = releaseFile.Name ?? string.Empty,
        };
    }

    private static IdTitleViewModel MapReleaseVersion(ReleaseVersion releaseVersion)
    {
        return new IdTitleViewModel
        {
            Id = releaseVersion.Id,
            Title = releaseVersion.Release.Title,
        };
    }

    private DataSetVersionChangesViewModel MapVersionChanges(DataSetVersion dataSetVersion, DataSetVersionChangesViewModelDto dataSetVersionChanges)
    {
        return new DataSetVersionChangesViewModel
        {
            DataSet = new IdTitleViewModel(dataSetVersion.DataSetId, dataSetVersion.DataSet.Title),
            DataSetVersion = MapDataSetVersion(dataSetVersion),
            Changes = mapper.Map<DataSetVersionChangesViewModel2>(dataSetVersionChanges),
        };
    }

    private static DataSetVersionViewModel2 MapDataSetVersion(DataSetVersion dataSetVersion)
    {
        return new DataSetVersionViewModel2
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.PublicVersion,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            Notes = dataSetVersion.Notes,
        };
    }
}

public record DataSetVersionStatusSummary(Guid Id, string Title, DataSetVersionStatus Status);
