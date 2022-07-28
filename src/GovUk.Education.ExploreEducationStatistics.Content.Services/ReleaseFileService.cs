#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
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
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseFileService : IReleaseFileService
    {
        /// How long the all files zip should be
        /// cached in blob storage, in seconds.
        private const int AllFilesZipTtl = 60 * 60;

        private static readonly FileType[] AllowedZipFileTypes = {
            FileType.Ancillary,
            FileType.Data,
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
                .OnSuccessCombineWith(rf =>
                    _blobStorageService.DownloadToStream(PublicReleaseFiles, rf.PublicPath(), new MemoryStream()))
                .OnSuccess(methodologyFileAndStream =>
                {
                    var (releaseFile, stream) = methodologyFileAndStream;
                    return new FileStreamResult(stream, releaseFile.File.ContentType)
                    {
                        FileDownloadName = releaseFile.File.Filename
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
                        if (fileIds is null
                            && await TryStreamCachedAllFilesZip(release, outputStream, cancellationToken))
                        {
                            return;
                        }

                        if (fileIds is null)
                        {
                            await ZipAllFilesToStream(release, outputStream, cancellationToken);
                        }
                        else
                        {
                            var releaseFiles = (await QueryReleaseFiles(releaseId)
                                    .Where(rf => fileIds.Contains(rf.FileId))
                                    .ToListAsync())
                                .OrderBy(rf => rf.File.ZipFileEntryName())
                                .ToList();

                            await DoZipFilesToStream(releaseFiles, release, outputStream, cancellationToken);
                        }
                    }
                );
        }

        private async Task<bool> TryStreamCachedAllFilesZip(
            Release release,
            Stream outputStream,
            CancellationToken? cancellationToken = null)
        {
            var path = release.AllFilesZipPath();
            var allFilesZip = await _blobStorageService.FindBlob(PublicReleaseFiles, path);

            // Ideally, we would have some way to do this caching via annotations,
            // but this a chunk of work to get working properly as piping
            // the cached file to target stream isn't super trivial.
            // For now, we'll just do this manually as it's way easier.
            if (allFilesZip?.Updated is not null
                && allFilesZip.Updated.Value.AddSeconds(AllFilesZipTtl) >= DateTime.UtcNow)
            {
                var result = await _blobStorageService.DownloadToStream(
                    containerName: PublicReleaseFiles,
                    path: path,
                    stream: outputStream,
                    cancellationToken: cancellationToken
                );

                await outputStream.DisposeAsync();

                return result.IsRight;
            }

            return false;
        }

        private async Task ZipAllFilesToStream(
            Release release,
            Stream outputStream,
            CancellationToken? cancellationToken = null)
        {
            var releaseFiles = (await QueryReleaseFiles(release.Id).ToListAsync())
                .OrderBy(rf => rf.File.ZipFileEntryName())
                .ToList();

            var path = Path.GetTempPath() + release.AllFilesZipFileName();
            var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            await using var multiWriteStream = new MultiWriteStream(outputStream, fileStream);

            await DoZipFilesToStream(releaseFiles, release, multiWriteStream, cancellationToken);
            await multiWriteStream.FlushAsync();

            // Now cache the All files zip into blob storage
            // so that we can quickly fetch it again.
            fileStream.Position = 0;

            await _blobStorageService.UploadStream(
                containerName: PublicReleaseFiles,
                path: release.AllFilesZipPath(),
                stream: fileStream,
                contentType: MediaTypeNames.Application.Zip
            );

            await fileStream.DisposeAsync();
            File.Delete(path);
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

        private IQueryable<ReleaseFile> QueryReleaseFiles(Guid releaseId)
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
