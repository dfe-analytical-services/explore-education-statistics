#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
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
            await using var context = InMemoryContentDbContext();
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

        [Fact]
        public async Task ValidateDataFilesForUpload_Valid_Replacement()
        {
            var releaseVersionId = Guid.NewGuid();

            var toBeReplacedReleaseFile = new ReleaseFile
            {
                ReleaseVersionId = releaseVersionId,
                Name = "Data set name",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test.csv",
                },
            };

            var toBeReplacedReleaseMetaFile = new ReleaseFile
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
                context.ReleaseFiles.AddRange(toBeReplacedReleaseFile, toBeReplacedReleaseMetaFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var (service, fileTypeService) = BuildService(context);

                // NOTE: Not necessary, but in this test case new files have same name as the files they're replacing
                var dataFile = CreateFormFileMock(toBeReplacedReleaseFile.File.Filename).Object;
                var metaFile = CreateFormFileMock(toBeReplacedReleaseMetaFile.File.Filename).Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var result = await service.ValidateDataSetFilesForUpload(
                    Guid.NewGuid(),
                    toBeReplacedReleaseFile.Name,
                    dataFile.FileName,
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    metaFile.FileName,
                    metaFile.Length,
                    metaFile.OpenReadStream(),
                    toBeReplacedReleaseFile.File);

                VerifyAllMocks(fileTypeService);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataSetFileNameCannotBeEmpty()
        {
            await using var context = InMemoryContentDbContext();
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "",
                dataFile.FileName,
                dataFile.Length,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.Length,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                new ErrorViewModel
                {
                    Code = ValidationMessages.DataSetFileNameCannotBeEmpty.Code,
                    Message = ValidationMessages.DataSetFileNameCannotBeEmpty.Message,
                },
            ]);
        }

        [Theory]
        [InlineData("test/")]
        [InlineData("test&")]
        [InlineData($"test\0")]
        public async Task ValidateDataFilesForUpload_DataSetFileNameCannotContainSpecialCharacters(string dataSetFileName)
        {
            await using var context = InMemoryContentDbContext();
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                dataSetFileName,
                dataFile.FileName,
                dataFile.Length,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.Length,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorDataSetFileNameShouldNotContainSpecialCharacters(dataSetFileName),
            ]);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataSetFileNameShouldBeUnique()
        {
            var releaseFile = new ReleaseFile
            {
                Name = "Used data set file name",
                ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
                File = new File { Type = FileType.Data, },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
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

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseFile.ReleaseVersionId,
                    "Used data set file name",
                    dataFile.FileName,
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    metaFile.FileName,
                    metaFile.Length,
                    metaFile.OpenReadStream());
                VerifyAllMocks(fileTypeService);

                AssertHasErrors(errors, [
                    ValidationMessages.GenerateErrorDataSetFileNamesShouldBeUnique("Used data set file name"),
                ]);
            }
        }

        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFilesCannotHaveSameName (I think this is necessary)
        // @MarkFix ValidateDataSetFilesForUpload_DataAndMetaFilesCannotContainSpecialCharacters

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFilesHaveCorrectSuffix()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contentDbContextId);
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csvv").Object;
            var metaFile = CreateFormFileMock("test.metaa.csv").Object;

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set file name",
                dataFile.FileName,
                dataFile.Length,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.Length,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorFilenameMustEndDotCsv("test.csvv"),
                ValidationMessages.GenerateErrorMetaFilenameMustEndDotMetaDotCsv("test.metaa.csv"),
            ]);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFilesNamesTooLong()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contentDbContextId);
            var (service, fileTypeService) = BuildService(context);

            var filename = "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooongfilename";

            var dataFile = CreateFormFileMock($"{filename}.csv").Object;
            var metaFile = CreateFormFileMock($"{filename}.meta.csv").Object;

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set file name",
                dataFile.FileName,
                dataFile.Length,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.Length,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorFileNameTooLong($"{filename}.csv",
                    FileUploadsValidatorService.MaxFilenameSize),
                ValidationMessages.GenerateErrorFileNameTooLong($"{filename}.meta.csv",
                    FileUploadsValidatorService.MaxFilenameSize),
            ]);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataFileNamesNotUnique()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion()
                .Generate();

            var releaseFile =_fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile()
                    .WithType(FileType.Data)
                    .WithFilename("test.csv"))
                .Generate();

            var releaseFileMeta =_fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile()
                    .WithType(Metadata)
                    .WithFilename("test.meta.csv"))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.ReleaseFiles.AddRange(releaseFile, releaseFileMeta);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
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

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    "Data set file name",
                    dataFile.FileName,
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    metaFile.FileName,
                    metaFile.Length,
                    metaFile.OpenReadStream());
                VerifyAllMocks(fileTypeService);

                AssertHasErrors(errors, [
                    ValidationMessages.GenerateErrorFilenameNotUnique(
                        releaseFile.File.Filename, releaseFile.File.Type),
                    // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
                    // zips, so meta files won't be included in the same directory by name and thereby cannot clash.
                ]);
            }
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataAndMetaFilesShouldNotBeSizeZero()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contentDbContextId);
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csv", 0).Object;
            var metaFile = CreateFormFileMock("test.meta.csv", 0).Object;

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set file name",
                dataFile.FileName,
                dataFile.Length,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.Length,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorFileSizeMustNotBeZero(dataFile.FileName),
                ValidationMessages.GenerateErrorFileSizeMustNotBeZero(metaFile.FileName),
            ]);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_DataAndMetaFilesShouldBeValidCsvFiles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contentDbContextId);
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            fileTypeService
                .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                .ReturnsAsync(false);
            fileTypeService
                .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                .ReturnsAsync(false);

            var errors = await service.ValidateDataSetFilesForUpload(
                Guid.NewGuid(),
                "Data set file name",
                dataFile.FileName,
                dataFile.Length,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.Length,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorMustBeCsvFile(dataFile.FileName),
                ValidationMessages.GenerateErrorMustBeCsvFile(metaFile.FileName),
            ]);
        }

        [Fact]
        public async Task ValidateDataFilesForUpload_Replacement_FilesNamesNotUnique()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion()
                .Generate();

            var otherSubjectId = Guid.NewGuid();

            var otherReleaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile()
                    .WithType(FileType.Data)
                    .WithFilename("usedfilename.csv")
                    .WithSubjectId(otherSubjectId))
                .Generate();

            var otherMetaReleaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile()
                    .WithType(Metadata)
                    .WithFilename("usedfilename.meta.csv")
                    .WithSubjectId(otherSubjectId))
                .Generate();

            var toBeReplacedSubjectId = Guid.NewGuid();

            var toBeReplacedReleaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithName("Data set name")
                .WithFile(_fixture.DefaultFile()
                    .WithType(FileType.Data)
                    .WithFilename("test.csv")
                    .WithSubjectId(toBeReplacedSubjectId))
                .Generate();

            var toBeReplacedMetaReleaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_fixture.DefaultFile()
                    .WithType(Metadata)
                    .WithFilename("test.meta.csv")
                    .WithSubjectId(toBeReplacedSubjectId))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.ReleaseFiles.AddRange(
                    otherReleaseFile, otherMetaReleaseFile,
                    toBeReplacedReleaseFile, toBeReplacedMetaReleaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {

                var (service, fileTypeService) = BuildService(context);

                // New files have different filenames to files being replaced, but same name as a different existing subject
                var dataFile = CreateFormFileMock("usedfilename.csv").Object;
                var metaFile = CreateFormFileMock("usedfilename.meta.csv").Object;

                fileTypeService
                    .Setup(s => s.IsValidCsvFile(dataFile.OpenReadStream()))
                    .ReturnsAsync(true);
                fileTypeService
                    .Setup(s => s.IsValidCsvFile(metaFile.OpenReadStream()))
                    .ReturnsAsync(true);

                var errors = await service.ValidateDataSetFilesForUpload(
                    releaseVersion.Id,
                    toBeReplacedReleaseFile.Name!,
                    "usedfilename.csv",
                    dataFile.Length,
                    dataFile.OpenReadStream(),
                    "usedfilename.meta.csv",
                    metaFile.Length,
                    metaFile.OpenReadStream(),
                    toBeReplacedReleaseFile.File);

                VerifyAllMocks(fileTypeService);

                AssertHasErrors(errors, [
                    ValidationMessages.GenerateErrorFilenameNotUnique("usedfilename.csv", FileType.Data),
                    // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
                    // zips, so meta files won't be included in the same directory by name and thereby cannot clash.
                ]);
            }
        }

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
