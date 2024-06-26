using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataArchiveValidationServiceTests
    {
        [Fact]
        public void UploadedZippedDatafileIsValid()
        {
            var fileTypeService = new Mock<IFileTypeService>(MockBehavior.Strict);

            var service = SetupDataArchiveValidationService(fileTypeService: fileTypeService.Object);
            var archive = CreateFormFileFromResource("data-zip-valid.zip");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(archive, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => true);
            fileTypeService
                .Setup(s => s.HasMatchingEncodingType(archive, It.IsAny<IEnumerable<string>>()))
                .Returns(() => true);

            var result = service.ValidateDataArchiveFile(archive).Result;

            Assert.True(result.IsRight);

            VerifyAllMocks(fileTypeService);
        }

        [Fact]
        public void UploadedZippedDataFilenameTooLong()
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

            var result = service.ValidateDataArchiveFile(archive).Result;
            VerifyAllMocks(fileTypeService);

            result.AssertBadRequest(DataZipFilenameTooLong);
        }

        [Fact]
        public void UploadedZippedDatafileIsInvalid()
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

            var result = service.ValidateDataArchiveFile(archive).Result;
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
            IFileTypeService fileTypeService = null)
        {
            return new DataArchiveValidationService(
                fileTypeService ?? new Mock<IFileTypeService>().Object
            );
        }
    }
}
