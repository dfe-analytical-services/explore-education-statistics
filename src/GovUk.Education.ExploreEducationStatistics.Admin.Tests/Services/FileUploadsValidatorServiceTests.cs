#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using AngleSharp.Common;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.ResponseErrorAssertUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileUploadsValidatorServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task ValidateDataFilesForUpload_Valid()
        {
            await using (var context = InMemoryContentDbContext())
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
                    "DataSetFile name",
                    dataFile.FileName,
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    metaFile.FileName,
                    metaFile.Length,
                    metaFile.OpenReadStream());
                VerifyAllMocks(fileTypeService);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_Valid_Replacement()
        {
            var releaseVersionId = Guid.NewGuid();

            var replacingReleaseFile = new ReleaseFile
            {
                ReleaseVersionId = releaseVersionId,
                Name = "Data set name",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test.csv",
                },
            };

            var replacingReleaseMetaFile = new ReleaseFile
            {
                ReleaseVersionId = releaseVersionId,
                File = new File
                {
                    Type = Metadata,
                    Filename = "test.meta.csv",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.ReleaseFiles.AddRange(replacingReleaseFile, replacingReleaseMetaFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateFormFileMock(replacingReleaseFile.File.Filename).Object;
                var metaFile = CreateFormFileMock(replacingReleaseMetaFile.File.Filename).Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    Guid.NewGuid(),
                    replacingReleaseFile.Name,
                    dataFile.FileName,
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    metaFile.FileName,
                    metaFile.Length,
                    metaFile.OpenReadStream(),
                    replacingReleaseFile.File);

                VerifyAllMocks(fileTypeService);

                Assert.Empty(result);
            }
        }

        // @MarkFix ValidateDataSetFilesForUpload_DataSetFileNameCannotBeEmpty
        // @MarkFix ValidateDataSetFilesForUpload_DataSetFileNameCannotContainSpecialCharacters
        // @MarkFix ValidateDataSetFilesForUpload_DataSetFileNameShouldBeUnique
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFilesCannotHaveSameName (I think this is necessary)
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFilesCannotContainSpecialCharacters
        // @MarkFix ValidateDataSetFilesForUpload_DataFileMustEndDotCsv
        // @MarkFix ValidateDataSetFilesForUpload_MetaFileMustEndDotMetaDotCsv
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFileNamesTooLong
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFileNamesNotUnique
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFilesShouldNotBeSizeZero
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFilesShouldBeValidCsvFile

        [Fact]
        public async Task ValidateDataFilesForUpload_Replacement_FilesnamesNotUnique()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
            };

            var otherReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Name = "Data set name",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "usedfilename.csv",
                    SubjectId = Guid.NewGuid(),
                },
            };

            var otherMetaReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Type = Metadata,
                    Filename = "usedfilename.meta.csv",
                    SubjectId = otherReleaseFile.File.SubjectId,
                },
            };

            var replacingReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Name = "Data set name",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test.csv",
                    SubjectId = Guid.NewGuid(),
                },
            };

            var replacingReleaseMetaFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Type = Metadata,
                    Filename = "test.meta.csv",
                    SubjectId = replacingReleaseFile.File.SubjectId,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.ReleaseFiles.AddRange(
                    otherReleaseFile, otherMetaReleaseFile,
                    replacingReleaseFile, replacingReleaseMetaFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {

                var (service, fileTypeService) = BuildService(context);

                var dataFile = CreateFormFileMock(replacingReleaseFile.File.Filename).Object;
                var metaFile = CreateFormFileMock(replacingReleaseMetaFile.File.Filename).Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    replacingReleaseFile.Name,
                    "usedfilename.csv",
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    "usedfilename.meta.csv",
                    metaFile.Length,
                    metaFile.OpenReadStream(),
                    replacingReleaseFile.File);

                VerifyAllMocks(fileTypeService);

                AssertHasErrors(errors, [
                    ValidationMessages.GenerateErrorFilenameNotUnique("usedfilename.csv", FileType.Data),
                    ValidationMessages.GenerateErrorFilenameNotUnique("usedfilename.meta.csv", Metadata),
                ]);
            }
        }

        [Fact]
        public async Task ValidateSubjectName_SubjectNameNotUnique() // @MarkFix
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
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(dataReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var (service, _) = BuildService(contentDbContext);

                //var result = service.ValidateDataSetName(releaseVersion.Id, "Subject Title", false);

                //result.AssertBadRequest(SubjectTitleMustBeUnique);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_IFormFiles_DataFileIsEmpty() // @MarkFix
        {
            var dataFile = CreateFormFileMock("test.csv", 0).Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            await using var context = InMemoryContentDbContext();
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
            // Assert something blah here
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileIsEmpty() // @MarkFix
        {
            await using (var context = InMemoryContentDbContext())
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

                //result.AssertBadRequest(MetadataFileCannotBeEmpty);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNotCsv() // @MarkFix
        {
            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            await using var context = InMemoryContentDbContext();
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

            //result.AssertBadRequest(DataFileMustBeCsvFile);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_MetadataFileNotCsv() // @MarkFix
        {
            await using (var context = InMemoryContentDbContext())
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

                //result.AssertBadRequest(MetaFileMustBeCsvFile);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DuplicateDataFile() // @MarkFix
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile(FileType.Data)
                    .WithFilename("test.csv"))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
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

                //result.AssertBadRequest(DataFilenameNotUnique);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_ReplacingDataFileWithFileOfSameName() // @MarkFix
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

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
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

                //result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_ReplacingDataFileWithFileOfDifferentNameButClashesWithAnother() // @MarkFix
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

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
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
                    releaseFiles.GetItemByIndex(0).Name,
                    dataFile, metaFile, fileBeingReplaced);

                //result.AssertBadRequest(DataFilenameNotUnique);
            }
        }

        [Fact]
        public async Task ValidateFileForUpload_MetadataFileNamesCanBeDuplicated() // @MarkFix
        {
            var releaseVersionId = Guid.NewGuid();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
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

            await using (var context = InMemoryContentDbContext(contextId))
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

                //result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_Valid() // @MarkFix possible removal of this stuff has there is one method now
        {
            await using (var context = InMemoryContentDbContext())
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

                //result.AssertRight();
            }
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_DataFileNotCsv() // @MarkFix removal prospect
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(false);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var archiveFile = CreateArchiveDataSet("Data set name", "test.txt", "test.meta.csv");

            var result = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(DataFileMustBeCsvFile);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileNotCsv() // @MarkFix removal prospect
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(false);

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var archiveFile = CreateArchiveDataSet("Data set name", "test.csv", "test.meta.txt");

            var result = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetaFileMustBeCsvFile);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIncorrectlyNamed() // @MarkFix removal prospect
        {
            var archiveFile = CreateArchiveDataSet("Data set name", "test.csv", "meta.csv");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(false);

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetaFileIsIncorrectlyNamed);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFilenameTooLong() // @MarkFix
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
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

            //result.AssertBadRequest(DataFilenameTooLong);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetaFilenameTooLong() // @MarkFix Removal prospect
        {
            var archiveFile = CreateArchiveDataSet("Data set name", "test.csv",
                    "LoremipsumdolorsitametconsecteturadipiscingelitInsitametelitaccumsanbibendumlacusutmattismaurisCrasvehiculaaccumsaneratidelementumaugueposuereatNuncege.meta.csv");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetaFilenameTooLong);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileIsEmpty() // @MarkFix
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

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

            //result.AssertBadRequest(DataFileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateDataArchiveEntriesForUpload_MetadataFileIsEmpty() // @MarkFix
        {
            var archiveFile = CreateArchiveDataSet(
                "Data set name",
                "test.csv",
                "test.meta.csv",
                dataFileSize: 1024,
                metaFileSize: 0);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            var fileTypeService = new Mock<IFileTypeService>(Strict);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            fileTypeService.Setup(mock => mock
                .IsValidCsvFile(It.IsAny<Stream>()))
                .ReturnsAsync(true);

            var (service, _) = BuildService(
                contentDbContext,
                fileTypeService: fileTypeService.Object);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                archiveFile,
                Mock.Of<Stream>(),
                Mock.Of<Stream>());

            //result.AssertBadRequest(MetadataFileCannotBeEmpty);
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

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
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

                //result.AssertBadRequest(DataFilenameNotUnique);
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

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
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

                // The replacement file here has the same name as the one it is replacing, so this should be ok.
                var archiveFile = CreateArchiveDataSet(releaseFile.Name, "test.csv", "test.meta.csv");

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    archiveFile,
                    Mock.Of<Stream>(),
                    Mock.Of<Stream>(),
                    fileBeingReplaced);

                //result.AssertRight();
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

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
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

                //result.AssertBadRequest(DataFilenameNotUnique);
            }
        }

        [Fact]
        public async Task ValidateFileForUpload_FileCannotBeEmpty() // @MarkFix
        {
            var file = CreateFormFileMock("test.csv", 0).Object;

            var (service, _) = BuildService();
            var result = await service.ValidateFileForUpload(file, Ancillary);
            result.AssertBadRequest(FileCannotBeEmpty);
        }

        [Fact]
        public async Task ValidateFileForUpload_ExceptionThrownForDataFileType() // @MarkFix
        {
            var file = CreateFormFileMock("test.csv").Object;

            var (service, _) = BuildService();
            await Assert.ThrowsAsync<ArgumentException>(() => service.ValidateFileForUpload(file, FileType.Data));
        }

        [Fact]
        public async Task ValidateFileForUpload_FileTypeIsValid() // @MarkFix
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
        public async Task ValidateFileForUpload_FileTypeIsInvalid() // @MarkFix
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
