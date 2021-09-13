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
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseFileService : IReleaseFileService
    {
        private static readonly FileType[] ZipFileTypes = {
            FileType.Ancillary,
            FileType.Data,
            FileType.DataGuidance,
        };

        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDataGuidanceFileWriter _dataGuidanceFileWriter;
        private readonly IUserService _userService;

        public ReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IDataGuidanceFileWriter dataGuidanceFileWriter,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _dataGuidanceFileWriter = dataGuidanceFileWriter;
            _userService = userService;
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q
                    .Include(rf => rf.File)
                    .Include(rf => rf.Release)
                    .ThenInclude(release => release.Publication)
                    .Where(rf => rf.ReleaseId == releaseId && rf.FileId == fileId)
                )
                .OnSuccessDo(rf => _userService.CheckCanViewRelease(rf.Release))
                .OnSuccess(async rf =>
                {
                    return await GetBlob(rf.PublicPath())
                        .OnSuccess(blob => DownloadToStreamResult(blob, rf.File.Filename));
                });
        }

        public async Task<Either<ActionResult, Unit>> ZipFilesToStream(
            Guid releaseId,
            IEnumerable<Guid> fileIds,
            Stream outputStream,
            CancellationToken? cancellationToken = null)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccessVoid(
                    async release =>
                    {
                        var releaseFiles = (await QueryByFileType(releaseId, ZipFileTypes)
                            .Where(rf => fileIds.Contains(rf.FileId))
                            .ToListAsync())
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

                var entry = archive
                    .CreateEntry(releaseFile.File.ZipFileEntryName())
                    .SetUnixPermissions("664");

                await using var entryStream = entry.Open();

                await _blobStorageService.DownloadToStream(
                    containerName: PublicReleaseFiles,
                    path: releaseFile.PublicPath(),
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

        public async Task<Either<ActionResult, FileStreamResult>> StreamAllFilesZip(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    q => q.Include(release => release.Publication))
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => GetBlob(release.AllFilesZipPath()))
                .OnSuccess(DownloadToStreamResult);
        }

        private async Task<FileStreamResult> DownloadToStreamResult(BlobInfo blob, string filename)
        {
            var stream = new MemoryStream();
            var next = await _blobStorageService.DownloadToStream(PublicReleaseFiles, blob.Path, stream);

            return new FileStreamResult(next, blob.ContentType)
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
            var blob = await _blobStorageService.FindBlob(PublicReleaseFiles, path);

            if (blob is null)
            {
                return new NotFoundResult();
            }

            return blob;
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
    }
}
