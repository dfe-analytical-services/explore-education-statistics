using System.IO.Compression;
using System.Net.Mime;
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
    ILogger<ReleaseFileService> logger
) : IReleaseFileService
{
    private static readonly FileType[] AllowedFileTypes = [FileType.Ancillary, FileType.Data];

    public async Task<Either<ActionResult, IList<ReleaseFileViewModel>>> ListReleaseFiles(
        ReleaseFileListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var releaseFiles = await contentDbContext
            .ReleaseFiles.Include(rf => rf.ReleaseVersion)
                .ThenInclude(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
            .Include(rf => rf.File)
            .Where(rf => AllowedFileTypes.Contains(rf.File.Type))
            .Where(rf => request.Ids.Contains(rf.Id))
            .Where(rv => rv.Published != null)
            .ToListAsync(cancellationToken);

        var releaseVersions = releaseFiles.Select(rf => rf.ReleaseVersion).ToHashSet();

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
                Publication = new PublicationSummaryViewModel(releaseVersion.Release.Publication),
            },
        };
    }

    public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseVersionId, Guid fileId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseFile>(q =>
                q.Include(rf => rf.File)
                    .Include(rf => rf.ReleaseVersion)
                    .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.FileId == fileId)
            )
            .OnSuccessDo(releaseFile => userService.CheckCanViewReleaseVersion(releaseFile.ReleaseVersion))
            .OnSuccessCombineWith(releaseFile =>
                publicBlobStorageService.GetDownloadStream(PublicReleaseFiles, releaseFile.PublicPath())
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

    public async Task<Either<ActionResult, ZipDelivery>> GetZipDelivery(
        ReleaseVersion releaseVersion,
        AnalyticsFromPage fromPage,
        IEnumerable<Guid>? fileIds,
        CancellationToken cancellationToken = default
    )
    {
        if (fileIds is not null || releaseVersion.Published is null || releaseVersion.Published > DateTimeOffset.UtcNow)
        {
            return new ZipDelivery.Stream(releaseVersion);
        }

        var permissionResult = await userService.CheckCanViewReleaseVersion(releaseVersion);
        if (permissionResult.IsLeft)
        {
            return permissionResult.Left;
        }

        // Temporarily disabled for the AFD-generated ZIP POC. Published all-files
        // requests always redirect to the versioned endpoint so AFD can cache its response.
        // if (await FindValidAllFilesZip(releaseVersion, AllFilesZipFormat.CurrentVersion) is null)
        // {
        //     return new ZipDelivery.Stream(releaseVersion);
        // }

        await RecordZipDownloadAnalytics(releaseVersion, releaseFiles: null, fromPage, cancellationToken);

        return new ZipDelivery.Redirect($"/api/all-files/{releaseVersion.Id}/v{AllFilesZipFormat.CurrentVersion}");
    }

    public async Task<Either<ActionResult, FileStreamResult>> StreamCachedAllFilesZip(
        Guid releaseVersionId,
        int formatVersion,
        CancellationToken cancellationToken = default
    )
    {
        if (formatVersion != AllFilesZipFormat.CurrentVersion)
        {
            return new NotFoundResult();
        }

        var releaseVersionResult = await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(EnsurePublished)
            .OnSuccess(userService.CheckCanViewReleaseVersion);

        if (releaseVersionResult.IsLeft)
        {
            return releaseVersionResult.Left;
        }

        var releaseVersion = releaseVersionResult.Right;

        // Temporarily disabled for the AFD-generated ZIP POC. The versioned endpoint
        // generates a fresh ZIP on an AFD cache miss instead of reading the combined ZIP
        // from Blob Storage.
        // return await FindValidAllFilesZipOrNotFound(releaseVersion, formatVersion)
        //     .OnSuccess(allFilesZip =>
        //         publicBlobStorageService
        //             .GetDownloadStream(
        //                 PublicReleaseFiles,
        //                 allFilesZip.Path,
        //                 cancellationToken: cancellationToken
        //             )
        //             .OnSuccess(stream => new FileStreamResult(stream, MediaTypeNames.Application.Zip)
        //             {
        //                 FileDownloadName = releaseVersion.AllFilesZipFileName(),
        //                 EnableRangeProcessing = true,
        //             })
        //     );

        var temporaryZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        var temporaryZipStream = new FileStream(
            temporaryZipPath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose
        );

        try
        {
            await ZipAllFilesToStream(
                releaseVersion,
                temporaryZipStream,
                cancellationToken,
                leaveOutputStreamOpen: true
            );
            temporaryZipStream.Position = 0;

            return new FileStreamResult(temporaryZipStream, MediaTypeNames.Application.Zip)
            {
                FileDownloadName = releaseVersion.AllFilesZipFileName(),
                EnableRangeProcessing = true,
            };
        }
        catch
        {
            await temporaryZipStream.DisposeAsync();
            throw;
        }
    }

    public async Task<Either<ActionResult, Unit>> ZipFilesToStream(
        Guid releaseVersionId,
        Stream outputStream,
        AnalyticsFromPage fromPage,
        IEnumerable<Guid>? fileIds = null,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccessVoid(async releaseVersion =>
            {
                List<ReleaseFile>? releaseFiles = null;

                if (fileIds is null)
                {
                    // Temporarily disabled for the AFD-generated ZIP POC. All-files ZIPs
                    // are generated rather than read from the combined ZIP Blob.
                    // var successfullyStreamCachedAllFilesZip = await TryStreamCachedAllFilesZip(
                    //     releaseVersion,
                    //     outputStream,
                    //     cancellationToken
                    // );
                    //
                    // if (!successfullyStreamCachedAllFilesZip)
                    // {
                    //     await ZipAllFilesToStream(releaseVersion, outputStream, cancellationToken);
                    // }

                    await ZipAllFilesToStream(releaseVersion, outputStream, cancellationToken);
                }
                else
                {
                    releaseFiles = (
                        await QueryReleaseFiles(releaseVersionId)
                            .Where(rf => fileIds.Contains(rf.FileId))
                            .ToListAsync(cancellationToken: cancellationToken)
                    )
                        .OrderBy(rf => rf.File.ZipFileEntryName())
                        .ToList();

                    await DoZipFilesToStream(releaseFiles, releaseVersion, outputStream, cancellationToken);
                }

                await RecordZipDownloadAnalytics(releaseVersion, releaseFiles, fromPage, cancellationToken);
            });
    }

    private async Task<bool> TryStreamCachedAllFilesZip(
        ReleaseVersion releaseVersion,
        Stream outputStream,
        CancellationToken cancellationToken
    )
    {
        var allFilesZip = await FindValidAllFilesZip(releaseVersion, AllFilesZipFormat.CurrentVersion);
        if (allFilesZip is not null)
        {
            var streamResult = await publicBlobStorageService.GetDownloadStream(
                containerName: PublicReleaseFiles,
                path: allFilesZip.Path,
                cancellationToken: cancellationToken
            );

            if (streamResult.IsLeft)
            {
                return false;
            }

            await using var blobStream = streamResult.Right;
            await blobStream.CopyToAsync(outputStream, cancellationToken);
            return true;
        }

        return false;
    }

    private async Task<BlobInfo?> FindValidAllFilesZip(ReleaseVersion releaseVersion, int formatVersion)
    {
        if (releaseVersion.Published is null || releaseVersion.Published > DateTimeOffset.UtcNow)
        {
            return null;
        }

        var allFilesZip = await publicBlobStorageService.FindBlob(
            PublicReleaseFiles,
            releaseVersion.AllFilesZipPath(formatVersion)
        );

        if (allFilesZip?.Updated is null)
        {
            return null;
        }

        return allFilesZip.Updated >= releaseVersion.Published ? allFilesZip : null;
    }

    private static Either<ActionResult, ReleaseVersion> EnsurePublished(ReleaseVersion releaseVersion)
    {
        return releaseVersion.Published is null || releaseVersion.Published > DateTimeOffset.UtcNow
            ? new NotFoundResult()
            : releaseVersion;
    }

    private async Task<Either<ActionResult, BlobInfo>> FindValidAllFilesZipOrNotFound(
        ReleaseVersion releaseVersion,
        int formatVersion
    )
    {
        return await FindValidAllFilesZip(releaseVersion, formatVersion) is { } allFilesZip
            ? allFilesZip
            : new NotFoundResult();
    }

    private async Task ZipAllFilesToStream(
        ReleaseVersion releaseVersion,
        Stream outputStream,
        CancellationToken cancellationToken,
        bool leaveOutputStreamOpen = false
    )
    {
        var releaseFiles = (
            await QueryReleaseFiles(releaseVersion.Id).ToListAsync(cancellationToken: cancellationToken)
        )
            .OrderBy(rf => rf.File.ZipFileEntryName())
            .ToList();

        // var path = Path.GetTempPath() + releaseVersion.AllFilesZipFileName();
        // var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        // await using var multiWriteStream = new MultiWriteStream(outputStream, fileStream);

        await DoZipFilesToStream(releaseFiles, releaseVersion, outputStream, cancellationToken, leaveOutputStreamOpen);

        if (leaveOutputStreamOpen)
        {
            await outputStream.FlushAsync(cancellationToken);
        }

        // Now cache the All files zip into blob storage
        // so that we can quickly fetch it again.
        // fileStream.Position = 0;

        // await publicBlobStorageService.UploadStream(
        //     containerName: PublicReleaseFiles,
        //     path: releaseVersion.AllFilesZipPath(AllFilesZipFormat.CurrentVersion),
        //     sourceStream: fileStream,
        //     contentType: MediaTypeNames.Application.Zip,
        //     cancellationToken: cancellationToken
        // );
        //
        // await fileStream.DisposeAsync();
        // File.Delete(path);
    }

    private async Task DoZipFilesToStream(
        List<ReleaseFile> releaseFiles,
        ReleaseVersion releaseVersion,
        Stream outputStream,
        CancellationToken cancellationToken,
        bool leaveOutputStreamOpen = false
    )
    {
        using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: leaveOutputStreamOpen);

        var releaseFilesWithZipEntries = new List<ReleaseFile>();

        foreach (var releaseFile in releaseFiles)
        {
            var streamResult = await publicBlobStorageService.GetDownloadStream(
                containerName: PublicReleaseFiles,
                path: releaseFile.PublicPath(),
                cancellationToken: cancellationToken
            );

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
            await blobStream.CopyToAsync(destination: entryStream, cancellationToken: cancellationToken);

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
        return contentDbContext
            .ReleaseFiles.Include(f => f.ReleaseVersion)
            .Include(f => f.File)
            .Where(releaseFile =>
                releaseFile.ReleaseVersionId == releaseVersionId && AllowedFileTypes.Contains(releaseFile.File.Type)
            );
    }

    private async Task RecordZipDownloadAnalytics(
        ReleaseVersion releaseVersion,
        List<ReleaseFile>? releaseFiles,
        AnalyticsFromPage fromPage,
        CancellationToken cancellationToken
    )
    {
        if (releaseFiles is not null && releaseFiles.Count > 1)
        {
            logger.LogWarning(
                "We only record analytics for zip downloads for an entire release or one specific data set. So this means someone manually attempted to download a zip with more than one specific file?"
            );
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
                cancellationToken
            );
        }
        catch (Exception e)
        {
            if (subjectId == null)
            {
                logger.LogError(
                    exception: e,
                    message: "Error whilst capturing zip download analytics for releaseVersion {ReleaseVersion}",
                    releaseVersion.Id
                );
            }
            else
            {
                logger.LogError(
                    exception: e,
                    message: "Error whilst capturing zip download analytics for releaseVersion {ReleaseVersionId} and subject {SubjectId}",
                    releaseVersion.Id,
                    subjectId
                );
            }
        }
    }
}
