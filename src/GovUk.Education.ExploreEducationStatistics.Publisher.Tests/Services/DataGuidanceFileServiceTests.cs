#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class DataGuidanceFileServiceTests : IDisposable
    {
        private readonly List<FileStream> _files = new List<FileStream>();

        public void Dispose()
        {
            _files.ForEach(
                file =>
                {
                    file.Dispose();
                    File.Delete(file.Name);
                }
            );
        }

        [Fact]
        public async Task CreateDataGuidanceFile()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var mockFile = CreateMockFile();

                var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>();

                dataGuidanceFileWriter
                    .Setup(s => s.WriteFile(release.Id, It.IsAny<string>()))
                    .ReturnsAsync(mockFile);

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(
                        s => s.UploadStream(
                            PrivateReleaseFiles,
                            It.Is<string>(path => path.StartsWith($"{release.Id}/data-guidance")),
                            mockFile,
                            "text/plain",
                            null
                        )
                    );

                var service = BuildDataGuidanceFileService(
                    contentDbContext: contentDbContext,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object,
                    blobStorageService: blobStorageService.Object
                );

                var file = await service.CreateDataGuidanceFile(release.Id);

                Assert.Equal(FileType.DataGuidance, file.Type);
                Assert.Equal(release.Id, file.RootPath);
                Assert.Equal("data-guidance.txt", file.Filename);

                MockUtils.VerifyAllMocks(dataGuidanceFileWriter, blobStorageService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var file = contentDbContext.Files.First();

                Assert.Equal(FileType.DataGuidance, file.Type);
                Assert.Equal(release.Id, file.RootPath);
                Assert.Equal("data-guidance.txt", file.Filename);
            }
        }

        [Fact]
        public async Task CreateDataGuidanceFile_WriteThrows()
        {
            var releaseId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>();

                dataGuidanceFileWriter
                    .Setup(s => s.WriteFile(releaseId, It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Write to file failed"));

                var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

                var service = BuildDataGuidanceFileService(
                    contentDbContext: contentDbContext,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object,
                    blobStorageService: blobStorageService.Object
                );

                var exception = await Assert.ThrowsAsync<Exception>(
                    async () => { await service.CreateDataGuidanceFile(releaseId); }
                );

                Assert.Equal("Write to file failed", exception.Message);

                MockUtils.VerifyAllMocks(dataGuidanceFileWriter, blobStorageService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                // No database files are created
                Assert.Empty(contentDbContext.Files);
            }
        }

        [Fact]
        public async Task CreateDataGuidanceFile_UploadThrows()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var mockFile = CreateMockFile();

                var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>();

                dataGuidanceFileWriter
                    .Setup(s => s.WriteFile(release.Id, It.IsAny<string>()))
                    .ReturnsAsync(mockFile);

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(
                        s => s.UploadStream(
                            PrivateReleaseFiles,
                            It.Is<string>(path => path.StartsWith($"{release.Id}/data-guidance")),
                            mockFile,
                            "text/plain",
                            null
                        )
                    )
                    .ThrowsAsync(new Exception("Something went wrong with the upload"));

                var service = BuildDataGuidanceFileService(
                    contentDbContext: contentDbContext,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object,
                    blobStorageService: blobStorageService.Object
                );

                var exception = await Assert.ThrowsAsync<Exception>(
                    async () => { await service.CreateDataGuidanceFile(release.Id); }
                );

                Assert.Equal("Something went wrong with the upload", exception.Message);

                MockUtils.VerifyAllMocks(dataGuidanceFileWriter, blobStorageService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                // No database files are created
                Assert.Empty(contentDbContext.Files);
            }
        }

        private FileStream CreateMockFile()
        {
            var path = Path.GetTempPath() + Guid.NewGuid() + ".txt";
            var file = File.Create(path);

            _files.Add(file);

            return file;
        }

        private static DataGuidanceFileService BuildDataGuidanceFileService(
            ContentDbContext contentDbContext,
            IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
            IBlobStorageService? blobStorageService = null)
        {
            return new DataGuidanceFileService(
                contentDbContext,
                dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(),
                blobStorageService ?? Mock.Of<IBlobStorageService>()
            );
        }
    }
}