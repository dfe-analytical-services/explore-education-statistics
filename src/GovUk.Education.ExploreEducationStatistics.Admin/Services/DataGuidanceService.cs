#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataGuidanceService : IDataGuidanceService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IDataGuidanceDataSetService _dataGuidanceDataSetService;
    private readonly IUserService _userService;
    private readonly IReleaseDataFileRepository _releaseDataFileRepository;

    public DataGuidanceService(
        ContentDbContext contentDbContext,
        IDataGuidanceDataSetService dataGuidanceDataSetService,
        IUserService userService,
        IReleaseDataFileRepository releaseDataFileRepository
    )
    {
        _contentDbContext = contentDbContext;
        _dataGuidanceDataSetService = dataGuidanceDataSetService;
        _userService = userService;
        _releaseDataFileRepository = releaseDataFileRepository;
    }

    public async Task<Either<ActionResult, DataGuidanceViewModel>> GetDataGuidance(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await _contentDbContext
            .ReleaseVersions.FirstOrNotFoundAsync(
                releaseVersion => releaseVersion.Id == releaseVersionId,
                cancellationToken
            )
            .OnSuccess(releaseVersion => _userService.CheckCanViewReleaseVersion(releaseVersion))
            .OnSuccessCombineWith(_ =>
                _dataGuidanceDataSetService.ListDataSets(releaseVersionId, cancellationToken: cancellationToken)
            )
            .OnSuccess(releaseVersionAndDataSets =>
                BuildViewModel(releaseVersionAndDataSets.Item1, releaseVersionAndDataSets.Item2)
            );
    }

    public async Task<Either<ActionResult, DataGuidanceViewModel>> UpdateDataGuidance(
        Guid releaseVersionId,
        DataGuidanceUpdateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await _contentDbContext
            .ReleaseVersions.FirstOrNotFoundAsync(
                releaseVersion => releaseVersion.Id == releaseVersionId,
                cancellationToken
            )
            .OnSuccess(releaseVersion => _userService.CheckCanUpdateReleaseVersion(releaseVersion))
            .OnSuccessDo(releaseVersion => UpdateDataGuidance(releaseVersion, request, cancellationToken))
            .OnSuccessCombineWith(_ =>
                _dataGuidanceDataSetService.ListDataSets(releaseVersionId, cancellationToken: cancellationToken)
            )
            .OnSuccess(releaseVersionAndDataSets =>
                BuildViewModel(releaseVersionAndDataSets.Item1, releaseVersionAndDataSets.Item2)
            );
    }

    public async Task<Either<ActionResult, Unit>> ValidateForReleaseChecklist(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await _contentDbContext
            .ReleaseVersions.FirstOrNotFoundAsync(
                releaseVersion => releaseVersion.Id == releaseVersionId,
                cancellationToken
            )
            .OnSuccess<ActionResult, ReleaseVersion, Unit>(async releaseVersion =>
            {
                var releaseFilesQueryable = _contentDbContext.ReleaseFiles.Where(rf =>
                    rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data
                );

                if (await releaseFilesQueryable.AnyAsync(cancellationToken))
                {
                    if (string.IsNullOrWhiteSpace(releaseVersion.DataGuidance))
                    {
                        return ValidationResult(PublicDataGuidanceRequired);
                    }

                    if (
                        await releaseFilesQueryable.AnyAsync(
                            rf => string.IsNullOrWhiteSpace(rf.Summary),
                            cancellationToken
                        )
                    )
                    {
                        return ValidationResult(PublicDataGuidanceRequired);
                    }
                }

                return Unit.Instance;
            });
    }

    private async Task<Either<ActionResult, Unit>> UpdateDataGuidance(
        ReleaseVersion releaseVersion,
        DataGuidanceUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        _contentDbContext.Update(releaseVersion);
        releaseVersion.DataGuidance = request.Content;

        var allDataFileIds = (await _releaseDataFileRepository.ListDataFiles(releaseVersion.Id)).Select(file =>
            file.Id
        );

        var updateRequestsByFileId = request.DataSets.ToDictionary(dataSet => dataSet.FileId);
        var dataFileIds = updateRequestsByFileId.Keys.ToList();

        if (!allDataFileIds.ContainsAll(dataFileIds))
        {
            return ValidationResult(DataGuidanceDataSetNotAttachedToRelease);
        }

        var releaseFiles = await _contentDbContext
            .ReleaseFiles.Include(releaseFile => releaseFile.File)
            .Where(releaseFile =>
                releaseFile.ReleaseVersionId == releaseVersion.Id && dataFileIds.Contains(releaseFile.FileId)
            )
            .ToListAsync(cancellationToken);

        releaseFiles.ForEach(releaseFile =>
        {
            var content = updateRequestsByFileId[releaseFile.FileId].Content;

            _contentDbContext.Update(releaseFile);

            if (releaseFile.Summary != content)
            {
                releaseFile.Published = null; // This will be repopulated with the current date during the publish process
            }

            releaseFile.Summary = content;
        });

        await _contentDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Instance;
    }

    private static DataGuidanceViewModel BuildViewModel(
        ReleaseVersion releaseVersion,
        List<DataGuidanceDataSetViewModel> dataSets
    )
    {
        return new DataGuidanceViewModel
        {
            Id = releaseVersion.Id,
            Content = releaseVersion.DataGuidance ?? "",
            DataSets = dataSets,
        };
    }
}
