using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataArchiveValidationServiceTests
    {
        [Fact]
        public async Task UploadedZippedDatafileIsValid()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileTypeService = new Mock<IFileTypeService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);

            var service = SetupDataArchiveValidationService(
                fileTypeService: fileTypeService.Object,
                fileUploadsValidatorService: fileUploadsValidatorService.Object);
            var archive = CreateFormFileFromResource("data-zip-valid.zip");

            fileUploadsValidatorService.Setup(mock => mock.ValidateDataFilesForUpload(
                releaseVersionId,
                It.IsAny<ArchiveDataSetFile>(),
                It.IsAny<Task<Stream>>, // @MarkFix I cannot mock this to save my life
                It.IsAny<Task<Stream>>,
                null))
                .ReturnsAsync([]);

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(() => true);

            var result = await service.ValidateDataArchiveFile(
                releaseVersionId,
                "Data set name",
                archive);

            Assert.True(result.IsRight);

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public async Task UploadedZippedDataFilenameTooLong()
        {
            var fileTypeService = new Mock<IFileTypeService>(Strict);
            var service = SetupDataArchiveValidationService(fileTypeService: fileTypeService.Object);

            var fileName = "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.zip";
            var archive = CreateFormFileCopyFromResource("data-zip-valid.zip", fileName);

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

            Assert.True(result.IsLeft); // @MarkFix abstract out
            var badRequest = (BadRequestObjectResult)result.Left;
            Assert.Equal(400, badRequest.StatusCode);
            var validationProblemViewModel = (ValidationProblemViewModel)badRequest.Value;
            var error = Assert.Single(validationProblemViewModel.Errors);
            Assert.Equal("FilenameTooLong", error.Code);
            Assert.Equal($"Filename '{fileName}' is too long. Should be at most 150 characters.", error.Message);
        }

        [Fact]
        public async Task UploadedZippedDatafileIsInvalid()
        {
            var fileTypeService = new Mock<IFileTypeService>(MockBehavior.Strict);

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

        private static IFormFile CreateFormFileFromResource(string fileName)
        {
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + fileName);

            return CreateFormFileFromResourceWithPath(filePath, fileName);
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

        private static IFormFile CreateFormFileCopyFromResource(string originalFileName, string newFileName)
        {
            var originalFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + originalFileName);

            var newFilePath = Path.GetTempPath() +  Path.DirectorySeparatorChar + newFileName;

            File.Copy(originalFilePath, newFilePath, true);

            return CreateFormFileFromResourceWithPath(newFilePath, newFileName);
        }

        private static DataArchiveValidationService SetupDataArchiveValidationService(
            IFileTypeService fileTypeService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null)
        {
            return new DataArchiveValidationService(
                fileTypeService ?? Mock.Of<IFileTypeService>(Strict),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(Strict)
            );
        }
    }
}
