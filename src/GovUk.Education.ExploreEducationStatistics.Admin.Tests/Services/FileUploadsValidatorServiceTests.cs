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
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsValidatorServiceTests
    {
        [Fact]
        public async Task ValidateFileForUpload_FileCannotBeEmpty()
        {
            await using var context = InMemoryApplicationDbContext();
            
            var (subjectRepository, fileTypeService) = Mocks();
            var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

            var file = CreateFormFile("test.csv", "test.csv");

            var result = await service.ValidateFileForUpload(file, Ancillary);
            VerifyAllMocks(subjectRepository, fileTypeService);

            result.AssertBadRequest(FileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateFileForUpload_ExceptionThrownForDataFileType()
        {
            await using var context = InMemoryApplicationDbContext();
            
            var (subjectRepository, fileTypeService) = Mocks();
            var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

            var file = CreateFormFile("test.csv", "test.csv");

            await Assert.ThrowsAsync<ArgumentException>(() => service.ValidateFileForUpload(file, FileType.Data));
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsValid()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using var context = InMemoryApplicationDbContext();
            
            var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

            var file = CreateSingleLineFormFile("test.csv", "test.csv");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => true);
            
            var result = await service.ValidateFileForUpload(file, Ancillary);
            VerifyAllMocks(subjectRepository, fileTypeService);

            Assert.True(result.IsRight);
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsInvalid()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using var context = InMemoryApplicationDbContext();
            
            var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

            var file = CreateSingleLineFormFile("test.csv", "test.csv");

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => false);
            
            var result = await service.ValidateFileForUpload(file, Ancillary);
            VerifyAllMocks(subjectRepository, fileTypeService);

            result.AssertBadRequest(FileTypeInvalid);
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameContainsSpecialCharacters()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

                var result = await service.ValidateSubjectName(Guid.NewGuid(), "Subject & Title");
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(SubjectTitleCannotContainSpecialCharacters);
            }
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameNotUnique()
        {
            var release = new Release();
            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Subject Title",
                File = new File
                {
                    Type = FileType.Data,
                }
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(dataReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var (subjectRepository, fileTypeService) = Mocks();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, contentDbContext);
                var result = await service.ValidateSubjectName(release.Id,  "Subject Title");
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(SubjectTitleMustBeUnique);
            }
        }

        [Fact]
        public async Task UploadedDatafilesAreValid()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

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
                VerifyAllMocks(subjectRepository, fileTypeService);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileIsEmpty()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

                var dataFile = CreateFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(DataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(MetadataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNotCsv()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

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
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(DataFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileNotCsv()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

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
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(MetaFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DuplicateDataFile()
        {
            var releaseId = Guid.NewGuid();

            var (subjectRepository, fileTypeService) = Mocks();

            
            await using (var context = InMemoryApplicationDbContext())
            {
                context.ReleaseFiles.Add(new ReleaseFile
                {
                    ReleaseId = releaseId,
                    File = new File
                    {
                        Type = FileType.Data,
                        Filename = "test.csv"
                    }
                });

                await context.SaveChangesAsync();
            }
            
            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

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

                var result = await service.ValidateDataFilesForUpload(releaseId, dataFile, metaFile);
                VerifyAllMocks(subjectRepository, fileTypeService);

                result.AssertBadRequest(CannotOverwriteDataFile);
            }
        }

        [Fact]
        public async Task UploadedZippedDatafileIsValid()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

                var archiveFile = GetArchiveFile("data-zip-valid.zip");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(It.IsAny<Stream>(),
                        It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);
                VerifyAllMocks(subjectRepository, fileTypeService);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task UploadedZippedDatafileIsInvalid()
        {
            var (subjectRepository, fileTypeService) = Mocks();

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = new FileUploadsValidatorService(subjectRepository.Object, fileTypeService.Object, context);

                var archiveFile = GetArchiveFile("data-zip-invalid.zip");

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);

                result.AssertBadRequest(DataFileMustBeCsvFile);
            }
        }

        private static (Mock<ISubjectRepository>, Mock<IFileTypeService>) Mocks()
        {
            return (
                new Mock<ISubjectRepository>(Strict),
                new Mock<IFileTypeService>(Strict)
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
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources" + Path.DirectorySeparatorChar + fileName);

            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => System.IO.File.OpenRead(filePath));
            return formFile.Object;
        }
    }
}
