#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsValidatorServiceTests
    {
        [Fact]
        public async Task ValidateFileForUpload_FileCannotBeEmpty()
        {
            var file = CreateFormFileMock("test.csv", 0).Object;

            var (service, _) = BuildService();
            var result = await service.ValidateFileForUpload(file, Ancillary);
            result.AssertBadRequest(FileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateFileForUpload_ExceptionThrownForDataFileType()
        {
            var file = CreateFormFileMock("test.csv").Object;

            var (service, _) = BuildService();
            await Assert.ThrowsAsync<ArgumentException>(() => service.ValidateFileForUpload(file, FileType.Data));
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsValid()
        {
            var file = CreateFormFileMock("test.csv").Object;

            var (service, fileTypeService) = BuildService();

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, AllowedAncillaryFileTypes))
                .ReturnsAsync(() => true);

            var result = await service.ValidateFileForUpload(file, Ancillary);
            VerifyAllMocks(fileTypeService);

            result.AssertRight();
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsInvalid()
        {
            var file = CreateFormFileMock("test.csv").Object;

            var (service, fileTypeService) = BuildService();

            fileTypeService
                .Setup(s => s.HasMatchingMimeType(file, AllowedAncillaryFileTypes))
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
        public async Task ValidateDataFilesForUpload_Valid()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;
                
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
                    .ReturnsAsync(() => true);

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

                var dataFile = CreateFormFileMock("test.csv", 0);
                var metaFile = CreateFormFileMock("test.meta.csv");

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile.Object, metaFile.Object);

                result.AssertBadRequest(DataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService(context);

                var dataFile = CreateFormFileMock("test.csv");
                var metaFile = CreateFormFileMock("test.meta.csv", 0);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile.Object, metaFile.Object);

                result.AssertBadRequest(MetadataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNotCsv()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
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

                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
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

                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                var result = await service.ValidateDataFilesForUpload(releaseId, dataFile, metaFile);

                result.AssertBadRequest(DataFilenameNotUnique);
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
                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                // The file being replaced here has the same name as the one being uploaded, but that's ok.
                var fileBeingReplaced = new File
                {
                    Filename = "test.csv"
                };

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
                    .ReturnsAsync(() => true);

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
                var dataFile = CreateFormFileMock("another.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

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

                var dataFile = CreateFormFileMock("another.csv").Object;

                // This metafile has the same name as an existing metafile, but it shouldn't matter for metadata files
                // as they don't appear anywhere where the filenames have to be unique.
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataFilesForUpload(releaseId, dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_Valid()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService(context);

                var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.csv").Object;

                var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                    archiveFile);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFileNotCsv()
        {
            var (service, _) = BuildService();

            var archiveFile = CreateDataArchiveFileMock("test.txt", "test.meta.csv").Object;

            var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                archiveFile);

            result.AssertBadRequest(DataFileMustBeCsvFile);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileNotCsv()
        {
            var (service, _) = BuildService();

            var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.txt").Object;

            var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                archiveFile);

            result.AssertBadRequest(MetaFileMustBeCsvFile);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIncorrectlyNamed()
        {
            var (service, _) = BuildService();

            var archiveFile = CreateDataArchiveFileMock("test.csv", "meta.csv").Object;

            var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                archiveFile);

            result.AssertBadRequest(MetaFileIsIncorrectlyNamed);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFileIsEmpty()
        {
            var (service, _) = BuildService();

            var archiveFile = CreateDataArchiveFileMock("test.csv",
                "test.meta.csv",
                dataFileSize: 0,
                metaFileSize: 1024).Object;

            var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                archiveFile);

            result.AssertBadRequest(DataFileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIsEmpty()
        {
            var (service, _) = BuildService();

            var archiveFile = CreateDataArchiveFileMock("test.csv",
                "test.meta.csv",
                dataFileSize: 1024,
                metaFileSize: 0).Object;

            var result = await service.ValidateDataArchiveEntriesForUpload(Guid.NewGuid(),
                archiveFile);

            result.AssertBadRequest(MetadataFileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DuplicateDataFile()
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

                var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.csv").Object;

                var result = await service.ValidateDataArchiveEntriesForUpload(releaseId, archiveFile);

                result.AssertBadRequest(DataFilenameNotUnique);
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_ReplacingDataFileWithFileOfSameName()
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

                // The replacement file here has the same name as the one it is replacing, so this should be ok.
                var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.csv").Object;

                // The file being replaced here has the same name as the one being uploaded, but that's ok.
                var fileBeingReplaced = new File
                {
                    Filename = "test.csv"
                };

                var result =
                    await service.ValidateDataArchiveEntriesForUpload(releaseId, archiveFile, fileBeingReplaced);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task
            ValidateDataArchiveEntriesForUpload_ReplacingDataFileWithFileOfDifferentNameButClashesWithAnother()
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
                var archiveFile = CreateDataArchiveFileMock("another.csv", "test.meta.csv").Object;

                var fileBeingReplaced = new File
                {
                    Filename = "test.csv"
                };

                var result = await service.ValidateDataArchiveEntriesForUpload(
                    releaseId, archiveFile, fileBeingReplaced);

                result.AssertBadRequest(DataFilenameNotUnique);
            }
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
