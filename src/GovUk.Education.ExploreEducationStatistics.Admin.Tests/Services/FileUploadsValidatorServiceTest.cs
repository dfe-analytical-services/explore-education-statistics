using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        class FileInfo
        {
            public readonly string Filename;
        
            // we have a possible list of matching mime types that we'd expect to see, that potentially change when run
            // on different operating systems, but are considered valid either way 
            public readonly string[] ExpectedMimeTypes;

            public FileInfo(string filename, params string[] expectedMimeTypes)
            {
                Filename = filename;
                ExpectedMimeTypes = expectedMimeTypes;
            }
        }
        
        [Fact]
        public async void FileCannotBeEmpty()
        {
            var (subjectService, fileTypeService, cloudBlobContainer, emptyFormFile) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);

            var result = await service.ValidateFileForUpload(cloudBlobContainer.Object, Guid.NewGuid(),
                emptyFormFile.Object,ReleaseFileTypes.Ancillary, true);
            
            Assert.True(result.IsLeft); // Second time should be validation failure
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("FILE_CANNOT_BE_EMPTY", details.Errors[""].First());
        }
        
        [Fact]
        public async void CannotOverwriteBlobWhenExisting()
        {
            var (subjectService, fileTypeService, cloudBlobContainer, emptyFormFile) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object);
            
            var result = service.ValidateFileForUpload(cloudBlobContainer.Object, Guid.NewGuid(),
                emptyFormFile.Object,ReleaseFileTypes.Ancillary, false).Result;
            
            Assert.True(result.IsLeft); // Second time should be validation failure
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
            var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
            Assert.Equal("CANNOT_OVERWRITE_FILE", details.Errors[""].First());
        }

        private (Mock<ISubjectService>, Mock<IFileTypeService>, Mock<CloudBlobContainer>, Mock<IFormFile>) Mocks()
        {
            return (
                new Mock<ISubjectService>(), 
                new Mock<IFileTypeService>(),
                SetupMockedContainer(),
                SetupMockedEmptyFormFile());
        }

        private Mock<CloudBlobContainer> SetupMockedContainer()
        {
            var blobMock = new Mock<CloudBlockBlob>(new Uri("http://storageaccount/container/blob"));
            blobMock.Setup(b => b.Exists(null,null)).Returns(true);
            var containerMock = new Mock<CloudBlobContainer>(new Uri("http://storageaccount/container"));
            containerMock.Setup(c => c.GetBlockBlobReference(It.IsAny<string>()))
                .Returns(blobMock.Object);
            return containerMock;
        }

        private Mock<IFormFile> SetupMockedEmptyFormFile()
        {
            var csv = new FileInfo("test.csv", "application/csv");
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + csv.Filename);
            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));
            return formFile;
        }
    }
}