using System.IO.Compression;
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class ReleaseFileServiceTests : IDisposable
{
    private readonly DataFixture _dataFixture = new();
    private readonly List<string> _filePaths = new();

    public void Dispose()
    {
        // Cleanup any files that have been
        // written to the filesystem.
        _filePaths.ForEach(File.Delete);
    }

    [Fact]
    public async Task StreamFile()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                ContentType = "application/pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile.PublicPath(), "Test blob");

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = await service.StreamFile(releaseVersionId: releaseFile.ReleaseVersionId,
                fileId: releaseFile.File.Id);

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.True(result.IsRight);

            Assert.Equal("application/pdf", result.Right.ContentType);
            Assert.Equal("ancillary.pdf", result.Right.FileDownloadName);
            Assert.Equal("Test blob", result.Right.FileStream.ReadToEnd());
        }
    }

    [Fact]
    public async Task StreamFile_ReleaseNotFound()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.StreamFile(Guid.NewGuid(), releaseFile.File.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task StreamFile_ReleaseFileNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.StreamFile(releaseVersionId: releaseVersion.Id,
                fileId: Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task StreamFile_BlobDoesNotExist()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        publicBlobStorageService.SetupDownloadToStreamNotFound(PublicReleaseFiles, releaseFile.PublicPath());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = await service.StreamFile(releaseVersionId: releaseFile.ReleaseVersionId,
                fileId: releaseFile.File.Id);

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task ZipFilesToStream_ValidFileTypes()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };
        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile2.PublicPath(), true);
        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile1.PublicPath(), "Test data blob");
        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile2.PublicPath(), "Test ancillary blob");

        var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(MockBehavior.Strict);

        dataGuidanceFileWriter
            .Setup(
                s => s.WriteToStream(
                    It.IsAny<Stream>(),
                    It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                    ListOf(releaseFile1.FileId))
            )
            .Returns<Stream, ReleaseVersion, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
            .Callback<Stream, ReleaseVersion, IEnumerable<Guid>?>(
                (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
            );

        // This should not happen during normal usage, as the public frontend doesn't allow users to request
        // multiple files. At the time of writing, the service only officially allows users to download all data
        // sets for a release (if fileIds is null) or a specific data set. But this endpoint takes an array of
        // files as this is what the old Data catalogue page allowed.
        var logger = new Mock<ILogger<ReleaseFileService>>(MockBehavior.Strict);
        var expectedWarning =
            "We only record analytics for zip downloads for an entire release or one specific data set. So this means someone manually attempted to download a zip with more than one specific file?";
        MockUtils.ExpectLogMessage(logger, LogLevel.Warning, expectedWarning);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                dataGuidanceFileWriter: dataGuidanceFileWriter.Object,
                logger: logger.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds,
                fromPage: AnalyticsFromPage.DataCatalogue
            );

            MockUtils.VerifyAllMocks(dataGuidanceFileWriter);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Equal(3, zip.Entries.Count);
            Assert.Equal("data/data.csv", zip.Entries[0].FullName);
            Assert.Equal("Test data blob", zip.Entries[0].Open().ReadToEnd());

            Assert.Equal("supporting-files/ancillary.pdf", zip.Entries[1].FullName);
            Assert.Equal("Test ancillary blob", zip.Entries[1].Open().ReadToEnd());

            // Data guidance is generated if there is at least one data file
            Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
            Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_DataGuidanceForSingleDataFile()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data-1.csv",
                Type = FileType.Data
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath(), true);
        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile.PublicPath(), "Test data 1 blob");

        var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(MockBehavior.Strict);

        dataGuidanceFileWriter
            .Setup(
                s => s.WriteToStream(
                    It.IsAny<Stream>(),
                    It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                    ListOf(releaseFile.FileId))
            )
            .Returns<Stream, ReleaseVersion, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
            .Callback<Stream, ReleaseVersion, IEnumerable<Guid>?>(
                (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
            );

        var captureRequest = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            SubjectId = releaseFile.File.SubjectId,
            DataSetTitle = releaseFile.Name,
            FromPage = AnalyticsFromPage.DataCatalogue,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(captureRequest),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                dataGuidanceFileWriter: dataGuidanceFileWriter.Object,
                analyticsManager: analyticsManager.Object);

            var fileId = releaseFile.FileId;

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: [fileId],
                fromPage: AnalyticsFromPage.DataCatalogue);

            MockUtils.VerifyAllMocks(dataGuidanceFileWriter);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Equal(2, zip.Entries.Count);
            Assert.Equal("data/data-1.csv", zip.Entries[0].FullName);
            Assert.Equal("Test data 1 blob", zip.Entries[0].Open().ReadToEnd());

            // Data guidance is generated if there is at least one data file
            Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[1].FullName);
            Assert.Equal("Test data guidance blob", zip.Entries[1].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_FiltersInvalidFileTypes()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.meta.csv",
                Type = Metadata,
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip
            }
        };
        var releaseFile3 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.jpg",
                Type = Chart
            }
        };
        var releaseFile4 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.jpg",
                Type = Image
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3, releaseFile4);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        var stream = File.OpenWrite(path);

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        var captureRequest = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.ReleaseUsefulInfo,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(captureRequest),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                analyticsManager: analyticsManager.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fromPage: AnalyticsFromPage.ReleaseUsefulInfo,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_FiltersFilesNotInBlobStorage()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.pdf",
                Type = FileType.Data,
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        var stream = File.OpenWrite(path);

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        // File does not exist in blob storage
        publicBlobStorageService.SetupCheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath(), false);

        var request = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.DataCatalogue,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(request),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                analyticsManager: analyticsManager.Object);

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fromPage: AnalyticsFromPage.DataCatalogue,
                fileIds: [releaseFile.FileId]
            );

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_FiltersFilesForOtherReleases()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        // Files are for other releases
        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-1.pdf",
                Type = Ancillary,
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-2.pdf",
                Type = Ancillary
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        var stream = File.OpenWrite(path);

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        var request = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.ReleaseDownloads,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(request),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                analyticsManager: analyticsManager.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fromPage: AnalyticsFromPage.ReleaseDownloads,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_Empty()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        var stream = File.OpenWrite(path);

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        var request = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.ReleaseUsefulInfo,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(request),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                analyticsManager: analyticsManager.Object);

            var fileIds = ListOf(Guid.NewGuid(), Guid.NewGuid());
            var result = await service.ZipFilesToStream(
                releaseVersion.Id,
                stream,
                AnalyticsFromPage.ReleaseUsefulInfo,
                fileIds);

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.True(result.IsRight);

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_Cancelled()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-1.pdf",
                Type = Ancillary
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-2.pdf",
                Type = Ancillary
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        var stream = File.OpenWrite(path);

        var tokenSource = new CancellationTokenSource();

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        // After the first file has completed, we cancel the request
        // to prevent the next file from being fetched.
        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);

        publicBlobStorageService
            .SetupDownloadToStream(
                container: PublicReleaseFiles,
                path: releaseFile1.PublicPath(),
                content: "Test ancillary blob",
                cancellationToken: tokenSource.Token,
                callback: tokenSource.Cancel);

        // This should not happen during normal usage, as the public frontend doesn't allow users to request
        // multiple files. At the time of writing, the service only officially allows users to download all data
        // sets for a release (if fileIds is null) or a specific data set. But this endpoint takes an array of
        // files as this is what the old Data catalogue page allowed.
        var logger = new Mock<ILogger<ReleaseFileService>>(MockBehavior.Strict);
        var expectedWarning =
            "We only record analytics for zip downloads for an entire release or one specific data set. So this means someone manually attempted to download a zip with more than one specific file?";
        MockUtils.ExpectLogMessage(logger, LogLevel.Warning, expectedWarning);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                logger: logger.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fromPage: AnalyticsFromPage.DataCatalogue,
                fileIds: fileIds,
                cancellationToken: tokenSource.Token
            );

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Single(zip.Entries);
            Assert.Equal("supporting-files/ancillary-1.pdf", zip.Entries[0].FullName);
            Assert.Equal("Test ancillary blob", zip.Entries[0].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_NoFileIds_NoCachedAllFilesZip()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithSlug("publication-slug")));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };
        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile2.PublicPath(), true);
        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile1.PublicPath(), "Test data blob");
        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile2.PublicPath(), "Test ancillary blob");

        var allFilesZipPath = releaseVersion.AllFilesZipPath();

        // No 'All files' zip can be found in blob storage - not cached
        publicBlobStorageService
            .SetupFindBlob(PublicReleaseFiles, allFilesZipPath, null);

        // 'All files' zip will be uploaded to blob storage to be cached
        publicBlobStorageService
            .Setup(
                s =>
                    s.UploadStream(
                        PublicReleaseFiles,
                        allFilesZipPath,
                        It.IsAny<Stream>(),
                        MediaTypeNames.Application.Zip,
                        null,
                        It.IsAny<CancellationToken>()
                    )
            )
            .Returns(Task.CompletedTask);

        var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(MockBehavior.Strict);

        dataGuidanceFileWriter
            .Setup(
                s => s.WriteToStream(
                    It.IsAny<Stream>(),
                    It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                    ListOf(releaseFile1.FileId))
            )
            .Returns<Stream, ReleaseVersion, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
            .Callback<Stream, ReleaseVersion, IEnumerable<Guid>?>(
                (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
            );

        var request = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.ReleaseDownloads,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(request),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                dataGuidanceFileWriter: dataGuidanceFileWriter.Object,
                analyticsManager: analyticsManager.Object);

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                fromPage: AnalyticsFromPage.ReleaseDownloads,
                outputStream: stream
            );

            MockUtils.VerifyAllMocks(dataGuidanceFileWriter);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Equal(3, zip.Entries.Count);
            Assert.Equal("data/data.csv", zip.Entries[0].FullName);
            Assert.Equal("Test data blob", zip.Entries[0].Open().ReadToEnd());

            Assert.Equal("supporting-files/ancillary.pdf", zip.Entries[1].FullName);
            Assert.Equal("Test ancillary blob", zip.Entries[1].Open().ReadToEnd());

            // Data guidance is generated if there is at least one data file
            Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
            Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_NoFileIds_CachedAllFilesZip()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithSlug("publication-slug")));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        var allFilesZipPath = releaseVersion.AllFilesZipPath();

        // 'All files' zip is in blob storage - cached
        publicBlobStorageService
            .SetupFindBlob(
                PublicReleaseFiles,
                allFilesZipPath,
                new BlobInfo(
                    path: allFilesZipPath,
                    contentType: "application/zip",
                    contentLength: 1000L,
                    updated: DateTimeOffset.UtcNow.AddMinutes(-5)
                )
            );

        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, allFilesZipPath, "Test cached all files zip");

        var request = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.DataCatalogue,
        };
        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(request),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                analyticsManager: analyticsManager.Object);

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                fromPage: AnalyticsFromPage.DataCatalogue,
                outputStream: stream
            );

            result.AssertRight();

            await using var zip = File.OpenRead(path);
            Assert.Equal("Test cached all files zip", zip.ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_NoFileIds_StaleCachedAllFilesZip()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithSlug("publication-slug")));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new Model.File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary,
            }
        };

        var releaseFiles = ListOf(releaseFile1);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        var allFilesZipPath = releaseVersion.AllFilesZipPath();

        // 'All files' zip is in blob storage - cached, but stale
        publicBlobStorageService
            .SetupFindBlob(
                PublicReleaseFiles,
                allFilesZipPath,
                new BlobInfo(
                    path: allFilesZipPath,
                    contentType: "application/zip",
                    contentLength: 1000L,
                    updated: DateTimeOffset.UtcNow.AddMinutes(-60)
                )
            );

        publicBlobStorageService
            .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
        publicBlobStorageService
            .SetupDownloadToStream(PublicReleaseFiles, releaseFile1.PublicPath(), "Test ancillary blob");

        // 'All files' zip will be uploaded to blob storage to be re-cached
        publicBlobStorageService
            .Setup(
                s =>
                    s.UploadStream(
                        PublicReleaseFiles,
                        allFilesZipPath,
                        It.IsAny<Stream>(),
                        MediaTypeNames.Application.Zip,
                        null,
                        It.IsAny<CancellationToken>()
                    )
            )
            .Returns(Task.CompletedTask);

        var request = new CaptureZipDownloadRequest
        {
            PublicationName = releaseVersion.Release.Publication.Title,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseName = releaseVersion.Release.Title,
            ReleaseLabel = releaseVersion.Release.Label,
            FromPage = AnalyticsFromPage.ReleaseUsefulInfo,
        };

        var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
        analyticsManager.Setup(m => m.Add(
                ItIs.DeepEqualTo(request),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                analyticsManager: analyticsManager.Object);

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                fromPage: AnalyticsFromPage.ReleaseUsefulInfo,
                outputStream: stream
            );

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Single(zip.Entries);
            Assert.Equal("supporting-files/ancillary.pdf", zip.Entries[0].FullName);
            Assert.Equal("Test ancillary blob", zip.Entries[0].Open().ReadToEnd());
        }
    }

    private string GenerateZipFilePath()
    {
        var path = Path.GetTempPath() + Guid.NewGuid() + ".zip";
        _filePaths.Add(path);

        return path;
    }

    private static ReleaseFileService SetupReleaseFileService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IPublicBlobStorageService? publicBlobStorageService = null,
        IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
        IUserService? userService = null,
        IAnalyticsManager? analyticsManager = null,
        ILogger<ReleaseFileService>? logger = null)
    {
        return new(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(MockBehavior.Strict),
            dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            analyticsManager ?? Mock.Of<IAnalyticsManager>(),
            logger ?? Mock.Of<ILogger<ReleaseFileService>>(MockBehavior.Strict)
        );
    }
}
