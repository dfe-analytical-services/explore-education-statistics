#nullable enable
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using File = System.IO.File;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataArchiveValidationServiceTests
    {
        [Fact]
        public async Task ValidateDataArchiveFile_Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("data-zip-valid.zip");

            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    It.IsAny<ArchiveDataSetFile>(),
                    It.IsAny<Stream>(),
                    It.IsAny<Stream>()))
                .ReturnsAsync([]);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.ValidateDataArchiveFile(
                    releaseVersionId,
                    "Data set title",
                    archive);

                Assert.True(result.IsRight);
            }

            VerifyAllMocks(fileUploadsValidatorService, fileTypeService);
        }

        [Fact]
        public async Task ValidateDataArchiveFile_ZipFilenameTooLong_DoesNotEndInDotZip()
        {
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var fileName =
                "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.zipp";
            var archive = CreateFormFileFromResource("data-zip-valid.zip", fileName);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(() => true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateDataArchiveFile(
                    Guid.NewGuid(),
                    "Data set title",
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorFilenameTooLong(
                            fileName, DataArchiveValidationService.MaxFilenameSize),
                        ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(fileName),
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateDataArchiveFile_DataZipShouldContainTwoFiles()
        {
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var archive = CreateFormFileFromResource("data-zip-invalid.zip");

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(() => true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateDataArchiveFile(
                    Guid.NewGuid(),
                    "Data set title",
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        new ErrorViewModel
                        {
                            Code = ValidationMessages.DataZipShouldContainTwoFiles.Code,
                            Message = ValidationMessages.DataZipShouldContainTwoFiles.Message,
                        }
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-valid.zip");

            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                        releaseVersionId,
                        It.IsAny<ArchiveDataSetFile>(),
                        It.IsAny<Stream>(),
                        It.IsAny<Stream>()))
                    .ReturnsAsync([]);

                fileTypeService
                    .Setup(s => s.IsValidZipFile(archive))
                    .ReturnsAsync(true);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                var archiveDataSets = result.AssertRight();

                Assert.Equal(2, archiveDataSets.Count);

                Assert.Equal("First data set", archiveDataSets[0].Title);
                Assert.Equal("one.csv", archiveDataSets[0].DataFilename);
                Assert.Equal("one.meta.csv", archiveDataSets[0].MetaFilename);
                Assert.Equal(696, archiveDataSets[0].DataFileSize);
                Assert.Equal(210, archiveDataSets[0].MetaFileSize);
                Assert.Null(archiveDataSets[0].ReplacingFile);

                Assert.Equal("Second data set", archiveDataSets[1].Title);
                Assert.Equal("two.csv", archiveDataSets[1].DataFilename);
                Assert.Equal("two.meta.csv", archiveDataSets[1].MetaFilename);
                Assert.Equal(2085, archiveDataSets[1].DataFileSize);
                Assert.Equal(318, archiveDataSets[1].MetaFileSize);
                Assert.Null(archiveDataSets[1].ReplacingFile);
            }

            VerifyAllMocks(fileUploadsValidatorService, fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Replacement_Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-valid.zip");

            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var releaseFile = new ReleaseFile
            {
                Name = "First data set",
                ReleaseVersion = new ReleaseVersion
                {
                    Id = releaseVersionId,
                },
                File = new Content.Model.File
                {
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid(),
                    Filename = "one.csv",
                    ContentLength = 1024,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                        releaseVersionId,
                        It.IsAny<ArchiveDataSetFile>(),
                        It.IsAny<Stream>(),
                        It.IsAny<Stream>()))
                    .ReturnsAsync([]);

                fileTypeService
                    .Setup(s => s.IsValidZipFile(archive))
                    .ReturnsAsync(true);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                var archiveDataSets = result.AssertRight();

                Assert.Equal(2, archiveDataSets.Count);

                Assert.Equal("First data set", archiveDataSets[0].Title);
                Assert.Equal("one.csv", archiveDataSets[0].DataFilename);
                Assert.Equal("one.meta.csv", archiveDataSets[0].MetaFilename);
                Assert.Equal(696, archiveDataSets[0].DataFileSize);
                Assert.Equal(210, archiveDataSets[0].MetaFileSize);
                Assert.Equal(releaseFile.FileId, archiveDataSets[0].ReplacingFile!.Id);

                Assert.Equal("Second data set", archiveDataSets[1].Title);
                Assert.Equal("two.csv", archiveDataSets[1].DataFilename);
                Assert.Equal("two.meta.csv", archiveDataSets[1].MetaFilename);
                Assert.Equal(2085, archiveDataSets[1].DataFileSize);
                Assert.Equal(318, archiveDataSets[1].MetaFileSize);
                Assert.Null(archiveDataSets[1].ReplacingFile);
            }

            VerifyAllMocks(fileUploadsValidatorService, fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_IsValidZipFile()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var longFilename =
                "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooongfilename.csv";
            var archive = CreateFormFileFromResource("test-data.csv", longFilename);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorFilenameTooLong(longFilename,
                            FileUploadsValidatorService.MaxFilenameSize),
                        ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(longFilename),
                        ValidationMessages.GenerateErrorMustBeZipFile(longFilename),
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_NoDatasetNamesCsv()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-no-datasetnames-csv.zip");

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        new ErrorViewModel
                        {
                            Code = ValidationMessages.BulkDataZipMustContainDatasetNamesCsv.Code,
                            Message = ValidationMessages.BulkDataZipMustContainDatasetNamesCsv.Message,
                        }
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_DatasetNamesCsvIncorrectHeaders()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-datasetnames-headers.zip");

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        new ErrorViewModel
                        {
                            Code = ValidationMessages.DatasetNamesCsvIncorrectHeaders.Code,
                            Message = ValidationMessages.DatasetNamesCsvIncorrectHeaders.Message,
                        },
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_FilesNotFoundInZip()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-files-not-found.zip");

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorFileNotFoundInZip("one.meta.csv", FileType.Metadata),
                        ValidationMessages.GenerateErrorFileNotFoundInZip("two.csv", FileType.Data),
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_DuplicateDataSetTitlesAndFilenames()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-duplicate-names.zip");

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique("Duplicate title"),
                        ValidationMessages.GenerateErrorDatasetNamesCsvFilenamesShouldBeUnique("one"),
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_DataSetNamesCsvFilesnamesShouldNotEndDotCsv()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-filename-contains-extension.zip");

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object);
                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorDatasetNamesCsvFilenamesShouldNotEndDotCsv("one.csv")
                    ]);
            }

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_UnusedFilesInZip()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-unused-files.zip");

            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    It.IsAny<ArchiveDataSetFile>(),
                    It.IsAny<Stream>(),
                    It.IsAny<Stream>()))
                .ReturnsAsync([]);

            fileTypeService
                .Setup(s => s.IsValidZipFile(archive))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    fileTypeService: fileTypeService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.ValidateBulkDataArchiveFile(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorZipContainsUnusedFiles(["two.csv", "two.meta.csv"]),
                    ]);
            }

            VerifyAllMocks(fileUploadsValidatorService, fileTypeService);
        }

        private static IFormFile CreateFormFileFromResource(string fileName, string? newFileName = null)
        {
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources" + Path.DirectorySeparatorChar + fileName);

            return CreateFormFileFromResourceWithPath(filePath, newFileName ?? fileName);
        }

        private static IFormFile CreateFormFileFromResourceWithPath(string filePath, string fileName)
        {
            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));

            formFile
                .Setup(f => f.FileName)
                .Returns(() => fileName);

            return formFile.Object;
        }

        private static DataArchiveValidationService SetupDataArchiveValidationService(
            ContentDbContext? contentDbContext = null,
            IFileTypeService? fileTypeService = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null)
        {
            return new DataArchiveValidationService(
                contentDbContext ?? Mock.Of<ContentDbContext>(Strict),
            fileTypeService ?? Mock.Of<IFileTypeService>(Strict),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(Strict)
            );
        }
    }
}
