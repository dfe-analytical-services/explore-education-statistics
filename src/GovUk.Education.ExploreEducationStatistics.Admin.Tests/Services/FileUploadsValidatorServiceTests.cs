using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsServiceValidatorTests
    {
        [Fact]
        public async Task AncillaryFileCannotBeEmpty()
        {
            var (subjectService, fileTypeService) = Mocks();
            var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, null);

            var file = CreateFormFile("test.csv", "test.csv");

            var result = await service.ValidateFileForUpload(Guid.NewGuid(),
                file, ReleaseFileTypes.Ancillary, true);

            Assert.True(result.IsLeft);
            AssertValidationProblem(result.Left, FileCannotBeEmpty);
        }

        [Fact]
        public async Task AncillaryFileIsValid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var file = CreateSingleLineFormFile("test.csv", "test.csv");

                var result = await service.ValidateFileForUpload(Guid.NewGuid(),
                    file, ReleaseFileTypes.Ancillary, true);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task AncillaryFilenameIsInvalid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var file = CreateSingleLineFormFile("test 123.csv", "test 123.csv");

                var result = await service.ValidateFileForUpload(Guid.NewGuid(),
                    file, ReleaseFileTypes.Ancillary, true);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, FilenameCannotContainSpacesOrSpecialCharacters);
            }
        }

        [Fact]
        public async Task AncillaryCannotBeOverwritten()
        {
            var (subjectService, fileTypeService) = Mocks();
            await using (var context = InMemoryApplicationDbContext())
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
                await context.SaveChangesAsync();

                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var file = CreateSingleLineFormFile("test.csv", "test.csv");

                var result = await service.ValidateFileForUpload(releaseId,
                    file, ReleaseFileTypes.Ancillary, false);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, CannotOverwriteFile);
            }
        }

        [Fact]
        public async Task AncillaryFileTypeIsValid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var file = CreateSingleLineFormFile("test.csv", "test.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                var result = await service.ValidateUploadFileType(file, ReleaseFileTypes.Ancillary);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task AncillaryFileTypeIsInvalid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var file = CreateSingleLineFormFile("test.csv", "test.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => false);
                var result = await service.ValidateUploadFileType(file, ReleaseFileTypes.Ancillary);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, FileTypeInvalid);
            }
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameContainsSpecialCharacters()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var result = await service.ValidateSubjectName(Guid.NewGuid(), "Subject & Title");

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SubjectTitleCannotContainSpecialCharacters);
            }
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameNotUnique()
        {
            var (subjectService, fileTypeService) = Mocks();

            subjectService.Setup(service => service.Get(It.IsAny<Guid>(), "Subject Title"))
                .ReturnsAsync(new Subject());

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var result = await service.ValidateSubjectName(Guid.NewGuid(),  "Subject Title");

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SubjectTitleMustBeUnique);
            }
        }

        [Fact]
        public async Task UploadedDatafilesAreValid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

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

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileIsEmpty()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var dataFile = CreateFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, DataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, MetadataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNotCsv()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(dataFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => false);
                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(metaFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(dataFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(metaFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, DataFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileNotCsv()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(dataFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(metaFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => false);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(dataFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(metaFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, MetaFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task UploadedZippedDatafileIsValid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var archiveFile = GetArchiveFile("data-zip-valid.zip");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(It.IsAny<Stream>(),
                        It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task UploadedZippedDatafileIsInvalid()
        {
            var (subjectService, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectService.Object, fileTypeService.Object, context);

                var archiveFile = GetArchiveFile("data-zip-invalid.zip");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(It.IsAny<Stream>(),
                        It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, DataFileMustBeCsvFile);
            }
        }

        private static (Mock<ISubjectService>, Mock<IFileTypeService>) Mocks()
        {
            return (
                new Mock<ISubjectService>(),
                new Mock<IFileTypeService>()
            );
        }

        private static IFormFile CreateSingleLineFormFile( string fileName, string name)
        {
            return CreateFormFile(fileName, name, "line1");
        }

        private static IFormFile CreateFormFile(string fileName, string name, params string[] lines)
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

        private static DataArchiveFile GetArchiveFile(string archiveFileName)
        {
            var archiveFile = CreateFormFileFromResource(archiveFileName);

            using var stream = archiveFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            var file1 = archive.Entries[0];
            var file2 = archive.Entries[1];

            var dataFile = file1.Name.Contains(".meta.") ? file2 : file1;
            var metaFile = file1.Name.Contains(".meta.") ? file1 : file2;

            return new DataArchiveFile(dataFile, metaFile);
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