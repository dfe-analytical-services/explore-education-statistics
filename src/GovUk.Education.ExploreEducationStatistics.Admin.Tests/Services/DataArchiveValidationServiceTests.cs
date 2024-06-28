#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static Moq.MockBehavior;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataArchiveValidationServiceTests
    {
        [Fact]
        public async Task ValidateDataArchiveFile_Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object,
                fileUploadsValidatorService: fileUploadsValidatorService.Object);
            var archive = CreateFormFileFromResource("data-zip-valid.zip");

            fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    It.IsAny<ArchiveDataSetFile>(),
                    It.IsAny<Stream>(),
                    It.IsAny<Stream>(),
                    null))
                .ReturnsAsync([]);

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

            var result = await service.ValidateDataArchiveFile(
                releaseVersionId,
                "Data set title",
                archive);

            Assert.True(result.IsRight);

            VerifyAllMocks(fileUploadsValidatorService, fileTypeService);
        }

        [Fact]
        public async Task ValidateDataArchiveFile_ZipFilenameTooLong_DoesNotEndInDotZip()
        {
            var fileTypeService = new Mock<IFileTypeService>(Strict);
            var service = SetupDataArchiveValidationService(fileTypeService: fileTypeService.Object);

            var fileName =
                "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.zipp";
            var archive = CreateFormFileFromResource("data-zip-valid.zip", fileName);

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(() => true);

            var result = await service.ValidateDataArchiveFile(
                Guid.NewGuid(),
                "Data set title",
                archive);
            VerifyAllMocks(fileTypeService);

            result
                .AssertLeft()
                .AssertBadRequestWithValidationErrors([
                    ValidationMessages.GenerateErrorFilenameTooLong(
                        fileName, DataArchiveValidationService.MaxFilenameSize),
                    ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(fileName),
            ]);
        }

        [Fact]
        public async Task ValidateDataArchiveFile_DataZipShouldContainTwoFiles()
        {
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("data-zip-invalid.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(() => true);

            var result = await service.ValidateDataArchiveFile(
                Guid.NewGuid(),
                "Data set title",
                archive);
            VerifyAllMocks(fileTypeService);

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

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object,
                fileUploadsValidatorService: fileUploadsValidatorService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-valid.zip");

            fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    It.IsAny<ArchiveDataSetFile>(),
                    It.IsAny<Stream>(),
                    It.IsAny<Stream>(),
                    null))
                .ReturnsAsync([]);

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

            var result = await service.ValidateBulkDataArchiveFile(
                releaseVersionId,
                archive);

            Assert.True(result.IsRight);

            VerifyAllMocks(fileUploadsValidatorService, fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_IsValidZipFile()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object);
            var longFilename =
                "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooongfilename.csv";
            var archive = CreateFormFileFromResource("test-data.csv", longFilename);

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, AllowedArchiveMimeTypes))
                .ReturnsAsync(false);

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

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_NoDatasetNamesCsv()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-no-datasetnames-csv.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, AllowedArchiveMimeTypes))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

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

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_DatasetNamesCsvIncorrectHeaders()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-datasetnames-headers.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, AllowedArchiveMimeTypes))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

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

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_FilesNotFoundInZip()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-files-not-found.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, AllowedArchiveMimeTypes))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

            var result = await service.ValidateBulkDataArchiveFile(
                releaseVersionId,
                archive);

            result
                .AssertLeft()
                .AssertBadRequestWithValidationErrors([
                    ValidationMessages.GenerateErrorFileNotFoundInZip("one.meta.csv", FileType.Metadata),
                    ValidationMessages.GenerateErrorFileNotFoundInZip("two.csv", FileType.Data),
                ]);

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_DuplicateDataSetTitlesAndFilenames()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-duplicate-names.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, AllowedArchiveMimeTypes))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

            var result = await service.ValidateBulkDataArchiveFile(
                releaseVersionId,
                archive);

            result
                .AssertLeft()
                .AssertBadRequestWithValidationErrors([
                    ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique("Duplicate title"),
                    ValidationMessages.GenerateErrorDatasetNamesCsvFilenamesShouldBeUnique("one"),
                ]);

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_DataSetNamesCsvFilesnamesShouldNotEndDotCsv()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-filename-contains-extension.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, AllowedArchiveMimeTypes))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

            var result = await service.ValidateBulkDataArchiveFile(
                releaseVersionId,
                archive);

            result
                .AssertLeft()
                .AssertBadRequestWithValidationErrors([
                    ValidationMessages.GenerateErrorDatasetNamesCsvFilenamesShouldNotEndDotCsv("one.csv")
                ]);

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFile_Fail_UnusedFilesInZip()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var fileTypeService = new Mock<IFileTypeService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object,
                fileUploadsValidatorService: fileUploadsValidatorService.Object);
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-unused-files.zip");

            fileUploadsValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    It.IsAny<ArchiveDataSetFile>(),
                    It.IsAny<Stream>(),
                    It.IsAny<Stream>(),
                    null))
                .ReturnsAsync([]);

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(true);

            var result = await service.ValidateBulkDataArchiveFile(
                releaseVersionId,
                archive);

            result
                .AssertLeft()
                .AssertBadRequestWithValidationErrors([
                    ValidationMessages.GenerateErrorZipContainsUnusedFiles(["two.csv", "two.meta.csv"]),
                ]);

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
            IFileTypeService? fileTypeService = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null)
        {
            return new DataArchiveValidationService(
                fileTypeService ?? Mock.Of<IFileTypeService>(Strict),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(Strict)
            );
        }
    }
}
