using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsServiceValidatorTests
    {
        [Fact]
        public void AncillaryFileCannotBeEmpty()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);

            var lines = new List<string>().AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");

            var result = service.ValidateFileForUpload(cloudBlobContainer.Object, Guid.NewGuid(),
                file, ReleaseFileTypes.Ancillary, true).Result;

            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("FILE_CANNOT_BE_EMPTY", details.Errors[""].First());
        }

        [Fact]
        public void AncillaryFileIsValid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);

            var lines = new List<string>{"line1"}.AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");
            
            var result = service.ValidateFileForUpload(cloudBlobContainer.Object, Guid.NewGuid(),
                file, ReleaseFileTypes.Ancillary, true).Result;

            Assert.True(result.IsRight);
        }

        [Fact]
        public void AncillaryFilenameIsInvalid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);
            var lines = new List<string>{"line1"}.AsEnumerable();
            var file = CreateFormFile(lines, "test 123.csv", "test 123.csv");
            
            var result = service.ValidateFileForUpload(cloudBlobContainer.Object, Guid.NewGuid(),
                file, ReleaseFileTypes.Ancillary, true).Result;

            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS", details.Errors[""].First());
        }

        [Fact]
        public void AncillaryCannotOverwriteBlobWhenExisting()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);
            
            var lines = new List<string>{"line1"}.AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");
            
            var result = service.ValidateFileForUpload(cloudBlobContainer.Object, Guid.NewGuid(),
                file, ReleaseFileTypes.Ancillary, false).Result;

            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("CANNOT_OVERWRITE_FILE", details.Errors[""].First());
        }
        
        [Fact]
        public void AncillaryFileTypeIsValid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);

            var lines = new List<string>{"line1"}.AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .Returns(() => true);
            var result = service.ValidateUploadFileType(file, Common.Model.ReleaseFileTypes.Ancillary).Result;
            
            Assert.True(result.IsRight);
        }

        [Fact]
        public void AncillaryFileTypeIsInvalid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);

            var lines = new List<string>{"line1"}.AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");
            
            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .Returns(() => false);
            var result = service.ValidateUploadFileType(file, Common.Model.ReleaseFileTypes.Ancillary).Result;
            
            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("FILE_TYPE_INVALID", details.Errors[""].First());
        }
        
        [Fact]
        public void UploadFileTypeCannotBeDatafile()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);

            var lines = new List<string>{"line1"}.AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .Returns(() => true);
            var result = service.ValidateUploadFileType(file, Common.Model.ReleaseFileTypes.Data).Result;
            
            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("CANNOT_USE_GENERIC_FUNCTION_TO_ADD_DATA_FILE", details.Errors[""].First());
        }
        
        private (Mock<ISubjectService>, Mock<IFileTypeService>, Mock<CloudBlobContainer>) Mocks()
        {
            return (
                new Mock<ISubjectService>(),
                new Mock<IFileTypeService>(),
                SetupMockedContainer());
        }

        private Mock<CloudBlobContainer> SetupMockedContainer()
        {
            var blobMock = new Mock<CloudBlockBlob>(new Uri("http://storageaccount/container/blob"));
            blobMock.Setup(b => b.Exists(null, null)).Returns(true);
            var containerMock = new Mock<CloudBlobContainer>(new Uri("http://storageaccount/container"));
            containerMock.Setup(c => c.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(blobMock.Object);
            return containerMock;
        }

        private static IFormFile CreateFormFile(IEnumerable<string> lines, string fileName, string name)
        {
            var mStream = new MemoryStream();
            var writer = new StreamWriter(mStream);

            foreach (var line in lines)
            {
                writer.WriteLine(line);
                writer.Flush();
            }

            var f = new FormFile(mStream, 0, mStream.Length, name,
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };
            return f;
        }
    }
}