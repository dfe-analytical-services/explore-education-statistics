﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseFileService : IReleaseFileService
    {
        private static readonly FileType[] DeletableFileTypes = { Ancillary, Chart, Image, DataGuidance };

        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileRepository _fileRepository;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IUserService _userService;

        public ReleaseFileService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IFileRepository fileRepository,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IReleaseFileRepository releaseFileRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _fileRepository = fileRepository;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _releaseFileRepository = releaseFileRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            Guid id,
            bool forceDelete = false)
        {
            return await Delete(releaseId, new List<Guid>
            {
                id
            }, forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            IEnumerable<Guid> ids,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await _userService.CheckCanUpdateRelease(release, ignoreCheck: forceDelete))
                .OnSuccess(async release =>
                    await ids.Select(id =>
                        _releaseFileRepository.CheckFileExists(releaseId, id, DeletableFileTypes)).OnSuccessAll())
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

                    var filesWithMetadata = await releaseFiles
                        .SelectAsync(async releaseFile =>
                        {
                            var file = releaseFile.File;

                            // Files should exists in storage but if not then allow user to delete
                            var exists = await _blobStorageService.CheckBlobExists(
                                PrivateReleaseFiles,
                                file.Path());

                            if (!exists)
                            {
                                return file.ToFileInfoNotFound();
                            }

                            var blob = await _blobStorageService.GetBlob(PrivateReleaseFiles, file.Path());
                            return releaseFile.ToFileInfo(blob);
                        });

                    return filesWithMetadata
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, FileInfo>> GetFile(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(
                    q => q.Include(rf => rf.File)
                        .Where(rf =>
                            rf.ReleaseId == releaseId
                            && rf.FileId == fileId))
                .OnSuccess(async releaseFile =>
                {
                    var blobExists = await _blobStorageService.CheckBlobExists(
                        PrivateReleaseFiles,
                        releaseFile.Path());
                    if (!blobExists)
                    {
                        return releaseFile.File.ToFileInfoNotFound();
                    }
                    var blob = await _blobStorageService.GetBlob(PrivateReleaseFiles, releaseFile.Path());
                    return releaseFile.ToFileInfo(blob);
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _releaseFileRepository.CheckFileExists(releaseId, id))
                .OnSuccess(async file =>
                {
                    var path = file.Path();
                    var blob = await _blobStorageService.GetBlob(PrivateReleaseFiles, path);

                    var stream = new MemoryStream();
                    await _blobStorageService.DownloadToStream(PrivateReleaseFiles, path, stream);

                    return new FileStreamResult(stream, blob.ContentType)
                    {
                        FileDownloadName = file.Filename
                    };
                });
        }

        public Task<Either<ActionResult, Unit>> UpdateName(Guid releaseId, Guid fileId, string name)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessVoid(async () =>
                {
                    await _releaseFileRepository.UpdateName(releaseId, fileId, name);
                });
        }

        public async Task<Either<ActionResult, IEnumerable<AncillaryFileInfo>>> GetAncillaryFiles(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId, Ancillary);

                    var filesWithMetadata = await releaseFiles
                        .SelectAsync(async releaseFile =>
                        {
                            var exists = await _blobStorageService.CheckBlobExists(
                                PrivateReleaseFiles,
                                releaseFile.Path());

                            if (!exists)
                            {
                                return await ToAncillaryFileInfoNotFound(releaseFile);
                            }

                            var blob = await _blobStorageService.GetBlob(PrivateReleaseFiles, releaseFile.Path());
                            return await ToAncillaryFileInfo(releaseFile, blob);
                        });

                    return filesWithMetadata
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public Task<Either<ActionResult, AncillaryFileInfo>> UploadAncillary(Guid releaseId, IFormFile formFile, string name)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(formFile, Ancillary))
                .OnSuccess(async () =>
                {
                    var releaseFile = await _releaseFileRepository.Create(
                        releaseId: releaseId,
                        filename: formFile.FileName,
                        type: Ancillary,
                        createdById: _userService.GetUserId(),
                        name: name);

                    await _contentDbContext.SaveChangesAsync();

                    await _blobStorageService.UploadFile(
                        containerName: PrivateReleaseFiles,
                        path: releaseFile.Path(),
                        file: formFile);

                    var blob = await _blobStorageService.GetBlob(
                        PrivateReleaseFiles,
                        releaseFile.Path());

                    return await ToAncillaryFileInfo(releaseFile, blob);
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
                        ? await _releaseFileRepository.UpdateFilename(
                            releaseId,
                            replacingId.Value,
                            formFile.FileName)
                        : await _releaseFileRepository.Create(
                            releaseId: releaseId,
                            filename: formFile.FileName,
                            type: Chart,
                            createdById: _userService.GetUserId());

                    await _blobStorageService.UploadFile(
                        containerName: PrivateReleaseFiles,
                        path: releaseFile.Path(),
                        file: formFile
                    );

                    var blob = await _blobStorageService.GetBlob(
                        PrivateReleaseFiles,
                        releaseFile.Path());

                    return releaseFile.ToFileInfo(blob);
                });
        }

        private async Task<AncillaryFileInfo> ToAncillaryFileInfo(ReleaseFile releaseFile, BlobInfo blobInfo)
        {
            await _contentDbContext.Entry(releaseFile)
                .Reference(rf => rf.File)
                .LoadAsync();
            await _contentDbContext.Entry(releaseFile.File)
                .Reference(f => f.CreatedBy)
                .LoadAsync();

            return new AncillaryFileInfo
            {
                Id = releaseFile.FileId,
                FileName = releaseFile.File.Filename,
                Name = releaseFile.Name ?? releaseFile.File.Filename,
                Size = blobInfo.Size,
                Type = releaseFile.File.Type,
                UserName = releaseFile.File.CreatedBy?.Email ?? "",
                Created = releaseFile.File.Created
            };
        }

        // TODO: Remove after completion of EES-2343
        private async Task<AncillaryFileInfo> ToAncillaryFileInfoNotFound(ReleaseFile releaseFile)
        {
            await _contentDbContext.Entry(releaseFile)
                .Reference(rf => rf.File)
                .LoadAsync();
            await _contentDbContext.Entry(releaseFile.File)
                .Reference(f => f.CreatedBy)
                .LoadAsync();

            return new AncillaryFileInfo
            {
                Id = releaseFile.File.Id,
                FileName = releaseFile.File.Filename,
                Name = "Unknown",
                Size = "0.00 B",
                Type = releaseFile.File.Type,
                UserName = releaseFile.File.CreatedBy?.Email ?? "",
                Created = releaseFile.File.Created
            };
        }
    }
}
