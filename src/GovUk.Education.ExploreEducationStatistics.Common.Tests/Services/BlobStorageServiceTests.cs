#nullable enable
using System.Collections.Immutable;
using System.IO.Compression;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using static Azure.Storage.Blobs.Models.BlobsModelFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.BlobServiceTestUtils;
using static Moq.MockBehavior;
using Capture = Moq.Capture;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public class BlobStorageServiceTests
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TestClass(string Value);

    [Fact]
    public async Task CheckBlobExists_BlobExists()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(name: path, exists: true);

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        Assert.True(await service.CheckBlobExists(PublicReleaseFiles, path));
    }

    [Fact]
    public async Task CheckBlobExists_BlobDoesNotExist()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(name: path, exists: false);

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        Assert.False(await service.CheckBlobExists(PublicReleaseFiles, path));
    }

    [Fact]
    public async Task FindBlob()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(name: path, contentType: MediaTypeNames.Application.Pdf, exists: true);

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var blob = await service.FindBlob(PublicReleaseFiles, path);

        Assert.NotNull(blob);
        Assert.Equal(path, blob.Path);
    }

    [Fact]
    public async Task GetBlobs()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Returns(new TestAsyncPageable<BlobItem>(CreatePages(new List<BlobItem> { BlobItem("item-1") })));

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        Assert.Equal(["item-1"], await service.GetBlobs(PublicReleaseFiles));
    }

    [Fact]
    public async Task GetBlobs_ContainerNotFound_ReturnsNoBlobs()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Throws(
                new RequestFailedException(
                    status: 404,
                    message: "The specified container does not exist.",
                    errorCode: BlobErrorCode.ContainerNotFound.ToString(),
                    innerException: null
                )
            );

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        Assert.Empty(await service.GetBlobs(PublicReleaseFiles));
    }

    [Fact]
    public async Task DownloadBlobText()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadContentAsync(content: "Test content");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.DownloadBlobText(containerName: PublicReleaseFiles, path: path);

        result.AssertRight("Test content");
    }

    [Fact]
    public async Task DownloadBlobText_NotFound()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadContentAsyncNotFound();

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.DownloadBlobText(containerName: PublicReleaseFiles, path: path);

        result.AssertNotFound();
    }

    [Fact]
    public async Task DownloadToStream()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadToAsync("Test content");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        await using var targetStream = new MemoryStream();
        await service.DownloadToStream(containerName: PublicReleaseFiles, path: path, targetStream: targetStream);

        Assert.Equal(0, targetStream.Position);
        Assert.Equal("Test content", targetStream.ReadToEnd());
    }

    [Fact]
    public async Task DownloadToStream_NotFound()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>(),
            exists: false
        );

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.DownloadToStream(containerName: PublicReleaseFiles, path: path, new MemoryStream());

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetDownloadStream()
    {
        const string path = "path/to/test.csv";

        var blobClient = MockBlobClient(name: path);

        blobClient.SetupGetDownloadStreamAsync(content: "Test content");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.GetDownloadStream(containerName: PublicReleaseFiles, path: path, decompress: false);

        await using var stream = result.AssertRight();
        Assert.Equal("Test content", stream.ReadToEnd());
    }

    [Fact]
    public async Task GetBlobDownloadToken_Success()
    {
        const string filename = "test.pdf";
        const string path = "path/to/test.pdf";
        var container = PublicReleaseFiles;

        var tokenCreated = new BlobDownloadToken(
            Token: "a-token",
            ContainerName: container.Name,
            Path: path,
            Filename: filename,
            ContentType: MediaTypeNames.Text.Csv
        );

        var blobServiceClient = MockBlobServiceClient();

        var blobSasService = new Mock<IBlobSasService>(Strict);

        blobSasService
            .Setup(s =>
                s.CreateBlobDownloadToken(
                    blobServiceClient.Object,
                    PublicReleaseFiles,
                    filename,
                    path,
                    TimeSpan.FromMinutes(5),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(tokenCreated);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            blobSasService: blobSasService.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.GetBlobDownloadToken(
            container: PublicReleaseFiles,
            filename: filename,
            path: path,
            cancellationToken: CancellationToken.None
        );

        var tokenReturned = result.AssertRight();

        blobSasService.Verify();

        Assert.Same(tokenCreated, tokenReturned);
    }

    [Fact]
    public async Task StreamWithToken_Success()
    {
        var token = new BlobDownloadToken(
            Token: "a-token",
            ContainerName: "a-container",
            Path: "a-path",
            Filename: "a-filename.csv",
            ContentType: MediaTypeNames.Text.Csv
        );

        var secureClient = MockBlobClient(
            name: token.Path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        var blobServiceClient = MockBlobServiceClient();

        var blobSasService = new Mock<IBlobSasService>(Strict);

        blobSasService
            .Setup(s => s.CreateSecureBlobClient(blobServiceClient.Object, token))
            .ReturnsAsync(secureClient.Object);

        secureClient.SetupGetDownloadStreamAsync(content: "Test content");

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            blobSasService: blobSasService.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.StreamWithToken(token: token, cancellationToken: CancellationToken.None);

        var fileStreamResult = result.AssertRight();

        Assert.Equal("Test content", fileStreamResult.FileStream.ReadToEnd());
        Assert.Equal(token.Filename, fileStreamResult.FileDownloadName);
        Assert.Equal(token.ContentType, fileStreamResult.ContentType);
    }

    [Fact]
    public async Task GetDeserializedJson()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadContentAsync(content: @"{ ""Value"": ""test-value"" }");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.GetDeserializedJson<TestClass>(PublicReleaseFiles, path);

        result.AssertRight(new TestClass("test-value"));
    }

    [Fact]
    public async Task GetDeserializedJson_WithType()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadContentAsync(content: @"{ ""Value"": ""test-value"" }");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.GetDeserializedJson(PublicReleaseFiles, path, typeof(TestClass));

        result.AssertRight(new TestClass("test-value"));
    }

    [Fact]
    public async Task GetDeserializedJson_Null()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadContentAsync(content: "null");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var result = await service.GetDeserializedJson<object>(PublicReleaseFiles, path);

        result.AssertRight(expected: null);
    }

    [Fact]
    public async Task GetDeserializedJson_ThrowsOnEmptyJson()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            createdOn: DateTimeOffset.UtcNow,
            contentLength: 10 * 1024,
            contentType: MediaTypeNames.Application.Pdf,
            metadata: new Dictionary<string, string>()
        );

        blobClient.SetupDownloadContentAsync(content: "");

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatMustNotBeCalled()
        );

        var exception = await Assert.ThrowsAsync<JsonException>(() =>
            service.GetDeserializedJson(PublicReleaseFiles, path, typeof(TestClass))
        );

        Assert.Equal($"Found empty file when trying to deserialize JSON for path: {path}", exception.Message);
    }

    [Fact]
    public async Task DeleteBlobs()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        blobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem> { BlobItem("directory/item-1"), BlobItem("directory/item-2") },
            new List<BlobItem> { BlobItem("directory/item-3") }
        );

        blobContainerClient
            .Setup(s =>
                s.GetBlobsAsync(It.Is<GetBlobsOptions>(actual => actual.Prefix == "directory/"), CancellationToken.None)
            )
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        await service.DeleteBlobs(PublicReleaseFiles, "directory");

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Equal(3, deletedBlobs.Count);
        Assert.Equal("directory/item-1", deletedBlobs[0]);
        Assert.Equal("directory/item-2", deletedBlobs[1]);
        Assert.Equal("directory/item-3", deletedBlobs[2]);
    }

    [Fact]
    public async Task DeleteBlobs_NoDirectory()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem> { BlobItem("item-1"), BlobItem("directory/item-2") },
            new List<BlobItem> { BlobItem("nested/directory/item-3") }
        );

        blobContainerClient
            .Setup(s =>
                s.GetBlobsAsync(It.Is<GetBlobsOptions>(actual => actual.Prefix == null), CancellationToken.None)
            )
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(PublicReleaseFiles, directoryPath: null);

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Equal(3, deletedBlobs.Count);
        Assert.Equal("item-1", deletedBlobs[0]);
        Assert.Equal("directory/item-2", deletedBlobs[1]);
        Assert.Equal("nested/directory/item-3", deletedBlobs[2]);
    }

    [Fact]
    public async Task DeleteBlobs_FiltersOnExcludeRegex()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem> { BlobItem("item-1"), BlobItem("directory/item-2") },
            new List<BlobItem> { BlobItem("nested/directory/item-3") }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions { ExcludeRegex = new Regex("directory/") }
        );

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Single(deletedBlobs);
        Assert.Equal("item-1", deletedBlobs[0]);
    }

    [Fact]
    public async Task DeleteBlobs_FiltersOnIncludeRegex()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem> { BlobItem("item-1"), BlobItem("directory/item-2") },
            new List<BlobItem> { BlobItem("nested/directory/item-3") }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions { IncludeRegex = new Regex("item-3") }
        );

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Single(deletedBlobs);
        Assert.Equal("nested/directory/item-3", deletedBlobs[0]);
    }

    [Fact]
    public async Task DeleteBlobs_FilterPrioritisesExcludeRegex()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem> { BlobItem("item-1"), BlobItem("directory/item-2") },
            new List<BlobItem> { BlobItem("nested/directory/item-3") }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex("directory/"),
                ExcludeRegex = new Regex("item-2"),
            }
        );

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Single(deletedBlobs);
        Assert.Equal("nested/directory/item-3", deletedBlobs[0]);
    }

    [Fact]
    public async Task DeleteBlobs_NestedReleaseBlobs_IncludeRegex()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem>
            {
                BlobItem("publications/pupil-absence/releases/2020/data-blocks/item-1"),
                BlobItem("publications/pupil-absence/releases/2020/item-2"),
                BlobItem("publications/pupil-absence/item-3"),
                BlobItem("publications/item-4"),
            }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions { IncludeRegex = new Regex("^publications/.*/releases/.*/data-blocks") }
        );

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Single(deletedBlobs);
        Assert.Equal("publications/pupil-absence/releases/2020/data-blocks/item-1", deletedBlobs[0]);
    }

    [Fact]
    public async Task DeleteBlobs_NestedReleaseBlobs_ExcludeRegex()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem>
            {
                BlobItem("publications/pupil-absence/releases/2020/data-blocks/item-1"),
                BlobItem("publications/pupil-absence/releases/2020/item-2"),
                BlobItem("publications/pupil-absence/item-3"),
                BlobItem("publications/item-4"),
            }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(It.IsAny<GetBlobsOptions>(), CancellationToken.None))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(s => s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions { ExcludeRegex = new Regex("^publications/.*/releases/.*/data-blocks") }
        );

        MockUtils.VerifyAllMocks(blobContainerClient);

        Assert.Equal(3, deletedBlobs.Count);
        Assert.Equal("publications/pupil-absence/releases/2020/item-2", deletedBlobs[0]);
        Assert.Equal("publications/pupil-absence/item-3", deletedBlobs[1]);
        Assert.Equal("publications/item-4", deletedBlobs[2]);
    }

    [Fact]
    public async Task MoveBlob_SourceBlobNotFound_ReturnsFalse()
    {
        // Arrange
        const string sourcePath = "path/to/test.pdf";
        const string destinationPath = "new/path/to/test.pdf";

        var sourceBlobClient = MockBlobClient(name: sourcePath, exists: false);

        var sourceBlobContainerClient = MockBlobContainerClient(PrivateReleaseTempFiles.Name, sourceBlobClient);

        sourceBlobContainerClient.Setup(client => client.GetBlobClient(sourcePath)).Returns(sourceBlobClient.Object);

        var blobServiceClient = MockBlobServiceClient(sourceBlobContainerClient);

        var service = SetupTestBlobStorageService(blobServiceClient.Object);

        // Act
        var result = await service.MoveBlob(PrivateReleaseTempFiles, sourcePath, destinationPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MoveBlob_DestinationBlobAlreadyExists_ReturnsFalse()
    {
        // Arrange
        const string sourcePath = "path/to/test.pdf";
        const string destinationPath = "new/path/to/test.pdf";

        var sourceBlobClient = MockBlobClient(name: sourcePath, exists: true);
        var destinationBlobClient = MockBlobClient(name: destinationPath, exists: true);

        var blobContainerClient = MockBlobContainerClient(
            PrivateReleaseTempFiles.Name,
            sourceBlobClient,
            destinationBlobClient
        );

        blobContainerClient.Setup(client => client.GetBlobClient(sourcePath)).Returns(sourceBlobClient.Object);

        blobContainerClient
            .Setup(client => client.GetBlobClient(destinationPath))
            .Returns(destinationBlobClient.Object);

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(blobServiceClient.Object);

        // Act
        var result = await service.MoveBlob(PrivateReleaseTempFiles, sourcePath, destinationPath);

        // Assert
        Assert.False(result);
    }

    [Theory]
    // Connects with the app's managed identity, as there's no key.
    [InlineData("AccountName=testaccount;")]
    [InlineData(
        "DefaultEndpointsProtocol=https;AccountName=testaccount;AccountKey=dGVzdGtleQ==;EndpointSuffix=core.windows.net"
    )]
    [InlineData("UseDevelopmentStorage=true")]
    public void Constructor_AcceptsKeylessAndKeyedConnectionStrings(string connectionString)
    {
        var exception = Record.Exception(() =>
            new PrivateBlobStorageService(
                connectionString,
                Mock.Of<ILogger<IBlobStorageService>>(),
                Mock.Of<IBlobSasService>(Strict)
            )
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task UploadFile()
    {
        const string path = "path/to/test.csv";

        var blobClient = MockBlobClient(name: path);

        string? uploadedContent = null;
        BlobHttpHeaders? uploadedHeaders = null;

        blobClient
            .Setup(client =>
                client.UploadAsync(
                    It.IsAny<string>(),
                    It.IsAny<BlobHttpHeaders>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<IProgress<long>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<StorageTransferOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback(
                (
                    string filePath,
                    BlobHttpHeaders headers,
                    IDictionary<string, string> _,
                    BlobRequestConditions _,
                    IProgress<long> _,
                    AccessTier? _,
                    StorageTransferOptions _,
                    CancellationToken _
                ) =>
                {
                    uploadedContent = File.ReadAllText(filePath);
                    uploadedHeaders = headers;
                }
            )
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        blobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        var content = "a,b,c"u8.ToArray();
        var file = new FormFile(new MemoryStream(content), 0, content.Length, "file", "test.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = MediaTypeNames.Text.Csv,
        };

        await service.UploadFile(PublicReleaseFiles, path, file);

        blobContainerClient.VerifyCreateIfNotExistsAsync();
        Assert.Equal("a,b,c", uploadedContent);
        Assert.Equal(MediaTypeNames.Text.Csv, uploadedHeaders?.ContentType);
    }

    [Fact]
    public async Task UploadStream()
    {
        const string path = "path/to/test.csv";

        var blobClient = MockBlobClient(name: path);

        var upload = blobClient.SetupUploadStreamAsync();

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        blobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        // Not at the start of the stream, which the upload should start from regardless.
        var sourceStream = new MemoryStream("a,b,c"u8.ToArray());
        sourceStream.Seek(0, SeekOrigin.End);

        await service.UploadStream(PublicReleaseFiles, path, sourceStream, MediaTypeNames.Text.Csv);

        blobContainerClient.VerifyCreateIfNotExistsAsync();
        Assert.Equal("a,b,c", upload.Content);
        Assert.Equal(MediaTypeNames.Text.Csv, upload.Headers?.ContentType);
        Assert.Null(upload.Headers?.ContentEncoding);
    }

    [Fact]
    public async Task UploadStream_Compressed()
    {
        const string path = "path/to/test.csv";

        var blobClient = MockBlobClient(name: path);

        // The service writes the compressed content through this stream, which it disposes when done.
        var blobStream = new MemoryStream();
        BlobOpenWriteOptions? openWriteOptions = null;

        blobClient
            .Setup(client =>
                client.OpenWriteAsync(true, It.IsAny<BlobOpenWriteOptions>(), It.IsAny<CancellationToken>())
            )
            .Callback((bool _, BlobOpenWriteOptions options, CancellationToken _) => openWriteOptions = options)
            .ReturnsAsync(blobStream);

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        blobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        await service.UploadStream(
            PublicReleaseFiles,
            path,
            new MemoryStream("a,b,c"u8.ToArray()),
            MediaTypeNames.Text.Csv,
            contentEncoding: ContentEncodings.Gzip
        );

        blobContainerClient.VerifyCreateIfNotExistsAsync();
        Assert.Equal(MediaTypeNames.Text.Csv, openWriteOptions?.HttpHeaders.ContentType);
        Assert.Equal(ContentEncodings.Gzip, openWriteOptions?.HttpHeaders.ContentEncoding);

        await using var decompressed = new GZipStream(
            new MemoryStream(blobStream.ToArray()),
            CompressionMode.Decompress
        );
        Assert.Equal("a,b,c", await new StreamReader(decompressed).ReadToEndAsync());
    }

    [Fact]
    public async Task UploadAsJson()
    {
        const string path = "path/to/test.json";

        var blobClient = MockBlobClient(name: path);

        var upload = blobClient.SetupUploadStreamAsync();

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        blobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        await service.UploadAsJson(PublicReleaseFiles, path, new TestClass("test value"));

        blobContainerClient.VerifyCreateIfNotExistsAsync();
        Assert.Equal("{\"Value\":\"test value\"}", upload.Content);
        Assert.Equal(MediaTypeNames.Application.Json, upload.Headers?.ContentType);
    }

    [Fact]
    public async Task DeleteBlob()
    {
        const string path = "path/to/test.csv";

        var blobClient = MockBlobClient(name: path);

        blobClient
            .Setup(client =>
                client.DeleteIfExistsAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        blobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        await service.DeleteBlob(PublicReleaseFiles, path);

        blobContainerClient.VerifyCreateIfNotExistsAsync();
        blobClient.Verify(
            client =>
                client.DeleteIfExistsAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task MoveBlob_ToAnotherContainer()
    {
        const string sourcePath = "path/to/test.pdf";
        const string destinationPath = "new/path/to/test.pdf";
        var sourceUri = new Uri(
            $"https://testaccount.blob.core.windows.net/{PrivateReleaseTempFiles.Name}/{sourcePath}"
        );

        var sourceBlobClient = MockBlobClient(name: sourcePath, exists: true);
        sourceBlobClient.SetupGet(client => client.Uri).Returns(sourceUri);
        sourceBlobClient
            .Setup(client =>
                client.DeleteAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Mock.Of<Response>());

        // The source blob is leased while it's copied.
        var lease = new Mock<BlobLeaseClient>(Strict);
        lease
            .Setup(client =>
                client.AcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Mock.Of<Response<Azure.Storage.Blobs.Models.BlobLease>>());
        lease
            .Setup(client => client.ReleaseAsync(It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<ReleasedObjectInfo>>());
        sourceBlobClient
            .Protected()
            .Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
            .Returns(lease.Object);

        var destinationBlobClient = MockBlobClient(name: destinationPath, exists: false);
        destinationBlobClient
            .Setup(client =>
                client.StartCopyFromUriAsync(
                    sourceUri,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<RehydratePriority?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Mock.Of<CopyFromUriOperation>());
        destinationBlobClient
            .Setup(client =>
                client.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Response.FromValue(BlobProperties(blobCopyStatus: CopyStatus.Success), null!));

        var sourceBlobContainerClient = MockBlobContainerClient(PrivateReleaseTempFiles.Name, sourceBlobClient);
        sourceBlobContainerClient.SetupCreateIfNotExistsAsync();

        var destinationBlobContainerClient = MockBlobContainerClient(PrivateReleaseFiles.Name, destinationBlobClient);
        destinationBlobContainerClient.SetupCreateIfNotExistsAsync();

        var blobServiceClient = MockBlobServiceClient(sourceBlobContainerClient, destinationBlobContainerClient);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            storageInstanceCreationUtil: StorageInstanceCreationUtilThatCreates()
        );

        var moved = await service.MoveBlob(
            sourceContainer: PrivateReleaseTempFiles,
            sourcePath: sourcePath,
            destinationPath: destinationPath,
            destinationContainer: PrivateReleaseFiles
        );

        Assert.True(moved);
        sourceBlobContainerClient.VerifyCreateIfNotExistsAsync();
        destinationBlobContainerClient.VerifyCreateIfNotExistsAsync();
        MockUtils.VerifyAllMocks(lease);
        sourceBlobClient.Verify(
            client =>
                client.DeleteAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    // Runs the container creation it's given, as StorageInstanceCreationUtil does the first time it sees a container.
    private static IStorageInstanceCreationUtil StorageInstanceCreationUtilThatCreates()
    {
        var storageInstanceCreationUtil = new Mock<IStorageInstanceCreationUtil>(Strict);

        storageInstanceCreationUtil
            .Setup(util =>
                util.CreateInstanceIfNotExistsAsync(
                    It.IsAny<string>(),
                    AzureStorageType.Blob,
                    It.IsAny<string>(),
                    It.IsAny<Func<Task>>()
                )
            )
            .Returns((string _, AzureStorageType _, string _, Func<Task> createIfNotExists) => createIfNotExists());

        return storageInstanceCreationUtil.Object;
    }

    // Strict, so a read test fails if the service tries to create a container. Reads mustn't, as creating a container
    // needs write access, which read-only identities don't have.
    private static IStorageInstanceCreationUtil StorageInstanceCreationUtilThatMustNotBeCalled() =>
        Mock.Of<IStorageInstanceCreationUtil>(Strict);

    private static TestBlobStorageService SetupTestBlobStorageService(
        BlobServiceClient? blobServiceClient = null,
        IBlobSasService? blobSasService = null,
        IStorageInstanceCreationUtil? storageInstanceCreationUtil = null
    )
    {
        return new TestBlobStorageService(
            connectionString: "",
            blobServiceClient ?? Mock.Of<BlobServiceClient>(),
            Mock.Of<ILogger<BlobStorageService>>(),
            storageInstanceCreationUtil ?? Mock.Of<IStorageInstanceCreationUtil>(),
            blobSasService ?? Mock.Of<IBlobSasService>(Strict)
        );
    }

    private class TestBlobStorageService(
        string connectionString,
        BlobServiceClient client,
        ILogger<IBlobStorageService> logger,
        IStorageInstanceCreationUtil storageInstanceCreationUtil,
        IBlobSasService blobSasService
    ) : BlobStorageService(connectionString, client, logger, storageInstanceCreationUtil, blobSasService);

    private IEnumerable<Page<T>> CreatePages<T>(params List<T>[] pages)
    {
        return pages.Select(page => Page<T>.FromValues(ImmutableList.Create(page.ToArray()), "", null!));
    }

    private class TestAsyncPageable<T> : AsyncPageable<T>
        where T : notnull
    {
        private readonly IEnumerable<Page<T>> _pages;

        public TestAsyncPageable(IEnumerable<Page<T>> pages)
        {
            _pages = pages;
        }

        public override async IAsyncEnumerable<Page<T>> AsPages(
            string? continuationToken = null,
            int? pageSizeHint = null
        )
        {
            foreach (var page in _pages)
            {
                yield return page;
            }

            await Task.CompletedTask;
        }
    }
}
