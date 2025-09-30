#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseImageService : IReleaseImageService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IPrivateBlobStorageService _privateBlobStorageService;
    private readonly IFileValidatorService _fileValidatorService;
    private readonly IReleaseFileRepository _releaseFileRepository;
    private readonly IUserService _userService;

    public ReleaseImageService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IPrivateBlobStorageService privateBlobStorageService,
        IFileValidatorService fileValidatorService,
        IReleaseFileRepository releaseFileRepository,
        IUserService userService
    )
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _privateBlobStorageService = privateBlobStorageService;
        _fileValidatorService = fileValidatorService;
        _releaseFileRepository = releaseFileRepository;
        _userService = userService;
    }

    public async Task<Either<ActionResult, FileStreamResult>> Stream(
        Guid releaseVersionId,
        Guid fileId
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseFile>(q =>
                q.Include(rf => rf.File)
                    .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.FileId == fileId)
            )
            .OnSuccessCombineWith(rf =>
                _privateBlobStorageService.GetDownloadStream(PrivateReleaseFiles, rf.Path())
            )
            .OnSuccess(releaseFileAndStream =>
            {
                var (releaseFile, stream) = releaseFileAndStream;
                return new FileStreamResult(stream, releaseFile.File.ContentType)
                {
                    FileDownloadName = releaseFile.File.Filename,
                };
            });
    }

    public Task<Either<ActionResult, ImageFileViewModel>> Upload(
        Guid releaseVersionId,
        IFormFile formFile
    )
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async () =>
                await _fileValidatorService.ValidateFileForUpload(formFile, Image)
            )
            .OnSuccess(async () => await Upload(releaseVersionId, Image, formFile))
            .OnSuccess(releaseFile => new ImageFileViewModel(
                $"/api/releases/{releaseVersionId}/images/{releaseFile.File.Id}"
            )
            {
                // TODO EES-1922 Add support for resizing the image
            });
    }

    private async Task<Either<ActionResult, ReleaseFile>> Upload(
        Guid releaseVersionId,
        FileType type,
        IFormFile formFile
    )
    {
        var releaseFile = await _releaseFileRepository.Create(
            releaseVersionId: releaseVersionId,
            filename: formFile.FileName,
            contentLength: formFile.Length,
            contentType: formFile.ContentType,
            type: type,
            createdById: _userService.GetUserId()
        );

        await _contentDbContext.SaveChangesAsync();

        await _privateBlobStorageService.UploadFile(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            file: formFile
        );

        return releaseFile;
    }
}
