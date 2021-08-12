#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class ReleaseFileService : IReleaseFileService
    {
        private static readonly FileType[] DownloadableFileTypes = {
            FileType.Ancillary,
            FileType.Data,
        };

        private static readonly FileType[] ZipFileTypes = {
            FileType.Ancillary,
            FileType.Data,
            FileType.DataGuidance,
        };

        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUserService _userService;
        private readonly ILogger<ReleaseFileService> _logger;

        public ReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IUserService userService,
            ILogger<ReleaseFileService> logger)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q
                    .Include(rf => rf.File)
                    .Include(rf => rf.Release)
                    .ThenInclude(release => release.Publication)
                    .Where(rf => rf.ReleaseId == releaseId && rf.FileId == fileId)
                )
                .OnSuccess(async rf =>
                {
                    return await GetBlob(rf.PublicPath())
                        .OnSuccess(blob => DownloadToStreamResult(blob, rf.File.Filename));
                });
        }

        public async Task<Either<ActionResult, Unit>> ZipFilesToStream(
            Guid releaseId,
            IList<Guid> fileIds,
            Stream outputStream,
            CancellationToken? cancellationToken = null)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccessVoid(
                    async () =>
                    {
                        var releaseFiles = (await QueryByFileType(releaseId, ZipFileTypes)
                            .Where(rf => fileIds.Contains(rf.FileId))
                            .ToListAsync())
                            .OrderBy(rf => rf.File.ZipFileEntryName())
                            .ToList();

                        await DoZipFilesToStream(releaseFiles, outputStream, cancellationToken);
                    }
                );
        }

        private async Task DoZipFilesToStream(
            List<ReleaseFile> releaseFiles,
            Stream outputStream,
            CancellationToken? cancellationToken = null)
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
                    PublicReleaseFiles,
                    releaseFile.PublicPath()
                );

                if (!blobExists)
                {
                    continue;
                }

                var entry = archive.CreateEntry(releaseFile.File.ZipFileEntryName());
                await using var entryStream = entry.Open();

                await _blobStorageService.DownloadToStream(
                    containerName: PublicReleaseFiles,
                    path: releaseFile.PublicPath(),
                    stream: entryStream,
                    cancellationToken: cancellationToken
                );
            }
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamByPath(string path)
        {
            return await GetBlob(path)
                .OnSuccess(DownloadToStreamResult);
        }

        private async Task<FileStreamResult> DownloadToStreamResult(BlobInfo blob, string filename)
        {
            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(PublicReleaseFiles, blob.Path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = filename
            };
        }

        private async Task<FileStreamResult> DownloadToStreamResult(BlobInfo blob)
        {
            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(PublicReleaseFiles, blob.Path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = blob.FileName
            };
        }

        private async Task<Either<ActionResult, BlobInfo>> GetBlob(string path)
        {
            if (!await _blobStorageService.CheckBlobExists(PublicReleaseFiles, path))
            {
                return new NotFoundResult();
            }

            var blob = await _blobStorageService.GetBlob(PublicReleaseFiles, path);

            if (!blob.IsReleased())
            {
                return new NotFoundResult();
            }

            return blob;
        }

        public async Task<List<FileInfo>> ListDownloadFiles(Release release)
        {
            var releaseFiles = await ListDownloadableFiles(release.Id);

            // There are no files for this release
            if (releaseFiles.Count == 0)
            {
                return new List<FileInfo>();
            }

            var orderedFiles = (
                    await releaseFiles
                        .SelectAsync(async releaseFile => await GetPublicFileInfo(releaseFile))
                )
                .OrderBy(file => file.Name)
                .ToList();

            // Prepend the "All files" zip
            var allFilesZip = await GetAllFilesZip(release);

            return orderedFiles.Prepend(allFilesZip).ToList();
        }

        private async Task<FileInfo> GetPublicFileInfo(ReleaseFile releaseFile)
        {
            var file = releaseFile.File;

            var exists = await _blobStorageService.CheckBlobExists(
                containerName: PublicReleaseFiles,
                path: file.PublicPath(releaseFile.Release));

            if (!exists)
            {
                _logger.LogWarning("Public blob not found for file: {0} at: {1}", file.Id,
                    releaseFile.PublicPath());

                return releaseFile.ToFileInfoNotFound();
            }

            var blob = await _blobStorageService.GetBlob(
                containerName: PublicReleaseFiles,
                path: file.PublicPath(releaseFile.Release));

            return releaseFile.ToPublicFileInfo(blob);
        }

        private async Task<FileInfo> GetAllFilesZip(Release release)
        {
            var path = release.AllFilesZipPath();

            var exists = await _blobStorageService.CheckBlobExists(
                containerName: PublicReleaseFiles,
                path: path);

            if (!exists)
            {
                _logger.LogError("Public blob not found for 'All files' zip at: {0}", path);

                return new FileInfo
                {
                    FileName = release.AllFilesZipFileName(),
                    Name = "All files",
                    Size = "0.00 B",
                    Type = FileType.Ancillary
                };
            }

            var blob = await _blobStorageService.GetBlob(
                containerName: PublicReleaseFiles,
                path: path);

            return new FileInfo
            {
                FileName = blob.FileName,
                Name = "All files",
                Size = blob.Size,
                Type = FileType.Ancillary
            };
        }

        private IQueryable<ReleaseFile> QueryByFileType(Guid releaseId, params FileType[] types)
        {
            return _contentDbContext.ReleaseFiles
                .Include(f => f.Release)
                .ThenInclude(r => r.Publication)
                .Include(f => f.File)
                .Where(releaseFile => releaseFile.ReleaseId == releaseId
                                      && types.Contains(releaseFile.File.Type));
        }

        private Task<List<ReleaseFile>> ListDownloadableFiles(Guid releaseId)
        {
            return ListByFileType(releaseId, DownloadableFileTypes);
        }

        private async Task<List<ReleaseFile>> ListByFileType(Guid releaseId, params FileType[] types)
        {
            return await QueryByFileType(releaseId, types).ToListAsync();
        }
    }
}
