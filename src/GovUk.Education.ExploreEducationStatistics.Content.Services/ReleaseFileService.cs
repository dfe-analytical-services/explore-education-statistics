#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class ReleaseFileService(
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IPublicBlobStorageService publicBlobStorageService,
    IDataGuidanceFileWriter dataGuidanceFileWriter,
    IUserService userService,
    IAnalyticsManager analyticsManager,
    ILogger<ReleaseFileService> logger)
    : IReleaseFileService
{
    /// How long the all files zip should be
    /// cached in blob storage, in seconds.
    private const int AllFilesZipTtl = 60 * 60;

    private static readonly FileType[] AllowedFileTypes =
    [
        FileType.Ancillary,
        FileType.Data
    ];

    public async Task<Either<ActionResult, IList<ReleaseFileViewModel>>> ListReleaseFiles(
        ReleaseFileListRequest request,
        CancellationToken cancellationToken = default)
    {
        var releaseFiles = await contentDbContext.ReleaseFiles
            .Include(rf => rf.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .Include(rf => rf.File)
            .Where(rf => AllowedFileTypes.Contains(rf.File.Type))
            .Where(rf => request.Ids.Contains(rf.Id))
            .Where(rv => rv.Published != null)
            .ToListAsync(cancellationToken);

        var releaseVersions = releaseFiles.Select(rf => rf.ReleaseVersion)
            .ToHashSet();

        var allowedReleaseVersions = new HashSet<ReleaseVersion>();

        foreach (var releaseVersion in releaseVersions)
        {
            if (await userService.CheckCanViewReleaseVersion(releaseVersion).IsRight())
            {
                allowedReleaseVersions.Add(releaseVersion);
            }
        }

        return releaseFiles
            .Where(rf => allowedReleaseVersions.Contains(rf.ReleaseVersion))
            .Select(MapReleaseFileViewModel)
            .ToList();
    }

    private static ReleaseFileViewModel MapReleaseFileViewModel(ReleaseFile releaseFile)
    {
        var releaseVersion = releaseFile.ReleaseVersion;
        var isLatestPublishedRelease =
            releaseVersion.Id == releaseVersion.Release.Publication.LatestPublishedReleaseVersionId;

        return new ReleaseFileViewModel
        {
            Id = releaseFile.Id,
            File = releaseFile.ToPublicFileInfo(),
            DataSetFileId = releaseFile.File.DataSetFileId,
            Release = new ReleaseSummaryViewModel(releaseVersion, latestPublishedRelease: isLatestPublishedRelease)
            {
                Publication = new PublicationSummaryViewModel(releaseVersion.Release.Publication)
            },
        };
    }

    public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseVersionId, Guid fileId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseFile>(q => q
                .Include(rf => rf.File)
                .Include(rf => rf.ReleaseVersion)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.FileId == fileId)
            )
            .OnSuccessDo(rf => userService.CheckCanViewReleaseVersion(rf.ReleaseVersion))
            .OnSuccessCombineWith(rf =>
                publicBlobStorageService.DownloadToStream(PublicReleaseFiles, rf.PublicPath(), new MemoryStream()))
            .OnSuccess(tuple =>
            {
                var (releaseFile, stream) = tuple;

                return new FileStreamResult(stream, releaseFile.File.ContentType)
                {
                    FileDownloadName = releaseFile.File.Filename
                };
            });
    }

    public async Task<Either<ActionResult, Unit>> ZipFilesToStream(
        Guid releaseVersionId,
        Stream outputStream,
        AnalyticsFromPage fromPage,
        IEnumerable<Guid>? fileIds = null,
        CancellationToken cancellationToken = default)
    {
        return await contentDbContext.ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccessVoid(
                async releaseVersion =>
                {
                    List<ReleaseFile>? releaseFiles = null;

                    if (fileIds is null)
                    {
                        var successfullyStreamCachedAllFilesZip =
                            await TryStreamCachedAllFilesZip(releaseVersion, outputStream, cancellationToken);

                        if (!successfullyStreamCachedAllFilesZip)
                        {
                            await ZipAllFilesToStream(releaseVersion, outputStream, cancellationToken);
                        }
                    }
                    else
                    {
                        releaseFiles = (await QueryReleaseFiles(releaseVersionId)
                            .Where(rf => fileIds.Contains(rf.FileId))
                            .ToListAsync(cancellationToken: cancellationToken))
                            .OrderBy(rf => rf.File.ZipFileEntryName())
                            .ToList();

                        await DoZipFilesToStream(releaseFiles, releaseVersion, outputStream, cancellationToken);
                    }

                    await RecordZipDownloadAnalytics(releaseVersion, releaseFiles, fromPage, cancellationToken);
                }
            );
    }

    private async Task<bool> TryStreamCachedAllFilesZip(
        ReleaseVersion releaseVersion,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var path = releaseVersion.AllFilesZipPath();
        var allFilesZip = await publicBlobStorageService.FindBlob(PublicReleaseFiles, path);

        // Ideally, we would have some way to do this caching via annotations,
        // but this a chunk of work to get working properly as piping
        // the cached file to target stream isn't super trivial.
        // For now, we'll just do this manually as it's way easier.
        if (allFilesZip?.Updated is not null
            && allFilesZip.Updated.Value.AddSeconds(AllFilesZipTtl) >= DateTime.UtcNow)
        {
            var result = await publicBlobStorageService.DownloadToStream(
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
        ReleaseVersion releaseVersion,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var releaseFiles = (await QueryReleaseFiles(releaseVersion.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .OrderBy(rf => rf.File.ZipFileEntryName())
            .ToList();

        var path = Path.GetTempPath() + releaseVersion.AllFilesZipFileName();
        var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        await using var multiWriteStream = new MultiWriteStream(outputStream, fileStream);

        await DoZipFilesToStream(releaseFiles, releaseVersion, multiWriteStream, cancellationToken);
        await multiWriteStream.FlushAsync(cancellationToken);

        // Now cache the All files zip into blob storage
        // so that we can quickly fetch it again.
        fileStream.Position = 0;

        await publicBlobStorageService.UploadStream(
            containerName: PublicReleaseFiles,
            path: releaseVersion.AllFilesZipPath(),
            sourceStream: fileStream,
            contentType: MediaTypeNames.Application.Zip,
            cancellationToken: cancellationToken
        );

        await fileStream.DisposeAsync();
        File.Delete(path);
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
            // Stop immediately if we receive a cancellation request
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var blobExists = await publicBlobStorageService.CheckBlobExists(
                PublicReleaseFiles,
                releaseFile.PublicPath()
            );

            // Ignore files which do not exist in blob storage
            if (!blobExists)
            {
                continue;
            }

            var entry = archive.CreateEntry(releaseFile.File.ZipFileEntryName());

            await using var entryStream = entry.Open();

            await publicBlobStorageService.DownloadToStream(
                containerName: PublicReleaseFiles,
                path: releaseFile.PublicPath(),
                stream: entryStream,
                cancellationToken: cancellationToken
            );

            releaseFilesWithZipEntries.Add(releaseFile);
        }

        // Add data guidance file if there are any data files in this zip.
        if (releaseFilesWithZipEntries.Any(rf => rf.File.Type == FileType.Data))
        {
            var entry = archive.CreateEntry(FileType.DataGuidance.GetEnumLabel() + "/data-guidance.txt");

            await using var entryStream = entry.Open();

            var dataFileIds = releaseFilesWithZipEntries
                .Where(rf => rf.File.Type == FileType.Data)
                .Select(rf => rf.FileId)
                .ToList();

            await dataGuidanceFileWriter.WriteToStream(entryStream, releaseVersion, dataFileIds);
        }
    }

    private IQueryable<ReleaseFile> QueryReleaseFiles(Guid releaseVersionId)
    {
        return contentDbContext.ReleaseFiles
            .Include(f => f.ReleaseVersion)
            .Include(f => f.File)
            .Where(releaseFile => releaseFile.ReleaseVersionId == releaseVersionId
                                  && AllowedFileTypes.Contains(releaseFile.File.Type));
    }

    private async Task RecordZipDownloadAnalytics(
        ReleaseVersion releaseVersion,
        List<ReleaseFile>? releaseFiles,
        AnalyticsFromPage fromPage,
        CancellationToken cancellationToken)
    {
        if (releaseFiles is not null && releaseFiles.Count > 1)
        {
            logger.LogWarning("We only record analytics for zip downloads for an entire release or one specific data set. So this means someone manually attempted to download a zip with more than one specific file?");
            return;
        }

        Guid? subjectId = null;
        string? dataSetName = null;

        if (releaseFiles is not null && releaseFiles.Count == 1)
        {
            subjectId = releaseFiles[0].File.SubjectId;
            dataSetName = releaseFiles[0].Name;
        }

        try
        {
            await analyticsManager.Add(
                new CaptureZipDownloadRequest
                {
                    PublicationName = releaseVersion.Release.Publication.Title,
                    ReleaseVersionId = releaseVersion.Id,
                    ReleaseName = releaseVersion.Release.Title,
                    ReleaseLabel = releaseVersion.Release.Label,
                    FromPage = fromPage,
                    SubjectId = subjectId,
                    DataSetTitle = dataSetName,
                },
                cancellationToken);
        }
        catch (Exception e)
        {
            if (subjectId == null)
            {
                logger.LogError(
                    exception: e,
                    message: "Error whilst capturing zip download analytics for releaseVersion {ReleaseVersion}",
                    releaseVersion.Id);
            }
            else
            {
                logger.LogError(
                    exception: e,
                    message: "Error whilst capturing zip download analytics for releaseVersion {ReleaseVersionId} and subject {SubjectId}",
                    releaseVersion.Id,
                    subjectId);

            }
        }
    }
}
