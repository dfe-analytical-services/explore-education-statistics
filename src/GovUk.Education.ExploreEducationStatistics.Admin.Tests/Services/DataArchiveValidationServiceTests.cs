#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.ResponseErrorAssertUtils;
using static Moq.MockBehavior;

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
                "Data set name",
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
                "Data set name",
                archive);
            VerifyAllMocks(fileTypeService);

            Assert.True(result.IsLeft);
            AssertBadRequestHasErrors(result.Left, [
                ValidationMessages.GenerateErrorFileNameTooLong(
                    fileName, DataArchiveValidationService.MaxFilenameSize),
                ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(fileName),
            ]);
        }

        [Fact]
        public async Task ValidateDataArchiveFile_ZipDoesNotContainDataFiles()
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
                "Data set name",
                archive);
            VerifyAllMocks(fileTypeService);

            result.AssertBadRequest(DataZipFileDoesNotContainCsvFiles);
        }

        // @MarkFix ValidateBulkDataArchiveFile_Success
        // @MarkFix ValidateBulkDataArchiveFile_InvalidZipFile
        // @MarkFix ValidateBulkDataArchiveFile_NoDataSetNamesCsv
        // @MarkFix ValidateBulkDataArchiveFile_DataSetNamesCsvHasHeadersOnly - i.e. no data rows just columns in csv (duplicate of NoDataSetsInZip?)
        // @MarkFix ValidateBulkDataArchiveFile_DataSetNamesCsvIncorrectHeaders
        // @MarkFix ValidateBulkDataArchiveFile_FilesNotFoundInZip
        // @MarkFix ValidateBulkDataArchiveFile_DataSetNamesCsvContainsDuplicateDataSetNames
        // @MarkFix ValidateBulkDataArchiveFile_UnusedFilesInZip
        // @MarkFix ValidateBulkDataArchiveFile_NoDataSetsInZip

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
