using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsServiceValidatorTests
    {
        [Fact]
        public void AncillaryFileCannotBeEmpty()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, null);

            var lines = new List<string>().AsEnumerable();
            var file = CreateFormFile(lines, "test.csv", "test.csv");

            var result = service.ValidateFileForUpload(Guid.NewGuid(),
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

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var lines = new List<string> {"line1"}.AsEnumerable();
                var file = CreateFormFile(lines, "test.csv", "test.csv");

                var result = service.ValidateFileForUpload(Guid.NewGuid(),
                    file, ReleaseFileTypes.Ancillary, true).Result;

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public void AncillaryFilenameIsInvalid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);
                var lines = new List<string> {"line1"}.AsEnumerable();
                var file = CreateFormFile(lines, "test 123.csv", "test 123.csv");

                var result = service.ValidateFileForUpload(Guid.NewGuid(),
                    file, ReleaseFileTypes.Ancillary, true).Result;

                Assert.True(result.IsLeft);
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS", details.Errors[""].First());
            }
        }

        [Fact]
        public void AncillaryCannotBeOverwritten()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();
            using (var context = InMemoryApplicationDbContext())
            {
                var releaseId = Guid.NewGuid();
                
                context.Add(new ReleaseFile
                {
                    
                    ReleaseId = releaseId,
                    ReleaseFileReference = new ReleaseFileReference()
                    {
                        ReleaseId = releaseId,
                        ReleaseFileType = ReleaseFileTypes.Ancillary,
                        Filename = "test.csv"
                    }
                });
                context.SaveChanges();
                
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var lines = new List<string> {"line1"}.AsEnumerable();
                var file = CreateFormFile(lines, "test.csv", "test.csv");

                var result = service.ValidateFileForUpload(releaseId,
                    file, ReleaseFileTypes.Ancillary, false).Result;

                Assert.True(result.IsLeft);
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("CANNOT_OVERWRITE_FILE", details.Errors[""].First());
            }
        }
        
        [Fact]
        public void AncillaryFileTypeIsValid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var lines = new List<string> {"line1"}.AsEnumerable();
                var file = CreateFormFile(lines, "test.csv", "test.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                var result = service.ValidateUploadFileType(file, Common.Model.ReleaseFileTypes.Ancillary).Result;

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public void AncillaryFileTypeIsInvalid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var lines = new List<string> {"line1"}.AsEnumerable();
                var file = CreateFormFile(lines, "test.csv", "test.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => false);
                var result = service.ValidateUploadFileType(file, Common.Model.ReleaseFileTypes.Ancillary).Result;

                Assert.True(result.IsLeft);
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("FILE_TYPE_INVALID", details.Errors[""].First());
            }
        }
        
        [Fact]
        public void UploadedDatafilesAreValid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var lines = new List<string> {"line1"}.AsEnumerable();
                var dataFile = CreateFormFile(lines, "test.csv", "test.csv");
                var metaFile = CreateFormFile(lines, "test.meta.csv", "test.meta.csv");
                var subjectTitle = "Subject Title";

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(dataFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(metaFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(dataFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(metaFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                
                var result = service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile, subjectTitle).Result;

                Assert.True(result.IsRight);
            }
        }
        
        [Fact]
        public void UploadedDatafilesAreInvalid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var lines = new List<string> {"line1"}.AsEnumerable();
                var dataFile = CreateFormFile(lines, "test.csv", "test.csv");
                var metaFile = CreateFormFile(lines, "test.meta.csv", "test.meta.csv");
                var subjectTitle = "Subject & Title";

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(dataFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(metaFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(dataFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(metaFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                
                var result = service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile, subjectTitle).Result;

                Assert.True(result.IsLeft);
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("SUBJECT_TITLE_CANNOT_CONTAIN_SPECIAL_CHARACTERS", details.Errors[""].First());
            }
        }
        
        [Fact]
        public void UploadedZippedDatafileIsValid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var subjectTitle = "Subject Title";
                var entries = GetArchiveEntries("data-zip-valid.zip");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(It.IsAny<CloudBlob>(), 
                        It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                
                var result = service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(), 
                    entries.Item1,entries.Item2, subjectTitle).Result;

                Assert.True(result.IsRight);
            }
        }
        
        [Fact]
        public void UploadedZippedDatafileIsInvalid()
        {
            var (subjectService, fileTypeService, cloudBlobContainer) = Mocks();

            using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var subjectTitle = "Subject Title";
                var entries = GetArchiveEntries("data-zip-invalid.zip");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(It.IsAny<CloudBlob>(), 
                        It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                
                var result = service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(), 
                    entries.Item1,entries.Item2, subjectTitle).Result;

                Assert.True(result.IsLeft);
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);
                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("DATA_FILE_MUST_BE_CSV_FILE", details.Errors[""].First());
            }
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
        
        private static Tuple<ZipArchiveEntry, ZipArchiveEntry> GetArchiveEntries(string archiveFileName)
        {
            var archiveFile = CreateFormFileFromResource(archiveFileName);
            
            using var stream = archiveFile.OpenReadStream();
            using var archive = new ZipArchive(stream);
            
            var file1 = archive.Entries[0];
            var file2 = archive.Entries[1];
            
            var dataFile = file1.Name.Contains(".meta.") ? file2 : file1;
            var metaFile = file1.Name.Contains(".meta.") ? file1 : file2;
            
            return new Tuple<ZipArchiveEntry, ZipArchiveEntry>(dataFile, metaFile);
        }

        private static IFormFile CreateFormFileFromResource(string fileName)
        {
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + fileName);
            
            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));
            return formFile.Object;
        }
    }
}