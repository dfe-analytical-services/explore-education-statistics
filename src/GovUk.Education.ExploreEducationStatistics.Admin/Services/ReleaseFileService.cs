#nullable enable
using System.IO.Compression;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;
using IReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseFileService;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseFileService : IReleaseFileService
{
    private static readonly FileType[] AllowedZipFileTypes =
    {
        Ancillary,
        FileType.Data
    };

    private static readonly FileType[] DeletableFileTypes =
    {
        Ancillary,
        Chart,
        Image,
        DataGuidance
    };

    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IPrivateBlobStorageService _privateBlobStorageService;
    private readonly IFileRepository _fileRepository;
    private readonly IFileValidatorService _fileValidatorService;
    private readonly IReleaseFileRepository _releaseFileRepository;
    private readonly IDataGuidanceFileWriter _dataGuidanceFileWriter;
    private readonly IUserService _userService;

    public ReleaseFileService(ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IPrivateBlobStorageService privateBlobStorageService,
        IFileRepository fileRepository,
        IFileValidatorService fileValidatorService,
        IReleaseFileRepository releaseFileRepository,
        IDataGuidanceFileWriter dataGuidanceFileWriter,
        IUserService userService)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _privateBlobStorageService = privateBlobStorageService;
        _fileRepository = fileRepository;
        _fileValidatorService = fileValidatorService;
        _releaseFileRepository = releaseFileRepository;
        _dataGuidanceFileWriter = dataGuidanceFileWriter;
        _userService = userService;
    }

    public async Task<Either<ActionResult, File>> CheckFileExists(Guid releaseVersionId,
        Guid fileId,
        params FileType[] allowedFileTypes)
    {
        // Ensure file is linked to the release version by getting the ReleaseFile first
        var releaseFile = await _releaseFileRepository.Find(releaseVersionId, fileId);

        if (releaseFile == null)
        {
            return new NotFoundResult();
        }

        if (allowedFileTypes.Any() && !allowedFileTypes.Contains(releaseFile.File.Type))
        {
            return ValidationUtils.ValidationResult(FileTypeInvalid);
        }

        return releaseFile.File;
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
        Guid fileId,
        bool forceDelete = false)
    {
        return await Delete(releaseVersionId, new List<Guid>
        {
            fileId
        }, forceDelete: forceDelete);
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
        IEnumerable<Guid> fileIds,
        bool forceDelete = false)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(async releaseVersion =>
                await _userService.CheckCanUpdateReleaseVersion(releaseVersion, ignoreCheck: forceDelete))
            .OnSuccess(async _ =>
                await fileIds.Select(fileId => CheckFileExists(releaseVersionId: releaseVersionId,
                    fileId: fileId,
                    DeletableFileTypes)).OnSuccessAll())
            .OnSuccessVoid(async files =>
            {
                foreach (var file in files)
                {
                    await _releaseFileRepository.Delete(releaseVersionId: releaseVersionId,
                        fileId: file.Id);
                    if (!await _releaseFileRepository.FileIsLinkedToOtherReleases(
                            releaseVersionId: releaseVersionId,
                            fileId: file.Id))
                    {
                        await _privateBlobStorageService.DeleteBlob(
                            PrivateReleaseFiles,
                            file.Path());

                        await _fileRepository.Delete(file.Id);
                    }
                }
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseVersionId, bool forceDelete = false)
    {
        var releaseFiles = await _releaseFileRepository.GetByFileType(releaseVersionId, types: DeletableFileTypes);

        return await Delete(releaseVersionId,
            releaseFiles.Select(releaseFile => releaseFile.File.Id),
            forceDelete: forceDelete);
    }

    public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListAll(Guid releaseVersionId,
        params FileType[] types)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
            {
                var releaseFiles = await _releaseFileRepository.GetByFileType(releaseVersionId, types: types);

                return releaseFiles
                    .Select(releaseFile => releaseFile.ToFileInfo())
                    .OrderBy(file => file.Name.IsNullOrWhitespace())
                    .ThenBy(file => file.Name)
                    .AsEnumerable();
            });
    }

    public async Task<Either<ActionResult, FileInfo>> GetFile(Guid releaseVersionId, Guid fileId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(() => _releaseFileRepository.FindOrNotFound(releaseVersionId: releaseVersionId,
                fileId: fileId))
            .OnSuccess(releaseFile => releaseFile.ToFileInfo());
    }

    public async Task<Either<ActionResult, BlobDownloadToken>> GetBlobDownloadToken(
        Guid releaseVersionId,
        Guid fileId,
        CancellationToken cancellationToken)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(_ => CheckFileExists(releaseVersionId: releaseVersionId, fileId: fileId))
            .OnSuccess(file => _privateBlobStorageService
                .GetBlobDownloadToken(
                    container: PrivateReleaseFiles,
                    filename: file.Filename,
                    path: file.Path(),
                    cancellationToken: cancellationToken));
    }

    public async Task<Either<ActionResult, Unit>> ZipFilesToStream(
        Guid releaseVersionId,
        Stream outputStream,
        IEnumerable<Guid>? fileIds = null,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccessVoid(
                async releaseVersion =>
                {
                    var query = QueryZipReleaseFiles(releaseVersionId);

                    if (fileIds != null)
                    {
                        query = query.Where(rf => fileIds.Contains(rf.FileId));
                    }

                    var releaseFiles = (await query.ToListAsync(cancellationToken: cancellationToken))
                        .OrderBy(rf => rf.File.ZipFileEntryName())
                        .ToList();

                    await DoZipFilesToStream(releaseFiles, releaseVersion, outputStream, cancellationToken);
                }
            );
    }

    private async Task DoZipFilesToStream(
        List<ReleaseFile> releaseFiles,
        ReleaseVersion releaseVersion,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create);

        var releaseFilesWithZipEntries = new List<ReleaseFile>();
        foreach (var releaseFile in releaseFiles)
        {
            var streamResult = await _privateBlobStorageService.GetDownloadStream(
                containerName: PrivateReleaseFiles,
                path: releaseFile.Path(),
                cancellationToken: cancellationToken);
            
            // Stop immediately if we receive a cancellation request
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            // Ignore files where we cannot successfully get their blob download streams.
            if (streamResult.IsLeft)
            {
                continue;
            }
            
            await using var blobStream = streamResult.Right;

            var entry = archive.CreateEntry(releaseFile.File.ZipFileEntryName());
            await using var entryStream = entry.Open();
            await blobStream.CopyToAsync(entryStream, cancellationToken);

            releaseFilesWithZipEntries.Add(releaseFile);
        }

        // Add data guidance file if there are any data files in this zip.
        if (releaseFilesWithZipEntries.Any(rf => rf.File.Type == FileType.Data))
        {
            var entry = archive.CreateEntry($"{DataGuidance.GetEnumLabel()}/data-guidance.txt");

            await using var entryStream = entry.Open();

            var dataFileIds = releaseFilesWithZipEntries
                .Where(rf => rf.File.Type == FileType.Data)
                .Select(rf => rf.FileId)
                .ToList();

            await _dataGuidanceFileWriter.WriteToStream(entryStream, releaseVersion, dataFileIds);
        }
    }

    public Task<Either<ActionResult, Unit>> UpdateDataFileDetails(Guid releaseVersionId,
        Guid fileId,
        ReleaseDataFileUpdateRequest update)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ =>
                _persistenceHelper.CheckEntityExists<ReleaseFile>(q => q
                    .Where(rf => rf.ReleaseVersionId == releaseVersionId
                                 && rf.FileId == fileId
                                 && rf.File.Type == FileType.Data)))
            .OnSuccessVoid(
                async releaseFile =>
                {
                    releaseFile.Name = update.Title ?? releaseFile.Name;
                    releaseFile.Summary = update.Summary ?? releaseFile.Summary;

                    var releaseFileDataHasChanged =
                        !string.IsNullOrWhiteSpace(update.Title) ||
                        !string.IsNullOrWhiteSpace(update.Summary);

                    if (releaseFileDataHasChanged)
                    {
                        releaseFile.Published = null; // This will be repopulated with the current date during the publish process
                    }

                    await _contentDbContext.SaveChangesAsync();
                }
            );
    }

    public async Task<Either<ActionResult, IEnumerable<FileInfo>>> GetAncillaryFiles(Guid releaseVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
            {
                var releaseFiles = await _releaseFileRepository.GetByFileType(releaseVersionId, types: Ancillary);

                var filesWithMetadata = await releaseFiles
                    .SelectAsync(async releaseFile => await ToAncillaryFileInfo(releaseFile));

                return filesWithMetadata
                    .OrderBy(file => file.Name)
                    .AsEnumerable();
            });
    }

    public Task<Either<ActionResult, FileInfo>> UploadAncillary(
        Guid releaseVersionId,
        ReleaseAncillaryFileUploadRequest upload)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async () => await _fileValidatorService.ValidateFileForUpload(upload.File, Ancillary))
            .OnSuccess(async () =>
            {
                var newFileId = Guid.NewGuid();

                await _privateBlobStorageService.UploadFile(
                    containerName: PrivateReleaseFiles,
                    path: FileExtensions.Path(
                        rootPath: releaseVersionId,
                        type: Ancillary,
                        fileId: newFileId),
                    file: upload.File);

                var releaseFile = await _releaseFileRepository.Create(
                    newFileId: newFileId,
                    releaseVersionId: releaseVersionId,
                    filename: upload.File.FileName,
                    contentLength: upload.File.Length,
                    contentType: upload.File.ContentType,
                    type: Ancillary,
                    createdById: _userService.GetUserId(),
                    name: upload.Title,
                    summary: upload.Summary);

                await _contentDbContext.SaveChangesAsync();

                return await ToAncillaryFileInfo(releaseFile);
            });
    }

    public Task<Either<ActionResult, FileInfo>> UpdateAncillary(
        Guid releaseVersionId,
        Guid fileId,
        ReleaseAncillaryFileUpdateRequest request)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ => _persistenceHelper.CheckEntityExists<ReleaseFile>(query =>
                query
                    .Include(rf => rf.File)
                    .Where(rf =>
                        rf.FileId == fileId
                        && rf.ReleaseVersionId == releaseVersionId
                        && rf.File.Type == Ancillary))
            )
            .OnSuccessDo(async _ =>
            {
                if (request.File != null)
                {
                    return await _fileValidatorService.ValidateFileForUpload(request.File, Ancillary);
                }

                return Unit.Instance;
            })
            .OnSuccess(async releaseFile =>
            {
                if (request.File != null)
                {
                    var oldFile = releaseFile.File;
                    var newFileId = Guid.NewGuid();

                    await _privateBlobStorageService.UploadFile(
                        containerName: PrivateReleaseFiles,
                        path: FileExtensions.Path(
                            rootPath: releaseVersionId,
                            type: Ancillary,
                            fileId: newFileId),
                        file: request.File);

                    var newFile = await _fileRepository.Create(
                        newFileId: newFileId,
                        releaseVersionId: releaseVersionId,
                        filename: request.File.FileName,
                        contentLength: request.File.Length,
                        contentType: request.File.ContentType,
                        type: Ancillary,
                        createdById: _userService.GetUserId());

                    releaseFile.FileId = newFile.Id;
                    releaseFile.Name = request.Title;
                    releaseFile.Summary = request.Summary;
                    _contentDbContext.Update(releaseFile);
                    await _contentDbContext.SaveChangesAsync();

                    if (!await _releaseFileRepository.FileIsLinkedToOtherReleases(
                            releaseVersionId: releaseVersionId,
                            fileId: oldFile.Id))
                    {
                        await _privateBlobStorageService.DeleteBlob(
                            PrivateReleaseFiles,
                            oldFile.Path());

                        await _fileRepository.Delete(oldFile.Id);
                    }

                    return await ToAncillaryFileInfo(releaseFile);
                }

                _contentDbContext.Update(releaseFile);
                releaseFile.Name = request.Title;
                releaseFile.Summary = request.Summary;
                await _contentDbContext.SaveChangesAsync();

                return await ToAncillaryFileInfo(releaseFile);
            });
    }

    public Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseVersionId,
        IFormFile formFile,
        Guid? replacingId = null)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async () => await _fileValidatorService.ValidateFileForUpload(formFile, Chart))
            .OnSuccess(async () =>
            {
                var releaseFile = replacingId.HasValue
                    ? await _releaseFileRepository.Update(
                        releaseVersionId: releaseVersionId,
                        fileId: replacingId.Value,
                        fileName: formFile.FileName)
                    : await _releaseFileRepository.Create(
                        releaseVersionId: releaseVersionId,
                        filename: formFile.FileName,
                        contentLength: formFile.Length,
                        contentType: formFile.ContentType,
                        type: Chart,
                        createdById: _userService.GetUserId());

                await _privateBlobStorageService.UploadFile(
                    containerName: PrivateReleaseFiles,
                    path: releaseFile.Path(),
                    file: formFile
                );

                return releaseFile.ToFileInfo();
            });
    }

    private async Task HydrateReleaseFile(ReleaseFile releaseFile)
    {
        await _contentDbContext.Entry(releaseFile)
            .Reference(rf => rf.File)
            .LoadAsync();
        await _contentDbContext.Entry(releaseFile.File)
            .Reference(f => f.CreatedBy)
            .LoadAsync();
    }

    private async Task<FileInfo> ToAncillaryFileInfo(ReleaseFile releaseFile)
    {
        await HydrateReleaseFile(releaseFile);
        return releaseFile.ToFileInfo();
    }

    private IQueryable<ReleaseFile> QueryZipReleaseFiles(Guid releaseVersionId)
    {
        return _contentDbContext.ReleaseFiles
            .Include(f => f.ReleaseVersion)
            .ThenInclude(rv => rv.Publication)
            .Include(f => f.File)
            .Where(releaseFile => releaseFile.ReleaseVersionId == releaseVersionId
                                  && AllowedZipFileTypes.Contains(releaseFile.File.Type));
    }
}
