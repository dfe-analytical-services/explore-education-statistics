#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.ValidationProblemViewModelTestExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataArchiveValidationServiceTests
    {
        [Fact]
        public async Task ValidateBulkDataArchiveFiles_Success_ReturnsArchiveSummary()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-valid.zip");

            var dataSetValidatorService = new Mock<IDataSetValidatorService>(Strict);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    dataSetValidatorService: dataSetValidatorService.Object);

                dataSetValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                        releaseVersionId,
                        It.IsAny<ArchiveDataSetFile>(),
                        It.IsAny<Stream>(),
                        It.IsAny<Stream>()))
                    .ReturnsAsync([]);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                var archiveDataSets = result.AssertRight();

                Assert.Equal(2, archiveDataSets.Count);

                Assert.Equal("First data set", archiveDataSets[0].Title);
                Assert.Equal("one.csv", archiveDataSets[0].DataFilename);
                Assert.Equal("one.meta.csv", archiveDataSets[0].MetaFilename);
                Assert.Equal(696, archiveDataSets[0].DataFileSize);
                Assert.Equal(210, archiveDataSets[0].MetaFileSize);
                Assert.Null(archiveDataSets[0].ReplacingFile);

                Assert.Equal("Second data set", archiveDataSets[1].Title);
                Assert.Equal("two.csv", archiveDataSets[1].DataFilename);
                Assert.Equal("two.meta.csv", archiveDataSets[1].MetaFilename);
                Assert.Equal(2085, archiveDataSets[1].DataFileSize);
                Assert.Equal(318, archiveDataSets[1].MetaFileSize);
                Assert.Null(archiveDataSets[1].ReplacingFile);
            }

            VerifyAllMocks(dataSetValidatorService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_WithFileReplacement_ReturnsArchiveSummary()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-valid.zip");

            var dataSetValidatorService = new Mock<IDataSetValidatorService>(Strict);

            var releaseFile = new ReleaseFile
            {
                Name = "First data set",
                ReleaseVersion = new ReleaseVersion
                {
                    Id = releaseVersionId,
                },
                File = new Content.Model.File
                {
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid(),
                    Filename = "one.csv",
                    ContentLength = 1024,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    dataSetValidatorService: dataSetValidatorService.Object);

                dataSetValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                        releaseVersionId,
                        It.IsAny<ArchiveDataSetFile>(),
                        It.IsAny<Stream>(),
                        It.IsAny<Stream>()))
                    .ReturnsAsync([]);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                var archiveDataSets = result.AssertRight();

                Assert.Equal(2, archiveDataSets.Count);

                Assert.Equal("First data set", archiveDataSets[0].Title);
                Assert.Equal("one.csv", archiveDataSets[0].DataFilename);
                Assert.Equal("one.meta.csv", archiveDataSets[0].MetaFilename);
                Assert.Equal(696, archiveDataSets[0].DataFileSize);
                Assert.Equal(210, archiveDataSets[0].MetaFileSize);
                Assert.Equal(releaseFile.FileId, archiveDataSets[0].ReplacingFile!.Id);

                Assert.Equal("Second data set", archiveDataSets[1].Title);
                Assert.Equal("two.csv", archiveDataSets[1].DataFilename);
                Assert.Equal("two.meta.csv", archiveDataSets[1].MetaFilename);
                Assert.Equal(2085, archiveDataSets[1].DataFileSize);
                Assert.Equal(318, archiveDataSets[1].MetaFileSize);
                Assert.Null(archiveDataSets[1].ReplacingFile);
            }

            VerifyAllMocks(dataSetValidatorService);
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_IndexFileMissing_ReturnsValidationError()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-no-datasetnames-csv.zip");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        new ErrorViewModel
                        {
                            Code = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Code,
                            Message = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Message,
                        }
                    ]);
            }
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_IndexFileHasIncorrectHeaders_ReturnsValidationError()
        {
            var releaseVersionId = Guid.NewGuid();

            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-datasetnames-headers.zip");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        new ErrorViewModel
                        {
                            Code = ValidationMessages.DataSetNamesCsvIncorrectHeaders.Code,
                            Message = ValidationMessages.DataSetNamesCsvIncorrectHeaders.Message,
                        },
                    ]);
            }
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_ReferencedFilesNotFoundInArchive_ReturnsValidationError()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-files-not-found.zip");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorFileNotFoundInZip("one.meta.csv", FileType.Metadata),
                        ValidationMessages.GenerateErrorFileNotFoundInZip("two.csv", FileType.Data),
                    ]);
            }
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_DuplicateDataSetTitlesAndFileNames_ReturnsValidationError()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-duplicate-names.zip");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique("Duplicate title"),
                        ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique("one"),
                    ]);
            }
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_Fail_DataSetNamesCsvFilesnamesShouldNotEndDotCsv()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-filename-contains-extension.zip");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext);
                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldNotEndDotCsv("one.csv")
                    ]);
            }
        }

        [Fact]
        public async Task ValidateBulkDataArchiveFiles_IndexFileMissingReferenceToArchiveFile_ReturnsValidationError()
        {
            var releaseVersionId = Guid.NewGuid();
            var archive = CreateFormFileFromResource("bulk-data-zip-invalid-unused-files.zip");

            var dataSetValidatorService = new Mock<IDataSetValidatorService>(Strict);

            dataSetValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
                    releaseVersionId,
                    It.IsAny<ArchiveDataSetFile>(),
                    It.IsAny<Stream>(),
                    It.IsAny<Stream>()))
                .ReturnsAsync([]);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupDataArchiveValidationService(
                    contentDbContext: contentDbContext,
                    dataSetValidatorService: dataSetValidatorService.Object);

                var result = await service.ValidateBulkDataArchiveFiles(
                    releaseVersionId,
                    archive);

                result
                    .AssertLeft()
                    .AssertBadRequestWithValidationErrors([
                        ValidationMessages.GenerateErrorZipContainsUnusedFiles(["two.csv", "two.meta.csv"]),
                    ]);
            }

            VerifyAllMocks(dataSetValidatorService);
        }

        [Fact]
        public async Task IsValidZipFile_InvalidFileType_ReturnsValidationError()
        {
            // Arrange
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId);

            var filename = "test-data.csv";
            var archive = CreateFormFileFromResource("test-data.csv", filename);

            var fileTypeService = new Mock<IFileTypeService>(Strict);
            fileTypeService
                .Setup(s => s.HasValidZipFileMeta(archive))
                .ReturnsAsync(false);

            var service = SetupDataArchiveValidationService(
                contentDbContext: contentDbContext,
                fileTypeService: fileTypeService.Object);

            // Act
            var result = await service.IsValidZipFile(archive);

            // Assert
            AssertHasErrors(result, [
                ValidationMessages.GenerateErrorMustBeZipFile(filename),
            ]);

            VerifyAllMocks(fileTypeService);
        }

        private static DataSetArchiveService SetupDataArchiveValidationService(
            ContentDbContext? contentDbContext = null,
            IFileTypeService? fileTypeService = null,
            IDataSetValidatorService? dataSetValidatorService = null)
        {
            return new DataSetArchiveService(
                contentDbContext ?? Mock.Of<ContentDbContext>(Strict),
                fileTypeService ?? Mock.Of<IFileTypeService>(Strict),
                dataSetValidatorService ?? Mock.Of<IDataSetValidatorService>(Strict)
            );
        }
    }
}
