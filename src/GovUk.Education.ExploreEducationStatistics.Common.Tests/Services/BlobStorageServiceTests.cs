#nullable enable
using System.Collections.Immutable;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
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

        var blobClient = MockBlobClient(
            name: path,
            exists: true);

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(blobServiceClient.Object);

        Assert.True(await service.CheckBlobExists(PublicReleaseFiles, path));
    }

    [Fact]
    public async Task CheckBlobExists_BlobDoesNotExist()
    {
        const string path = "path/to/test.pdf";

        var blobClient = MockBlobClient(
            name: path,
            exists: false);

        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        Assert.False(await service.CheckBlobExists(PublicReleaseFiles, path));
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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        var result = await service.DownloadBlobText(
            containerName: PublicReleaseFiles,
            path: path);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        var result = await service.DownloadBlobText(
            containerName: PublicReleaseFiles,
            path: path);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await using var targetStream = new MemoryStream();
        await service.DownloadToStream(
            containerName: PublicReleaseFiles,
            path: path,
            targetStream: targetStream);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        var result = await service.DownloadToStream(
            containerName: PublicReleaseFiles,
            path: path,
            new MemoryStream());

        result.AssertNotFound();
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
            ContentType: MediaTypeNames.Text.Csv);

        var blobServiceClient = MockBlobServiceClient();

        var blobSasService = new Mock<IBlobSasService>(Strict);

        blobSasService
            .Setup(s => s.CreateBlobDownloadToken(
                blobServiceClient.Object,
                PublicReleaseFiles,
                filename,
                path,
                TimeSpan.FromMinutes(5),
                default))
            .ReturnsAsync(tokenCreated);

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            blobSasService: blobSasService.Object);

        var result = await service.GetBlobDownloadToken(
            container: PublicReleaseFiles,
            filename: filename,
            path: path,
            cancellationToken: default);

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
            ContentType: MediaTypeNames.Text.Csv);

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
            .Setup(s => s.CreateSecureBlobClient(
                blobServiceClient.Object,
                token))
            .ReturnsAsync(secureClient.Object);

        secureClient.SetupGetDownloadStreamAsync(content: "Test content");

        var service = SetupTestBlobStorageService(
            blobServiceClient: blobServiceClient.Object,
            blobSasService: blobSasService.Object);

        var result = await service.StreamWithToken(
            token: token,
            cancellationToken: default);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        var exception = await Assert.ThrowsAsync<JsonException>(
            () =>
                service.GetDeserializedJson(PublicReleaseFiles, path, typeof(TestClass))
        );

        Assert.Equal($"Found empty file when trying to deserialize JSON for path: {path}", exception.Message);
    }

    [Fact]
    public async Task DeleteBlobs()
    {
        var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);

        var pages = CreatePages(
            new List<BlobItem>
            {
                BlobItem("directory/item-1"),
                BlobItem("directory/item-2"),
            },
            new List<BlobItem>
            {
                BlobItem("directory/item-3"),
            }
        );

        blobContainerClient
            .Setup(
                s =>
                    s.GetBlobsAsync(default, default, "directory/", default)
            )
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

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
            new List<BlobItem>
            {
                BlobItem("item-1"),
                BlobItem("directory/item-2"),
            },
            new List<BlobItem>
            {
                BlobItem("nested/directory/item-3"),
            }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(default, default, default, default))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(PublicReleaseFiles);

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
            new List<BlobItem>
            {
                BlobItem("item-1"),
                BlobItem("directory/item-2"),
            },
            new List<BlobItem>
            {
                BlobItem("nested/directory/item-3"),
            }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(default, default, default, default))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions
            {
                ExcludeRegex = new Regex("directory/")
            }
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
            new List<BlobItem>
            {
                BlobItem("item-1"),
                BlobItem("directory/item-2"),
            },
            new List<BlobItem>
            {
                BlobItem("nested/directory/item-3"),
            }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(default, default, default, default))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex("item-3")
            }
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
            new List<BlobItem>
            {
                BlobItem("item-1"),
                BlobItem("directory/item-2"),
            },
            new List<BlobItem>
            {
                BlobItem("nested/directory/item-3"),
            }
        );

        blobContainerClient
            .Setup(s => s.GetBlobsAsync(default, default, default, default))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex("directory/"),
                ExcludeRegex = new Regex("item-2")
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
            .Setup(s => s.GetBlobsAsync(default, default, default, default))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex("^publications/.*/releases/.*/data-blocks")
            }
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
            .Setup(s => s.GetBlobsAsync(default, default, default, default))
            .Returns(new TestAsyncPageable<BlobItem>(pages));

        var deletedBlobs = new List<string>();

        blobContainerClient
            .Setup(
                s =>
                    s.DeleteBlobIfExistsAsync(Capture.In(deletedBlobs), default, default, default)
            )
            .ReturnsAsync(Response.FromValue(true, null!));

        var service = SetupTestBlobStorageService(blobServiceClient: blobServiceClient.Object);

        await service.DeleteBlobs(
            PublicReleaseFiles,
            options: new DeleteBlobsOptions
            {
                ExcludeRegex = new Regex("^publications/.*/releases/.*/data-blocks")
            }
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

        sourceBlobContainerClient
            .Setup(client => client.GetBlobClient(sourcePath))
            .Returns(sourceBlobClient.Object);

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

        var blobContainerClient = MockBlobContainerClient(PrivateReleaseTempFiles.Name, sourceBlobClient, destinationBlobClient);

        blobContainerClient
            .Setup(client => client.GetBlobClient(sourcePath))
            .Returns(sourceBlobClient.Object);

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

    private static TestBlobStorageService SetupTestBlobStorageService(
        BlobServiceClient? blobServiceClient = null,
        IBlobSasService? blobSasService = null)
    {
        return new TestBlobStorageService(
            connectionString: "",
            blobServiceClient ?? Mock.Of<BlobServiceClient>(),
            Mock.Of<ILogger<BlobStorageService>>(),
            Mock.Of<IStorageInstanceCreationUtil>(),
            blobSasService ?? Mock.Of<IBlobSasService>(Strict));
    }

    private class TestBlobStorageService(
        string connectionString,
        BlobServiceClient client,
        ILogger<IBlobStorageService> logger,
        IStorageInstanceCreationUtil storageInstanceCreationUtil,
        IBlobSasService blobSasService)
        : BlobStorageService(
            connectionString,
            client,
            logger,
            storageInstanceCreationUtil,
            blobSasService);

    private IEnumerable<Page<T>> CreatePages<T>(params List<T>[] pages)
    {
        return pages.Select(
            page => Page<T>.FromValues(
                ImmutableList.Create(page.ToArray()), "", null!
            )
        );
    }

    private class TestAsyncPageable<T> : AsyncPageable<T> where T : notnull
    {
        private readonly IEnumerable<Page<T>> _pages;

        public TestAsyncPageable(IEnumerable<Page<T>> pages)
        {
            _pages = pages;
        }

        public override async IAsyncEnumerable<Page<T>> AsPages(
            string? continuationToken = null,
            int? pageSizeHint = null)
        {
            foreach (var page in _pages)
            {
                yield return page;
            }

            await Task.CompletedTask;
        }
    }
}
