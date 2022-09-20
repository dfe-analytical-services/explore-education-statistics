#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;
using IReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseFileService;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
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
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileRepository _fileRepository;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IDataGuidanceFileWriter _dataGuidanceFileWriter;
        private readonly IUserService _userService;

        public ReleaseFileService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IFileRepository fileRepository,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IReleaseFileRepository releaseFileRepository,
            IDataGuidanceFileWriter dataGuidanceFileWriter,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _fileRepository = fileRepository;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _releaseFileRepository = releaseFileRepository;
            _dataGuidanceFileWriter = dataGuidanceFileWriter;
            _userService = userService;
        }

        public async Task<Either<ActionResult, File>> CheckFileExists(Guid releaseId,
            Guid fileId,
            params FileType[] allowedFileTypes)
        {
            // Ensure file is linked to the Release by getting the ReleaseFile first
            var releaseFile = await _releaseFileRepository.Find(releaseId, fileId);

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

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            Guid fileId,
            bool forceDelete = false)
        {
            return await Delete(releaseId, new List<Guid>
            {
                fileId
            }, forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await _userService.CheckCanUpdateRelease(release, ignoreCheck: forceDelete))
                .OnSuccess(async _ =>
                    await fileIds.Select(fileId => CheckFileExists(releaseId, fileId, DeletableFileTypes)).OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    foreach (var file in files)
                    {
                        await _releaseFileRepository.Delete(releaseId, file.Id);
                        if (!await _releaseFileRepository.FileIsLinkedToOtherReleases(releaseId, file.Id))
                        {
                            await _blobStorageService.DeleteBlob(
                                PrivateReleaseFiles,
                                file.Path());

                            await _fileRepository.Delete(file.Id);
                        }
                    }
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseId, bool forceDelete = false)
        {
            var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId, DeletableFileTypes);

            return await Delete(releaseId,
                releaseFiles.Select(releaseFile => releaseFile.File.Id),
                forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListAll(Guid releaseId,
            params FileType[] types)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId, types);

                    return releaseFiles
                        .Select(releaseFile => releaseFile.ToFileInfo())
                        .OrderBy(file => file.Name.IsNullOrWhitespace())
                        .ThenBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, FileInfo>> GetFile(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(() => _releaseFileRepository.FindOrNotFound(releaseId, fileId))
                .OnSuccess(releaseFile => releaseFile.ToFileInfo());
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_ => CheckFileExists(releaseId, fileId))
                .OnSuccessCombineWith(file =>
                    _blobStorageService.DownloadToStream(PrivateReleaseFiles, file.Path(), new MemoryStream()))
                .OnSuccess(fileAndStream =>
                {
                    var (file, stream) = fileAndStream;
                    return new FileStreamResult(stream, file.ContentType)
                    {
                        FileDownloadName = file.Filename
                    };
                });
        }

        public async Task<Either<ActionResult, Unit>> ZipFilesToStream(
            Guid releaseId,
            Stream outputStream,
            IEnumerable<Guid>? fileIds = null,
            CancellationToken? cancellationToken = null)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(
                    releaseId,
                    q => q.Include(r => r.Publication)
                )
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccessVoid(
                    async release =>
                    {
                        var query = QueryZipReleaseFiles(releaseId);

                        if (fileIds != null)
                        {
                            query = query.Where(rf => fileIds.Contains(rf.FileId));
                        }

                        var releaseFiles = (await query.ToListAsync())
                            .OrderBy(rf => rf.File.ZipFileEntryName())
                            .ToList();

                        await DoZipFilesToStream(releaseFiles, release, outputStream, cancellationToken);
                    }
                );
        }

        private async Task DoZipFilesToStream(
            List<ReleaseFile> releaseFiles,
            Release release,
            Stream outputStream,
            CancellationToken? cancellationToken)
        {
            using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create);

            foreach (var releaseFile in releaseFiles)
            {
                // Stop immediately if we receive a cancellation request
                if (cancellationToken?.IsCancellationRequested == true)
                {
                    return;
                }

                var blobExists = await _blobStorageService.CheckBlobExists(
                    PrivateReleaseFiles,
                    releaseFile.Path()
                );

                if (!blobExists)
                {
                    continue;
                }

                var entry = archive
                    .CreateEntry(releaseFile.File.ZipFileEntryName())
                    .SetUnixPermissions("664");

                await using var entryStream = entry.Open();

                await _blobStorageService.DownloadToStream(
                    containerName: PrivateReleaseFiles,
                    path: releaseFile.Path(),
                    stream: entryStream,
                    cancellationToken: cancellationToken
                );
            }

            // Add data guidance file if there are any data files in this zip.
            var subjectIds = releaseFiles
                .Where(rf => rf.File.SubjectId.HasValue)
                .Select(rf => rf.File.SubjectId.GetValueOrDefault())
                .ToList();

            if (subjectIds.Any())
            {
                var entry = archive
                    .CreateEntry(FileType.DataGuidance.GetEnumLabel() + "/data-guidance.txt")
                    .SetUnixPermissions("664");

                await using var entryStream = entry.Open();

                await _dataGuidanceFileWriter.WriteToStream(entryStream, release, subjectIds);
            }
        }

        public Task<Either<ActionResult, Unit>> Update(Guid releaseId, Guid fileId, ReleaseFileUpdateViewModel update)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => _releaseFileRepository.FindOrNotFound(releaseId, fileId))
                .OnSuccessVoid(
                    async () =>
                    {
                        await _releaseFileRepository.Update(
                            releaseId: releaseId,
                            fileId: fileId,
                            name: update.Title,
                            summary: update.Summary
                        );
                    }
                );
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> GetAncillaryFiles(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId, Ancillary);

                    var filesWithMetadata = await releaseFiles
                        .SelectAsync(async releaseFile => await ToAncillaryFileInfo(releaseFile));

                    return filesWithMetadata
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadAncillary(
            Guid releaseId,
            ReleaseAncillaryFileUploadViewModel upload)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(upload.File, Ancillary))
                .OnSuccess(async () =>
                {
                    var releaseFile = await _releaseFileRepository.Create(
                        releaseId: releaseId,
                        filename: upload.File.FileName,
                        contentLength: upload.File.Length,
                        contentType: upload.File.ContentType,
                        type: Ancillary,
                        createdById: _userService.GetUserId(),
                        name: upload.Title,
                        summary: upload.Summary);

                    await _contentDbContext.SaveChangesAsync();

                    await _blobStorageService.UploadFile(
                        containerName: PrivateReleaseFiles,
                        path: releaseFile.Path(),
                        file: upload.File);

                    return await ToAncillaryFileInfo(releaseFile);
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseId,
            IFormFile formFile,
            Guid? replacingId = null)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(formFile, Chart))
                .OnSuccess(async () =>
                {
                    var releaseFile = replacingId.HasValue
                        ? await _releaseFileRepository.Update(
                            releaseId: releaseId,
                            fileId: replacingId.Value,
                            fileName: formFile.FileName)
                        : await _releaseFileRepository.Create(
                            releaseId: releaseId,
                            filename: formFile.FileName,
                            contentLength: formFile.Length,
                            contentType: formFile.ContentType,
                            type: Chart,
                            createdById: _userService.GetUserId());

                    await _blobStorageService.UploadFile(
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

        private IQueryable<ReleaseFile> QueryZipReleaseFiles(Guid releaseId)
        {
            return _contentDbContext.ReleaseFiles
                .Include(f => f.Release)
                .ThenInclude(r => r.Publication)
                .Include(f => f.File)
                .Where(releaseFile => releaseFile.ReleaseId == releaseId
                                      && AllowedZipFileTypes.Contains(releaseFile.File.Type));
        }
    }
}
