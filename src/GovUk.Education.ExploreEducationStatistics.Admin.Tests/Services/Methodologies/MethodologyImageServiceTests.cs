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
        public async Task Delete()
        {
            var methodology = new Methodology();

            var imageFile1 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var imageFile3 = new MethodologyFile
            {
                Methodology = methodology,
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
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2, imageFile3);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateMethodologyFiles, imageFile1.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateMethodologyFiles, imageFile2.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(methodology.Id, new List<Guid>
                {
                    imageFile1.File.Id,
                    imageFile2.File.Id
                });

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateMethodologyFiles, imageFile1.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateMethodologyFiles, imageFile2.Path()), Times.Once);
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

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_InvalidFileType()
        {
            var methodology = new Methodology();

            var ancillaryFile = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };
            
            var imageFile = new MethodologyFile
            {
                Methodology = methodology,
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
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyFiles.AddRangeAsync(ancillaryFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(methodology.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    imageFile.File.Id,
                });

                Assert.True(result.IsLeft);
                ValidationTestUtil.AssertValidationProblem(result.Left, FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
                
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_MethodologyNotFound()
        {
            var methodology = new Methodology();

            var imageFile = new MethodologyFile
            {
                Methodology = methodology,
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
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyFiles.AddAsync(imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(Guid.NewGuid(), new List<Guid>
                {
                    imageFile.File.Id
                });

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_FileNotFound()
        {
            var methodology = new Methodology();

            var imageFile = new MethodologyFile
            {
                Methodology = methodology,
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
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyFiles.AddAsync(imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(methodology.Id, new List<Guid>
                {
                    imageFile.File.Id,
                    // Include an unknown id
                    Guid.NewGuid()
                });

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.MethodologyFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream()
        {
            var methodology = new Methodology();

            var methodologyFile = new MethodologyFile
            {
                Methodology = methodology,
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
            IFileRepository fileRepository = null,
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
                fileRepository ?? new FileRepository(contentDbContext),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
