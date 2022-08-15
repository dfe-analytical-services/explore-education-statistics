#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
            var file = CreateFormFile("test.csv", "test.csv");

            var (service, _) = BuildService();
            var result = await service.ValidateFileForUpload(file, Ancillary);
            result.AssertBadRequest(FileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateFileForUpload_ExceptionThrownForDataFileType()
        {
            var file = CreateFormFile("test.csv", "test.csv");

            var (service, _) = BuildService();
            await Assert.ThrowsAsync<ArgumentException>(() => service.ValidateFileForUpload(file, FileType.Data));
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsValid()
        {
            var file = CreateSingleLineFormFile("test.csv", "test.csv");

            var (service, fileTypeService) = BuildService();

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => true);

            var result = await service.ValidateFileForUpload(file, Ancillary);
            VerifyAllMocks(fileTypeService);

            result.AssertRight();
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsInvalid()
        {
            var file = CreateSingleLineFormFile("test.csv", "test.csv");

            var (service, fileTypeService) = BuildService();

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, It.IsAny<IEnumerable<Regex>>()))
                .ReturnsAsync(() => false);

            var result = await service.ValidateFileForUpload(file, Ancillary);
            VerifyAllMocks(fileTypeService);

            result.AssertBadRequest(FileTypeInvalid);
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameContainsSpecialCharacters()
        {
            var (service, _) = BuildService();

            var result = await service.ValidateSubjectName(Guid.NewGuid(), "Subject & Title");

            result.AssertBadRequest(SubjectTitleCannotContainSpecialCharacters);
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var (service, _) = BuildService(contentDbContext);

                var result = await service.ValidateSubjectName(release.Id, "Subject Title");

                result.AssertBadRequest(SubjectTitleMustBeUnique);
            }
        }

        [Fact]
        public async Task UploadedDatafilesAreValid()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

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
                VerifyAllMocks(fileTypeService);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileIsEmpty()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService(context);

                var dataFile = CreateFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                result.AssertBadRequest(DataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService(context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);

                result.AssertBadRequest(MetadataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNotCsv()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(dataFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => false);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                result.AssertBadRequest(DataFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileNotCsv()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(dataFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingEncodingType(dataFile, It.IsAny<IEnumerable<string>>()))
                    .Returns(() => true);
                fileTypeService
                    .Setup(s => s.HasMatchingMimeType(metaFile, It.IsAny<IEnumerable<Regex>>()))
                    .ReturnsAsync(() => false);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                result.AssertBadRequest(MetaFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DuplicateDataFile()
        {
            var releaseId = Guid.NewGuid();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
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

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, _) = BuildService(context);

                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(releaseId, dataFile, metaFile);

                result.AssertBadRequest(DataFilenameNotUnique);
            }
        }

        [Fact]
        public async Task UploadedZippedDatafileIsValid()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService(context);

                var archiveFile = GetArchiveFile("data-zip-valid.zip");

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_ReplacingDataFileWithFileOfSameName()
        {
            var releaseId = Guid.NewGuid();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
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

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, fileTypeService) = BuildService(context);

                // The replacement file here has the same name as the one it is replacing, so this should be ok.
                var dataFile = CreateSingleLineFormFile("test.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                // The file being replaced here has the same name as the one being uploaded, but that's ok.
                var fileBeingReplaced = new File
                {
                    Filename = "test.csv"
                };

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

                var result = await service.ValidateDataFilesForUpload(
                    releaseId, dataFile, metaFile, fileBeingReplaced);
                VerifyAllMocks(fileTypeService);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_ReplacingDataFileWithFileOfDifferentNameButClashesWithAnother()
        {
            var releaseId = Guid.NewGuid();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(new ReleaseFile
                {
                    ReleaseId = releaseId,
                    File = new File
                    {
                        Type = FileType.Data,
                        Filename = "test.csv"
                    }
                }, new ReleaseFile
                {
                    ReleaseId = releaseId,
                    File = new File
                    {
                        Type = FileType.Data,
                        Filename = "another.csv"
                    }
                });

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, _) = BuildService(context);

                // The replacement file here has the same name as another unrelated data file i.e. one that's not being
                // replaced here, which should be a problem as it would otherwise result in duplicate data file names
                // in this Release after the replacement is complete.
                var dataFile = CreateSingleLineFormFile("another.csv", "test.csv");
                var metaFile = CreateSingleLineFormFile("test.meta.csv", "test.meta.csv");

                var fileBeingReplaced = new File
                {
                    Filename = "test.csv"
                };

                var result = await service.ValidateDataFilesForUpload(
                    releaseId, dataFile, metaFile, fileBeingReplaced);

                result.AssertBadRequest(DataFilenameNotUnique);
            }
        }

        [Fact]
        public async Task ValidateFileForUpload_MetadataFileNamesCanBeDuplicated()
        {
            var releaseId = Guid.NewGuid();

            var file = CreateSingleLineFormFile("test.csv", "test.csv");

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(new ReleaseFile
                {
                    ReleaseId = releaseId,
                    File = new File
                    {
                        Type = FileType.Data,
                        Filename = "test.csv"
                    }
                }, new ReleaseFile
                {
                    ReleaseId = releaseId,
                    File = new File
                    {
                        Type = Metadata,
                        Filename = "test.meta.csv"
                    }
                });

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateSingleLineFormFile("another.csv", "another.csv");

                // This metafile has the same name as an existing metafile, but it shouldn't matter for metadata files
                // as they don't appear anywhere where the filenames have to be unique (e.g. in zip files).
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
                VerifyAllMocks(fileTypeService);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task UploadedZippedDatafileIsInvalid()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService();

                var archiveFile = GetArchiveFile("data-zip-invalid.zip");

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);

                result.AssertBadRequest(DataFileMustBeCsvFile);
            }
        }

        private static IFormFile CreateSingleLineFormFile(string fileName, string name)
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

        private static (
            FileUploadsValidatorService,
            Mock<IFileTypeService> fileTypeService
            )
            BuildService(
                ContentDbContext? contentDbContext = null,
                IFileTypeService? fileTypeService = null)
        {
            var fileTypeServiceMock = new Mock<IFileTypeService>(Strict);

            var service = new FileUploadsValidatorService(
                fileTypeService ?? fileTypeServiceMock.Object,
                contentDbContext!);

            return (service, fileTypeServiceMock);
        }
    }
}
