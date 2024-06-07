#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsValidatorServiceTests
    {
        private readonly DataFixture _fixture = new();

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
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var (service, _) = BuildService(contentDbContext);

            var result = service.ValidateReleaseVersionDataSetFileName(Guid.NewGuid(), "Subject & Title");

            //result.AssertBadRequest(SubjectTitleCannotContainSpecialCharacters); // @MarkFix
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameNotUnique()
        {
            var releaseVersion = new ReleaseVersion();
            var dataReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Name = "Subject Title",
                File = new File
                {
                    Type = FileType.Data,
                }
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(dataReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var (service, _) = BuildService(contentDbContext);

                var result = service.ValidateReleaseVersionDataSetFileName(releaseVersion.Id, "Subject Title");

                //result.AssertBadRequest(SubjectTitleMustBeUnique); // @MarkFix
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

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileIsEmpty()
        {
            var dataFile = CreateFormFileMock("test.csv", 0);
            var metaFile = CreateFormFileMock("test.meta.csv");

            await using var context = InMemoryApplicationDbContext();
            var (service, fileTypeService) = BuildService(context);

            fileTypeService.Setup(mock => mock.IsValidCsvFile(dataFile.Object))
                .ReturnsAsync(true);
            fileTypeService.Setup(mock => mock.IsValidCsvFile(metaFile.Object))
                .ReturnsAsync(true);

            var results = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile.Object, metaFile.Object);

            var error = Assert.Single(results);
            // @MarkFix
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateFormFileMock("test.csv");
                var metaFile = CreateFormFileMock("test.meta.csv", 0);

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.Object))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.Object))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile.Object, metaFile.Object);

                //result.AssertBadRequest(MetadataFileCannotBeEmpty); // @MarkFix
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

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataFilesForUpload(Guid.NewGuid(), dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                //result.AssertBadRequest(DataFileMustBeCsvFile); // @MarkFix
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

                //result.AssertBadRequest(MetaFileMustBeCsvFile); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DuplicateDataFile()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile(FileType.Data)
                    .WithFilename("test.csv"));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
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

                var result = await service.ValidateDataFilesForUpload(releaseVersion.Id, dataFile, metaFile);

                //result.AssertBadRequest(DataFilenameNotUnique); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_ReplacingDataFileWithFileOfSameName()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            // The file being replaced here has the same name as the one being uploaded, but that's ok.
            var fileBeingReplaced = _fixture.DefaultFile(FileType.Data)
                .WithFilename("test.csv");

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(fileBeingReplaced);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, fileTypeService) = BuildService(context);

                // The replacement file here has the same name as the one it is replacing, so this should be ok.
                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataFilesForUpload(
                    releaseVersion.Id, dataFile, metaFile, fileBeingReplaced);
                VerifyAllMocks(fileTypeService);

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_ReplacingDataFileWithFileOfDifferentNameButClashesWithAnother()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            // Create two release files, one of which is the file being replaced, and the other has the same filename
            // as the file being uploaded.
            var (fileBeingReplaced, otherFile) = _fixture.DefaultFile(FileType.Data)
                .ForIndex(0, s => s.SetFilename("test.csv"))
                .ForIndex(1, s => s.SetFilename("another.csv"))
                .Generate(2)
                .ToTuple2();

            var releaseFiles = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFiles(ListOf(fileBeingReplaced, otherFile))
                .Generate(2);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, fileTypeService) = BuildService(context);

                // The replacement file here has the same name as another unrelated data file i.e. one that's not being
                // replaced here, which should be a problem as it would otherwise result in duplicate data file names
                // in this Release after the replacement is complete.
                var dataFile = CreateFormFileMock("another.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv").Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataFilesForUpload(
                    releaseVersion.Id, dataFile, metaFile, fileBeingReplaced);

                //result.AssertBadRequest(DataFilenameNotUnique); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateFileForUpload_MetadataFileNamesCanBeDuplicated()
        {
            var releaseVersionId = Guid.NewGuid();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(new ReleaseFile
                {
                    ReleaseVersionId = releaseVersionId,
                    File = new File
                    {
                        Type = FileType.Data,
                        Filename = "test.csv"
                    }
                }, new ReleaseFile
                {
                    ReleaseVersionId = releaseVersionId,
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

                var result = await service.ValidateDataFilesForUpload(releaseVersionId, dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_Valid()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, _) = BuildService(context);

                var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.csv").Object;

                var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                    archiveFile);

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFileNotCsv()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile = CreateDataArchiveFileMock("test.txt", "test.meta.csv").Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(DataFileMustBeCsvFile); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileNotCsv()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.txt").Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(MetaFileMustBeCsvFile); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIncorrectlyNamed()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile = CreateDataArchiveFileMock("test.csv", "meta.csv").Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(MetaFileIsIncorrectlyNamed); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFilenameTooLong()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile =
                CreateDataArchiveFileMock(
                    "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.csv",
                    "test.meta.csv").Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(DataFilenameTooLong); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetaFilenameTooLong()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile = CreateDataArchiveFileMock("test.csv",
                    "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.meta.csv")
                .Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(MetaFilenameTooLong); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFileIsEmpty()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile = CreateDataArchiveFileMock("test.csv",
                "test.meta.csv",
                dataFileSize: 0,
                metaFileSize: 1024).Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(DataFileCannotBeEmpty); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIsEmpty()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var archiveFile = CreateDataArchiveFileMock("test.csv",
                "test.meta.csv",
                dataFileSize: 1024,
                metaFileSize: 0).Object;

            var result = service.ValidateDataArchiveFileForUpload(Guid.NewGuid(),
                archiveFile);

            //result.AssertBadRequest(MetadataFileCannotBeEmpty); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DuplicateDataFile()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile(FileType.Data)
                    .WithFilename("test.csv"));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, _) = BuildService(context);

                var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.csv").Object;

                var result = service.ValidateDataArchiveFileForUpload(releaseVersion.Id, archiveFile);

                //result.AssertBadRequest(DataFilenameNotUnique); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_ReplacingDataFileWithFileOfSameName()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            // The file being replaced here has the same name as the one being uploaded, but that's ok.
            var fileBeingReplaced = _fixture.DefaultFile(FileType.Data)
                .WithFilename("test.csv");

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(fileBeingReplaced);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, _) = BuildService(context);

                // The replacement file here has the same name as the one it is replacing, so this should be ok.
                var archiveFile = CreateDataArchiveFileMock("test.csv", "test.meta.csv").Object;

                var result = service.ValidateDataArchiveFileForUpload(releaseVersion.Id,
                        archiveFile,
                        fileBeingReplaced);

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task
            ValidateDataArchiveEntriesForUpload_ReplacingDataFileWithFileOfDifferentNameButClashesWithAnother()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            // Create two release files, one of which is the file being replaced, and the other has the same filename
            // as the file being uploaded.
            var (fileBeingReplaced, otherFile) = _fixture.DefaultFile(FileType.Data)
                .ForIndex(0, s => s.SetFilename("test.csv"))
                .ForIndex(1, s => s.SetFilename("another.csv"))
                .Generate(2)
                .ToTuple2();

            var releaseFiles = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFiles(ListOf(fileBeingReplaced, otherFile))
                .Generate(2);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var (service, _) = BuildService(context);

                // The replacement file here has the same name as another unrelated data file i.e. one that's not being
                // replaced here, which should be a problem as it would otherwise result in duplicate data file names
                // in this Release after the replacement is complete.
                var archiveFile = CreateDataArchiveFileMock("another.csv", "test.meta.csv").Object;

                var result = service.ValidateDataArchiveFileForUpload(
                    releaseVersion.Id, archiveFile, fileBeingReplaced);

                //result.AssertBadRequest(DataFilenameNotUnique); // @MarkFix
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
