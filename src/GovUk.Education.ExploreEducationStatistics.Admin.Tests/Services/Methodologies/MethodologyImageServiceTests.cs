using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyImageServiceTests
    {
        private readonly User _user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task Stream()
        {
            var methodology = new Methodology();

            var methodologyFile = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    PrivateBlobPathMigrated = true,
                    PublicBlobPathMigrated = true,
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.AddAsync(methodologyFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: null,
                size: null,
                contentType: "image/png",
                contentLength: 0L,
                meta: null,
                created: null);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateMethodologyFiles, methodologyFile.Path()))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateMethodologyFiles, methodologyFile.Path(),
                        It.IsAny<MemoryStream>()))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(methodology.Id, methodologyFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock => mock.GetBlob(PrivateMethodologyFiles, methodologyFile.Path()),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.DownloadToStream(PrivateMethodologyFiles, methodologyFile.Path(),
                        It.IsAny<MemoryStream>()), Times.Once());

                Assert.Equal("image/png", result.Right.ContentType);
                Assert.Equal("image.png", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_MethodologyNotFound()
        {
            var methodologyFile = new MethodologyFile
            {
                Methodology = new Methodology(),
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyFiles.AddAsync(methodologyFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(Guid.NewGuid(), methodologyFile.File.Id);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_MethodologyFileNotFound()
        {
            var methodology = new Methodology();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(methodology.Id, Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Upload()
        {
            const string filename = "image.png";

            var methodology = new Methodology();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateMethodologyFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(methodology.Id, Image))),
                    formFile,
                    null
                )).Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateMethodologyFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(methodology.Id, Image)))))
                .ReturnsAsync(new BlobInfo(
                    path: "image/file/path",
                    size: "20 Kb",
                    contentType: "image/png",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(formFile, Image))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.Upload(methodology.Id, formFile);

                Assert.True(result.IsRight);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Image), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateMethodologyFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(methodology.Id, Image))),
                        formFile,
                        null
                    ), Times.Once);

                blobStorageService.Verify(mock =>
                        mock.GetBlob(PrivateMethodologyFiles,
                            It.Is<string>(path =>
                                path.Contains(FilesPath(methodology.Id, Image)))),
                    Times.Once);

                Assert.True(result.Right.ContainsKey("default"));
                Assert.Contains($"/api/methodologies/{methodology.Id}/images/", result.Right["default"]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyFile = await contentDbContext.MethodologyFiles
                    .Include(mf => mf.File)
                    .SingleOrDefaultAsync(mf =>
                        mf.MethodologyId == methodology.Id
                        && mf.File.Filename == filename
                        && mf.File.Type == Image
                    );

                Assert.NotNull(methodologyFile);
                Assert.InRange(DateTime.UtcNow.Subtract(methodologyFile.File.Created.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, methodologyFile.File.CreatedById);
            }

            MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService);
        }

        [Fact]
        public async Task Upload_MethodologyNotFound()
        {
            const string filename = "image.png";

            var formFile = CreateFormFileMock(filename).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.Upload(Guid.NewGuid(), formFile);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService);
        }

        private MethodologyImageService SetupMethodologyImageService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IMethodologyFileRepository methodologyFileRepository = null,
            IUserService userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new MethodologyImageService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
