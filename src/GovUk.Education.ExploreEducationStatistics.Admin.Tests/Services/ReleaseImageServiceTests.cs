#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseImageServiceTests
    {
        private readonly User _user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task Stream()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    ContentType = "image/png",
                    Type = Image
                }
            };

            var fileData = new byte[] { 0 };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

            privateBlobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile.Path(), fileData);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseImageService(contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(privateBlobStorageService);

                var fileStreamResult = result.AssertRight();

                Assert.Equal("image/png", fileStreamResult.ContentType);
                Assert.Equal("image.png", fileStreamResult.FileDownloadName);
                Assert.Equal(fileData, fileStreamResult.FileStream.ReadFully());
            }
        }

        [Fact]
        public async Task Stream_ReleaseNotFound()
        {
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
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
                await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseImageService(contentDbContext: contentDbContext);

                var result = await service.Stream(Guid.NewGuid(), releaseFile.File.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Stream_ReleaseFileNotFound()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseImageService(contentDbContext: contentDbContext);

                var result = await service.Stream(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Stream_BlobDoesNotExist()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    ContentType = "image/png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

            privateBlobStorageService.SetupDownloadToStreamNotFound(PrivateReleaseFiles, releaseFile.Path());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseImageService(contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(privateBlobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Upload()
        {
            const string filename = "image.png";

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename, "image/png").Object;
            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);

            privateBlobStorageService.Setup(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(release.Id, Image))),
                    formFile
                )).Returns(Task.CompletedTask);

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(formFile, Image))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseImageService(contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.Upload(release.Id, formFile);

                MockUtils.VerifyAllMocks(privateBlobStorageService, fileUploadsValidatorService);

                Assert.True(result.IsRight);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Image), Times.Once);

                privateBlobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Image))),
                        formFile
                    ), Times.Once);

                Assert.True(result.Right.ContainsKey("default"));
                Assert.Contains($"/api/releases/{release.Id}/images/", result.Right["default"]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseFile = await contentDbContext.ReleaseFiles
                    .Include(mf => mf.File)
                    .SingleOrDefaultAsync(mf =>
                        mf.ReleaseId == release.Id
                        && mf.File.Filename == filename
                        && mf.File.Type == Image
                    );

                Assert.NotNull(releaseFile);
                var file = releaseFile.File;

                Assert.Equal(10240, file.ContentLength);
                Assert.Equal("image/png", file.ContentType);
                Assert.InRange(DateTime.UtcNow.Subtract(file.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, file.CreatedById);
            }
        }

        [Fact]
        public async Task Upload_ReleaseNotFound()
        {
            var formFile = CreateFormFileMock("image.png", "image/png").Object;
            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseImageService(contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.Upload(Guid.NewGuid(), formFile);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(privateBlobStorageService, fileUploadsValidatorService);
        }

        private ReleaseImageService SetupReleaseImageService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPrivateBlobStorageService? privateBlobStorageService = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IUserService? userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new ReleaseImageService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                privateBlobStorageService ?? new Mock<IPrivateBlobStorageService>(Strict).Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>(Strict).Object,
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
