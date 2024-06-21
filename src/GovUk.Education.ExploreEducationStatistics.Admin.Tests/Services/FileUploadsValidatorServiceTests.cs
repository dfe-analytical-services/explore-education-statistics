#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using AngleSharp.Common;
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

            var result = service.ValidateDataSetName(Guid.NewGuid(), "Subject & Title", false);

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

                var result = service.ValidateDataSetName(releaseVersion.Id, "Subject Title", false);

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
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    Guid.NewGuid(),
                    "Data set name",
                    dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_IFormFiles_DataFileIsEmpty()
        {
            var dataFile = CreateFormFileMock("test.csv", 0).Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            await using var context = InMemoryApplicationDbContext();
            var (service, fileTypeService) = BuildService(context);

            fileTypeService.Setup(mock => mock.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService.Setup(mock => mock.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var results = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set name",
                dataFile, metaFile);

            var error = Assert.Single(results);
            // Assert something blah here // @MarkFix
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty()
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateFormFileMock("test.csv").Object;
                var metaFile = CreateFormFileMock("test.meta.csv", 0).Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    Guid.NewGuid(),
                    "Data set name",
                    dataFile, metaFile);

                //result.AssertBadRequest(MetadataFileCannotBeEmpty); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNotCsv()
        {
            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            await using var context = InMemoryApplicationDbContext();
            var (service, fileTypeService) = BuildService(context);

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(false);

            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var result = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set name",
                dataFile, metaFile);
            VerifyAllMocks(fileTypeService);

            //result.AssertBadRequest(DataFileMustBeCsvFile); // @MarkFix
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
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(false);

                var result = await service.ValidateDataSetFilesForUpload(
                    Guid.NewGuid(),
                    "Data set name",
                    dataFile, metaFile);
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
                    .WithFilename("test.csv"))
                .Generate();

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
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    releaseFile.Name,
                    dataFile, metaFile);

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
                .WithFile(fileBeingReplaced)
                .Generate();

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
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    releaseFile.Name,
                    dataFile, metaFile, fileBeingReplaced);
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
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(() => true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(() => true);

                var result = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    releaseFiles.GetItemByIndex(0).Name, // @MarkFix fix
                    dataFile, metaFile, fileBeingReplaced);

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
                    Name = "Data set name",
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
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    "Data set name",
                    dataFile, metaFile);
                VerifyAllMocks(fileTypeService);

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_Valid() // @MarkFix possible removal of this stuff has there is one method now
        {
            await using (var context = InMemoryApplicationDbContext())
            {
                var fileTypeService = new Mock<IFileTypeService>(Strict);

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true); // @MarkFix correct?

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true); // @MarkFix correct?

                var (service, _) = BuildService(
                    context,
                    fileTypeService: fileTypeService.Object);

                //var archiveFile = CreateArchiveDataSet("Data set name", "test.csv", "test.meta.csv");
                var archiveFile = new ArchiveDataSetFile(
                    "Data set name",
                    "test.csv",
                    1024,
                    "test.meta.csv",
                    1024);

                var result = await service.ValidateDataSetFilesForUpload(
                    Guid.NewGuid(),
                    archiveFile,
                    Mock.Of<Stream>(),
                    Mock.Of<Stream>());

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFileNotCsv() // @MarkFix removal prospect
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(false); // @MarkFix correct?

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var archiveFile = CreateArchiveDataSet("Data set name", "test.txt", "test.meta.csv");

            var result = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(DataFileMustBeCsvFile); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileNotCsv() // @MarkFix removal prospect
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(false); // @MarkFix correct?

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var archiveFile = CreateArchiveDataSet("Data set name", "test.csv", "test.meta.txt");

            var result = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetaFileMustBeCsvFile); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIncorrectlyNamed() // @MarkFix removal prospect
        {
            var archiveFile = CreateArchiveDataSet("Data set name", "test.csv", "meta.csv");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(false); // @MarkFix correct?

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetaFileIsIncorrectlyNamed); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFilenameTooLong()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var (service, _) = BuildService(contentDbContext);

            var result = service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set name",
                "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.csv",
                1024,
                Mock.Of<Stream>(),
                "dataset.meta.csv",
                1024,
                Mock.Of<Stream>());

            //result.AssertBadRequest(DataFilenameTooLong); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetaFilenameTooLong() // @MarkFix Removal prospect
        {
            var archiveFile = CreateArchiveDataSet("Data set name", "test.csv",
                    "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.meta.csv");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetaFilenameTooLong); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileIsEmpty()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var result = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set name",
                "test.csv", 0,
                Mock.Of<Stream>(),
                "test.meta.csv", 1024,
                Mock.Of<Stream>());

            //result.AssertBadRequest(DataFileCannotBeEmpty); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIsEmpty()
        {
            var archiveFile = CreateArchiveDataSet(
                "Data set name",
                "test.csv",
                "test.meta.csv",
                dataFileSize: 1024,
                metaFileSize: 0);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true); // @MarkFix correct?

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetadataFileCannotBeEmpty); // @MarkFix
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DuplicateDataFile() // @MarkFix removal prospect
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile(FileType.Data)
                    .WithFilename("test.csv"))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var fileTypeService = new Mock<IFileTypeService>(Strict);

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true);

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true);

                var (service, _) = BuildService(
                    context,
                    fileTypeService: fileTypeService.Object);

                var archiveFile = CreateArchiveDataSet(releaseFile.Name, "test.csv", "test.meta.csv");

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    archiveFile,
                    Mock.Of<Stream>(),
                    Mock.Of<Stream>());

                //result.AssertBadRequest(DataFilenameNotUnique); // @MarkFix
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_ReplacingDataFileWithFileOfSameName() // @MarkFix removal prospect
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
                var fileTypeService = new Mock<IFileTypeService>(Strict);

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true); // @MarkFix correct?

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true); // @MarkFix correct?

                var (service, _) = BuildService(
                    context,
                    fileTypeService: fileTypeService.Object);

                // The replacement file here has the same name as the one it is replacing, so this should be ok.
                var archiveFile = CreateArchiveDataSet(releaseFile.Name, "test.csv", "test.meta.csv");

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    archiveFile,
                    Mock.Of<Stream>(),
                    Mock.Of<Stream>(),
                    fileBeingReplaced);

                //result.AssertRight(); // @MarkFix
            }
        }

        [Fact]
        public async Task
            ValidateDataArchiveEntriesForUpload_ReplacingDataFileWithFileOfDifferentNameButClashesWithAnother() // @MarkFix removal prospect
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
                var fileTypeService = new Mock<IFileTypeService>(Strict);

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true); // @MarkFix correct?

                fileTypeService.Setup(mock => mock.
                        IsValidCsvFile(It.IsAny<Stream>()))
                    .ReturnsAsync(true); // @MarkFix correct?

                var (service, _) = BuildService(
                    context,
                    fileTypeService: fileTypeService.Object);

                // The replacement file here has the same name as another unrelated data file i.e. one that's not being
                // replaced here, which should be a problem as it would otherwise result in duplicate data file names
                // in this Release after the replacement is complete.
                var archiveFile = CreateArchiveDataSet("Data set name", "another.csv", "test.meta.csv");

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    archiveFile,
                    Mock.Of<Stream>(),
                    Mock.Of<Stream>(),
                    fileBeingReplaced);

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
