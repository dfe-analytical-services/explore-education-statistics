#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyImageServiceTests
    {
        private readonly User _user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task Delete()
        {
            var methodologyVersion = new MethodologyVersion();

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var imageFile3 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image3.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2, imageFile3);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDeleteBlob(PrivateMethodologyFiles, imageFile1.Path());
            blobStorageService.SetupDeleteBlob(PrivateMethodologyFiles, imageFile2.Path());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(methodologyVersion.Id,
                    AsList(imageFile1.File.Id, imageFile2.File.Id));

                result.AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile1.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile1.File.Id));
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile2.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile2.File.Id));

                // Check that other file remains untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile3.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile3.File.Id));
            }
        }

        [Fact]
        public async Task Delete_FilesLinkedToOtherMethodologyVersions()
        {
            var methodologyVersion = new MethodologyVersion();
            var anotherVersion = new MethodologyVersion();

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var imageFile2UsedByAnotherMethodology = new MethodologyFile
            {
                MethodologyVersion = anotherVersion,
                File = imageFile2.File
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2,
                    imageFile2UsedByAnotherMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDeleteBlob(PrivateMethodologyFiles, imageFile1.Path());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(methodologyVersion.Id,
                    AsList(imageFile1.File.Id, imageFile2.File.Id));

                result.AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // All of the MethodologyFiles links for the methodology having the images removed from it are removed.
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile1.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile1.File.Id));
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile2.Id));

                // However, the file that is still linked to anotherMethodology remains in the File table, as does
                // its link to anotherMethodology.
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile2UsedByAnotherMethodology.Id));

                var filesForFileLinkedToDeletingMethodology =
                    await contentDbContext.Files.FindAsync(imageFile2.File.Id);

                var filesForFileLinkedToAnotherMethodology =
                    await contentDbContext.Files.FindAsync(imageFile2UsedByAnotherMethodology.File.Id);

                Assert.NotNull(filesForFileLinkedToAnotherMethodology);

                // Sanity check that the File entry that remains is the same File as was referenced by the
                // MethodologyFile link that was deleted.
                Assert.Equal(filesForFileLinkedToDeletingMethodology, filesForFileLinkedToAnotherMethodology);
            }
        }

        [Fact]
        public async Task Delete_InvalidFileType()
        {
            var methodologyVersion = new MethodologyVersion();

            var ancillaryFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var imageFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
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
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddRangeAsync(ancillaryFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.Delete(methodologyVersion.Id,
                    AsList(ancillaryFile.File.Id, imageFile.File.Id));

                result.AssertBadRequest(FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }
        }

        [Fact]
        public async Task Delete_MethodologyVersionNotFound()
        {
            var methodologyVersion = new MethodologyVersion();

            var imageFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
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
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddAsync(imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.Delete(Guid.NewGuid(),
                    AsList(imageFile.File.Id));

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }
        }

        [Fact]
        public async Task Delete_FileNotFound()
        {
            var methodologyVersion = new MethodologyVersion();

            var imageFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
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
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddAsync(imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.Delete(methodologyVersion.Id,
                    AsList(imageFile.File.Id, Guid.NewGuid()));

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }
        }

        [Fact]
        public async Task DeleteAll()
        {
            var methodologyVersion = new MethodologyVersion();
            var anotherVersion = new MethodologyVersion();

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var imageFile3 = new MethodologyFile
            {
                MethodologyVersion = anotherVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image3.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddRangeAsync(methodologyVersion, anotherVersion);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2, imageFile3);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDeleteBlob(PrivateMethodologyFiles, imageFile1.Path());
            blobStorageService.SetupDeleteBlob(PrivateMethodologyFiles, imageFile2.Path());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(methodologyVersion.Id);

                MockUtils.VerifyAllMocks(blobStorageService);
                
                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile1.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile1.File.Id));

                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile2.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile2.File.Id));

                // Check that other file remains untouched since it is unrelated to this version
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile3.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile3.File.Id));
            }
        }

        [Fact]
        public async Task DeleteAll_MethodologyVersionNotFound()
        {
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.DeleteAll(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task DeleteAll_NoFiles()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.DeleteAll(methodologyVersion.Id);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task DeleteAll_FilesLinkedToOtherMethodologyVersions()
        {
            var methodologyVersion = new MethodologyVersion();
            var anotherVersion = new MethodologyVersion();

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var imageFile2UsedByAnotherMethodology = new MethodologyFile
            {
                MethodologyVersion = anotherVersion,
                File = imageFile2.File
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2,
                    imageFile2UsedByAnotherMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDeleteBlob(PrivateMethodologyFiles, imageFile1.Path());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(methodologyVersion.Id);

                result.AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // All of the MethodologyFiles links for the methodology having the images removed from it are removed.
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile1.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile1.File.Id));
                Assert.Null(await contentDbContext.MethodologyFiles.FindAsync(imageFile2.Id));

                // However, the file that is still linked to anotherMethodology remains in the File table, as does
                // its link to anotherMethodology.
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile2UsedByAnotherMethodology.Id));

                var filesForFileLinkedToDeletingMethodology =
                    await contentDbContext.Files.FindAsync(imageFile2.File.Id);

                var filesForFileLinkedToAnotherMethodology =
                    await contentDbContext.Files.FindAsync(imageFile2UsedByAnotherMethodology.File.Id);

                Assert.NotNull(filesForFileLinkedToAnotherMethodology);

                // Sanity check that the File entry that remains is the same File as was referenced by the
                // MethodologyFile link that was deleted.
                Assert.Equal(filesForFileLinkedToDeletingMethodology, filesForFileLinkedToAnotherMethodology);
            }
        }

        [Fact]
        public async Task Stream()
        {
            var methodologyVersion = new MethodologyVersion();

            var methodologyFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
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
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.AddAsync(methodologyFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: methodologyFile.Path(),
                size: "1 Mb",
                contentType: "image/png",
                contentLength: 0L,
                meta: null,
                created: null);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateMethodologyFiles, methodologyFile.Path()))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateMethodologyFiles, methodologyFile.Path(),
                        It.IsAny<MemoryStream>(), null))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(methodologyVersion.Id, methodologyFile.File.Id);

                var image = result.AssertRight();

                blobStorageService.Verify(
                    mock => mock.GetBlob(PrivateMethodologyFiles, methodologyFile.Path()),
                    Times.Once());

                blobStorageService.Verify(
                    mock =>
                        mock.DownloadToStream(PrivateMethodologyFiles, methodologyFile.Path(),
                        It.IsAny<MemoryStream>(), null),
                    Times.Once());

                Assert.Equal("image/png", image.ContentType);
                Assert.Equal("image.png", image.FileDownloadName);
                Assert.IsType<MemoryStream>(image.FileStream);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_MethodologyVersionNotFound()
        {
            var methodologyFile = new MethodologyFile
            {
                MethodologyVersion = new MethodologyVersion(),
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
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(methodologyVersion.Id, Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Upload()
        {
            const string filename = "image.png";

            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateMethodologyFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(methodologyVersion.Id, Image))),
                    formFile,
                    null
                )).Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateMethodologyFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(methodologyVersion.Id, Image)))))
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

                var result = await service.Upload(methodologyVersion.Id, formFile);

                var upload = result.AssertRight();

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Image), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateMethodologyFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(methodologyVersion.Id, Image))),
                        formFile,
                        null
                    ), Times.Once);

                blobStorageService.Verify(mock =>
                        mock.GetBlob(PrivateMethodologyFiles,
                            It.Is<string>(path =>
                                path.Contains(FilesPath(methodologyVersion.Id, Image)))),
                    Times.Once);

                Assert.True(upload.ContainsKey("default"));
                Assert.Contains($"/api/methodologies/{methodologyVersion.Id}/images/", upload["default"]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyFile = await contentDbContext.MethodologyFiles
                    .Include(mf => mf.File)
                    .SingleOrDefaultAsync(mf =>
                        mf.MethodologyVersionId == methodologyVersion.Id
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
        public async Task Upload_MethodologyVersionNotFound()
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
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IFileRepository? fileRepository = null,
            IMethodologyFileRepository? methodologyFileRepository = null,
            IUserService? userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new MethodologyImageService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? Mock.Of<IBlobStorageService>(MockBehavior.Strict),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(MockBehavior.Strict),
                fileRepository ?? new FileRepository(contentDbContext),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
