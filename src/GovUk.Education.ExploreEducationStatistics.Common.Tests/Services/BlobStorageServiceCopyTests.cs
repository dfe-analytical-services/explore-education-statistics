using System.Net;
using System.Net.Sockets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

/// <summary>
/// Tests the server-side copies made by <see cref="BlobStorageService"/> against Azurite, as their behaviour depends on
/// the storage service rather than on calls that can be mocked.
/// </summary>
public class BlobStorageServiceCopyTests(BlobStorageServiceCopyTests.AzuriteFixture fixture)
    : IClassFixture<BlobStorageServiceCopyTests.AzuriteFixture>
{
    // Each test copies under its own directory, so the tests can share one Azurite container.
    private readonly string _directory = Guid.NewGuid().ToString();

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    public async Task CopyDirectory_CopiesEveryBlobInTheDirectory(string trailingSlash)
    {
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/data/file.csv", "data", "text/csv");
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/meta/nested/file.csv", "meta", "text/csv");
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/sourcesibling/file.csv", "not copied", "text/csv");

        var service = BuildService();

        var copied = await service.CopyDirectory(
            sourceContainerName: PrivateReleaseFiles,
            sourceDirectoryPath: $"{_directory}/source{trailingSlash}",
            destinationContainerName: PublicReleaseFiles,
            destinationDirectoryPath: $"{_directory}/destination{trailingSlash}"
        );

        Assert.Equal(
            [$"{_directory}/destination/data/file.csv", $"{_directory}/destination/meta/nested/file.csv"],
            copied.Select(blob => blob.Path).Order()
        );
        Assert.All(copied, blob => Assert.Equal("text/csv", blob.ContentType));

        Assert.Equal(
            [$"{_directory}/destination/data/file.csv", $"{_directory}/destination/meta/nested/file.csv"],
            await ListBlobs(PublicReleaseFiles, _directory)
        );

        var copiedBlob = GetBlob(PublicReleaseFiles, $"{_directory}/destination/data/file.csv");
        BlobProperties properties = await copiedBlob.GetPropertiesAsync();

        Assert.Equal("data", await ReadBlob(copiedBlob));
        Assert.Equal("text/csv", properties.ContentType);
        Assert.Equal("value", properties.Metadata["key"]);
    }

    [Fact]
    public async Task CopyDirectory_ShouldCopyBlob_CopiesOnlyChosenBlobs()
    {
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/wanted.csv", "wanted", "text/csv");
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/unwanted.csv", "unwanted", "text/csv");

        var service = BuildService();

        var copied = await service.CopyDirectory(
            sourceContainerName: PrivateReleaseFiles,
            sourceDirectoryPath: $"{_directory}/source/",
            destinationContainerName: PublicReleaseFiles,
            destinationDirectoryPath: $"{_directory}/destination/",
            options: new IBlobStorageService.CopyDirectoryOptions
            {
                ShouldCopyBlob = path => path == $"{_directory}/source/wanted.csv",
            }
        );

        Assert.Equal([$"{_directory}/destination/wanted.csv"], copied.Select(blob => blob.Path));
        Assert.Equal([$"{_directory}/destination/wanted.csv"], await ListBlobs(PublicReleaseFiles, _directory));
    }

    [Fact]
    public async Task CopyDirectory_ExistingDestinationBlob_IsSkipped()
    {
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/existing.csv", "new", "text/csv");
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/new.csv", "new", "text/csv");
        await UploadBlob(PublicReleaseFiles, $"{_directory}/destination/existing.csv", "old", "text/csv");

        var service = BuildService();

        var copied = await service.CopyDirectory(
            sourceContainerName: PrivateReleaseFiles,
            sourceDirectoryPath: $"{_directory}/source/",
            destinationContainerName: PublicReleaseFiles,
            destinationDirectoryPath: $"{_directory}/destination/"
        );

        Assert.Equal([$"{_directory}/destination/new.csv"], copied.Select(blob => blob.Path));
        Assert.Equal("old", await ReadBlob(GetBlob(PublicReleaseFiles, $"{_directory}/destination/existing.csv")));
    }

    [Fact]
    public async Task CopyDirectory_DestinationConnectionString_CopiesWithThatConnection()
    {
        await UploadBlob(PrivateMethodologyFiles, $"{_directory}/image.png", "image", "image/png");

        var service = BuildService();

        var copied = await service.CopyDirectory(
            sourceContainerName: PrivateMethodologyFiles,
            sourceDirectoryPath: $"{_directory}/",
            destinationContainerName: PublicMethodologyFiles,
            destinationDirectoryPath: $"{_directory}/",
            options: new IBlobStorageService.CopyDirectoryOptions
            {
                DestinationConnectionString = fixture.ConnectionString,
            }
        );

        Assert.Equal([$"{_directory}/image.png"], copied.Select(blob => blob.Path));
        Assert.Equal("image", await ReadBlob(GetBlob(PublicMethodologyFiles, $"{_directory}/image.png")));
    }

    [Fact]
    public async Task MoveDirectory_OverwritesDestinationAndDeletesSource()
    {
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/existing.csv", "new", "text/csv");
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/source/new.csv", "new", "text/csv");
        await UploadBlob(PrivateReleaseFiles, $"{_directory}/destination/existing.csv", "old", "text/csv");

        var service = BuildService();

        await service.MoveDirectory(
            sourceContainerName: PrivateReleaseFiles,
            sourceDirectoryPath: $"{_directory}/source",
            destinationContainerName: PrivateReleaseFiles,
            destinationDirectoryPath: $"{_directory}/destination"
        );

        Assert.Equal(
            [$"{_directory}/destination/existing.csv", $"{_directory}/destination/new.csv"],
            await ListBlobs(PrivateReleaseFiles, _directory)
        );
        Assert.Equal("new", await ReadBlob(GetBlob(PrivateReleaseFiles, $"{_directory}/destination/existing.csv")));
    }

    [Fact]
    public async Task MoveBlob_MovesBlob()
    {
        await UploadBlob(PrivateReleaseTempFiles, $"{_directory}/upload.csv", "data", "text/csv");

        var service = BuildService();

        var moved = await service.MoveBlob(
            sourceContainer: PrivateReleaseTempFiles,
            sourcePath: $"{_directory}/upload.csv",
            destinationPath: $"{_directory}/data/upload.csv",
            destinationContainer: PrivateReleaseFiles
        );

        Assert.True(moved);
        Assert.Empty(await ListBlobs(PrivateReleaseTempFiles, _directory));
        Assert.Equal("data", await ReadBlob(GetBlob(PrivateReleaseFiles, $"{_directory}/data/upload.csv")));
    }

    private PrivateBlobStorageService BuildService()
    {
        return new PrivateBlobStorageService(
            connectionString: fixture.ConnectionString,
            logger: Mock.Of<ILogger<IBlobStorageService>>(),
            sasService: new BlobSasService(new DateTimeProvider(), Mock.Of<ILogger<BlobSasService>>())
        );
    }

    private BlobClient GetBlob(IBlobContainer container, string path)
    {
        return new BlobServiceClient(fixture.ConnectionString)
            .GetBlobContainerClient(container.EmulatedName)
            .GetBlobClient(path);
    }

    private async Task UploadBlob(IBlobContainer container, string path, string content, string contentType)
    {
        var containerClient = new BlobServiceClient(fixture.ConnectionString).GetBlobContainerClient(
            container.EmulatedName
        );
        await containerClient.CreateIfNotExistsAsync();

        await containerClient
            .GetBlobClient(path)
            .UploadAsync(
                BinaryData.FromString(content),
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                    Metadata = new Dictionary<string, string> { ["key"] = "value" },
                }
            );
    }

    private async Task<List<string>> ListBlobs(IBlobContainer container, string prefix)
    {
        var containerClient = new BlobServiceClient(fixture.ConnectionString).GetBlobContainerClient(
            container.EmulatedName
        );
        await containerClient.CreateIfNotExistsAsync();

        return await containerClient
            .GetBlobsAsync(new GetBlobsOptions { Prefix = prefix })
            .Select(blob => blob.Name)
            .Order()
            .ToListAsync();
    }

    private static async Task<string> ReadBlob(BlobClient blob)
    {
        var content = await blob.DownloadContentAsync();
        return content.Value.Content.ToString();
    }

    public class AzuriteFixture : IAsyncLifetime
    {
        // Azurite reads a SAS-authorised copy source by requesting its URL itself, so its blob service has to listen
        // inside the container on the same port that the URL, built outside it, uses. Testcontainers' Azurite module
        // maps a random host port instead, so the container is set up here directly.
        private static readonly int BlobPort = GetFreePort();

        private readonly IContainer _azurite = new ContainerBuilder("mcr.microsoft.com/azure-storage/azurite:3.35.0")
            .WithCommand("azurite-blob", "--blobHost", "0.0.0.0", "--blobPort", $"{BlobPort}", "--skipApiVersionCheck")
            .WithPortBinding(BlobPort, BlobPort)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(BlobPort))
            .Build();

        // The well-known Azurite development account and key.
        public string ConnectionString =>
            "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;"
            + "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;"
            + $"BlobEndpoint=http://127.0.0.1:{BlobPort}/devstoreaccount1;";

        public Task InitializeAsync() => _azurite.StartAsync();

        public Task DisposeAsync() => _azurite.DisposeAsync().AsTask();

        private static int GetFreePort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
    }
}
