#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static Azure.Storage.Blobs.Models.BlobsModelFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;
using static Moq.MockBehavior;
using Capture = Moq.Capture;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class BlobStorageServiceTests
    {
        private record TestClass(string Value);

        [Fact]
        public async Task CheckBlobExists_BlobExists()
        {
            var blobClient = MockBlobClient(
                name: "path/to/test.png",
                exists: true);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            Assert.True(await service.CheckBlobExists(PublicReleaseFiles, "path/to/test.png"));
        }

        [Fact]
        public async Task CheckBlobExists_BlobDoesNotExist()
        {
            var blobClient = MockBlobClient(
                name: "path/to/test.png",
                exists: false);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            Assert.False(await service.CheckBlobExists(PublicReleaseFiles, "path/to/test.png"));
        }

        [Fact]
        public async Task GetBlob()
        {
            var createdOn = DateTimeOffset.UtcNow;

            var blobClient = MockBlobClient(
                name: "path/to/test.pdf",
                createdOn: createdOn,
                contentLength: 10 * 1024,
                contentType: "application/pdf",
                metadata: new Dictionary<string, string>()
            );

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            var result = await service.GetBlob(PublicReleaseFiles, "path/to/test.pdf");

            Assert.Equal(createdOn, result.Created);
            Assert.Equal(10 * 1024, result.ContentLength);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal("test.pdf", result.FileName);
            Assert.Equal(new Dictionary<string, string>(), result.Meta);
            Assert.Equal("path/to/test.pdf", result.Path);
        }

        [Fact]
        public async Task GetDeserializedJson()
        {
            var createdOn = DateTimeOffset.UtcNow;

            var blobClient = MockBlobClient(
                name: "path/to/test.pdf",
                createdOn: createdOn,
                contentLength: 10 * 1024,
                contentType: "application/pdf",
                metadata: new Dictionary<string, string>()
            );

            var json = @"{ ""Value"": ""test-value"" }";

            var response = new Mock<Response<BlobDownloadResult>>(Strict);

            response.SetupGet(r => r.Value)
                .Returns(BlobDownloadResult(BinaryData.FromString(json)));

            blobClient.Setup(s => s.DownloadContentAsync(default))
                .ReturnsAsync(response.Object);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            var result = await service.GetDeserializedJson<TestClass>(PublicReleaseFiles, "path/to/test.pdf");

            var expected = new TestClass("test-value");

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetDeserializedJson_WithType()
        {
            var createdOn = DateTimeOffset.UtcNow;

            var blobClient = MockBlobClient(
                name: "path/to/test.pdf",
                createdOn: createdOn,
                contentLength: 10 * 1024,
                contentType: "application/pdf",
                metadata: new Dictionary<string, string>()
            );

            var json = @"{ ""Value"": ""test-value"" }";

            var response = new Mock<Response<BlobDownloadResult>>(Strict);

            response.SetupGet(r => r.Value)
                .Returns(BlobDownloadResult(BinaryData.FromString(json)));

            blobClient.Setup(s => s.DownloadContentAsync(default))
                .ReturnsAsync(response.Object);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            var result = await service.GetDeserializedJson(PublicReleaseFiles, "path/to/test.pdf", typeof(TestClass));

            var typedResult = Assert.IsType<TestClass>(result);
            var expected = new TestClass("test-value");

            Assert.Equal(expected, typedResult);
        }

        [Fact]
        public async Task GetDeserializedJson_Null()
        {
            var createdOn = DateTimeOffset.UtcNow;

            var blobClient = MockBlobClient(
                name: "path/to/test.pdf",
                createdOn: createdOn,
                contentLength: 10 * 1024,
                contentType: "application/pdf",
                metadata: new Dictionary<string, string>()
            );

            var json = "null";

            var response = new Mock<Response<BlobDownloadResult>>(Strict);

            response.SetupGet(r => r.Value)
                .Returns(BlobDownloadResult(BinaryData.FromString(json)));

            blobClient.Setup(s => s.DownloadContentAsync(default))
                .ReturnsAsync(response.Object);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            var result = await service.GetDeserializedJson<object>(PublicReleaseFiles, "path/to/test.pdf");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDeserializedJson_ThrowsOnEmptyJson()
        {
            var createdOn = DateTimeOffset.UtcNow;
            var path = "path/to/test.pdf";

            var blobClient = MockBlobClient(
                name: path,
                createdOn: createdOn,
                contentLength: 10 * 1024,
                contentType: "application/pdf",
                metadata: new Dictionary<string, string>()
            );


            var response = new Mock<Response<BlobDownloadResult>>(Strict);

            response.SetupGet(r => r.Value)
                .Returns(BlobDownloadResult(BinaryData.FromString("")));

            blobClient.Setup(s => s.DownloadContentAsync(default))
                .ReturnsAsync(response.Object);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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
        private async Task DeleteBlobs_FilterPrioritisesExcludeRegex()
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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

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


        private static Mock<BlobServiceClient> MockBlobServiceClient(
            params Mock<BlobContainerClient>[] blobContainerClients)
        {
            var blobServiceClient = new Mock<BlobServiceClient>(Strict);

            blobServiceClient.Setup(s => s.Uri)
                .Returns(new Uri("https://data-storage:10001/devstoreaccount1;"));

            foreach (var blobContainerClient in blobContainerClients)
            {
                blobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerClient.Object.Name))
                    .Returns(blobContainerClient.Object);
            }

            return blobServiceClient;
        }

        private static Mock<BlobContainerClient> MockBlobContainerClient(
            string containerName,
            params Mock<BlobClient>[] blobClients)
        {
            var blobContainerClient = new Mock<BlobContainerClient>(Strict);

            blobContainerClient
                .SetupGet(client => client.Name)
                .Returns(containerName);

            foreach (var blobClient in blobClients)
            {
                blobContainerClient.Setup(client => client.GetBlobClient(blobClient.Object.Name))
                    .Returns(blobClient.Object);
            }

            return blobContainerClient;
        }

        private static Mock<BlobClient> MockBlobClient(
            string name,
            DateTimeOffset createdOn = default,
            int contentLength = 0,
            string? contentType = null,
            IDictionary<string, string>? metadata = null,
            bool exists = true)
        {
            var blobClient = new Mock<BlobClient>(Strict);

            blobClient.SetupGet(client => client.Name)
                .Returns(name);

            blobClient.Setup(client => client.ExistsAsync(default))
                .ReturnsAsync(Response.FromValue(exists, null!));

            var blobProperties = BlobProperties(
                createdOn: createdOn,
                contentLength: contentLength,
                contentType: contentType,
                metadata: metadata
            );

            blobClient.Setup(client => client.GetPropertiesAsync(default, default))
                .ReturnsAsync(Response.FromValue(blobProperties, null!));

            return blobClient;
        }

        private static BlobStorageService SetupBlobStorageService(
            BlobServiceClient? blobServiceClient = null)
        {
            return new BlobStorageService(
                connectionString: "",
                blobServiceClient ?? new Mock<BlobServiceClient>().Object,
                Mock.Of<ILogger<BlobStorageService>>(),
                Mock.Of<IStorageInstanceCreationUtil>()
            );
        }

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
}
