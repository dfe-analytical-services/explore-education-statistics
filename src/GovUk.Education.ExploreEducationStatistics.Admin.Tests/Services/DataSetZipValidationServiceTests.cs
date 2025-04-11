//#nullable enable
//using GovUk.Education.ExploreEducationStatistics.Admin.Models;
//using GovUk.Education.ExploreEducationStatistics.Admin.Services;
//using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
//using GovUk.Education.ExploreEducationStatistics.Common.Model;
//using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
//using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
//using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
//using GovUk.Education.ExploreEducationStatistics.Content.Model;
//using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
//using Moq;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.ValidationProblemViewModelTestExtensions;
//using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
//using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
//using static Moq.MockBehavior;
//using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

//namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
//{
//    public class DataSetZipValidationServiceTests
//    {
//        [Fact]
//        public async Task ValidateBulkDataZipFiles_Success_ReturnsDataSetSummary()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-valid.zip");

//            var dataSetValidatorService = new Mock<IDataSetValidatorService>(Strict);

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext,
//                    dataSetValidatorService: dataSetValidatorService.Object);

//                dataSetValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
//                        releaseVersionId,
//                        It.IsAny<ZipDataSetFile>(),
//                        It.IsAny<Stream>(),
//                        It.IsAny<Stream>()))
//                    .ReturnsAsync([]);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                var zippedDataSets = result.AssertRight();

//                Assert.Equal(2, zippedDataSets.Count);

//                Assert.Equal("First data set", zippedDataSets[0].Title);
//                Assert.Equal("one.csv", zippedDataSets[0].DataFilename);
//                Assert.Equal("one.meta.csv", zippedDataSets[0].MetaFilename);
//                Assert.Equal(696, zippedDataSets[0].DataFileSize);
//                Assert.Equal(210, zippedDataSets[0].MetaFileSize);
//                Assert.Null(zippedDataSets[0].ReplacingFile);

//                Assert.Equal("Second data set", zippedDataSets[1].Title);
//                Assert.Equal("two.csv", zippedDataSets[1].DataFilename);
//                Assert.Equal("two.meta.csv", zippedDataSets[1].MetaFilename);
//                Assert.Equal(2085, zippedDataSets[1].DataFileSize);
//                Assert.Equal(318, zippedDataSets[1].MetaFileSize);
//                Assert.Null(zippedDataSets[1].ReplacingFile);
//            }

//            VerifyAllMocks(dataSetValidatorService);
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_WithFileReplacement_ReturnsDataSetSummary()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-valid.zip");

//            var dataSetValidatorService = new Mock<IDataSetValidatorService>(Strict);

//            var releaseFile = new ReleaseFile
//            {
//                Name = "First data set",
//                ReleaseVersion = new ReleaseVersion
//                {
//                    Id = releaseVersionId,
//                },
//                File = new Content.Model.File
//                {
//                    Type = FileType.Data,
//                    SubjectId = Guid.NewGuid(),
//                    Filename = "one.csv",
//                    ContentLength = 1024,
//                },
//            };

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                contentDbContext.ReleaseFiles.Add(releaseFile);
//                await contentDbContext.SaveChangesAsync();
//            }

//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext,
//                    dataSetValidatorService: dataSetValidatorService.Object);

//                dataSetValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
//                        releaseVersionId,
//                        It.IsAny<ZipDataSetFile>(),
//                        It.IsAny<Stream>(),
//                        It.IsAny<Stream>()))
//                    .ReturnsAsync([]);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                var zippedDataSets = result.AssertRight();

//                Assert.Equal(2, zippedDataSets.Count);

//                Assert.Equal("First data set", zippedDataSets[0].Title);
//                Assert.Equal("one.csv", zippedDataSets[0].DataFilename);
//                Assert.Equal("one.meta.csv", zippedDataSets[0].MetaFilename);
//                Assert.Equal(696, zippedDataSets[0].DataFileSize);
//                Assert.Equal(210, zippedDataSets[0].MetaFileSize);
//                Assert.Equal(releaseFile.FileId, zippedDataSets[0].ReplacingFile!.Id);

//                Assert.Equal("Second data set", zippedDataSets[1].Title);
//                Assert.Equal("two.csv", zippedDataSets[1].DataFilename);
//                Assert.Equal("two.meta.csv", zippedDataSets[1].MetaFilename);
//                Assert.Equal(2085, zippedDataSets[1].DataFileSize);
//                Assert.Equal(318, zippedDataSets[1].MetaFileSize);
//                Assert.Null(zippedDataSets[1].ReplacingFile);
//            }

//            VerifyAllMocks(dataSetValidatorService);
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_IndexFileMissing_ReturnsValidationError()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-invalid-no-datasetnames-csv.zip");

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                result
//                    .AssertLeft()
//                    .AssertBadRequestWithValidationErrors([
//                        new ErrorViewModel
//                        {
//                            Code = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Code,
//                            Message = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Message,
//                        }
//                    ]);
//            }
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_IndexFileHasIncorrectHeaders_ReturnsValidationError()
//        {
//            var releaseVersionId = Guid.NewGuid();

//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-invalid-datasetnames-headers.zip");

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                result
//                    .AssertLeft()
//                    .AssertBadRequestWithValidationErrors([
//                        new ErrorViewModel
//                        {
//                            Code = ValidationMessages.DataSetNamesCsvIncorrectHeaders.Code,
//                            Message = ValidationMessages.DataSetNamesCsvIncorrectHeaders.Message,
//                        },
//                    ]);
//            }
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_ReferencedFilesNotFoundInZipFile_ReturnsValidationError()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-invalid-files-not-found.zip");

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                result
//                    .AssertLeft()
//                    .AssertBadRequestWithValidationErrors([
//                        ValidationMessages.GenerateErrorFileNotFoundInZip("one.meta.csv", FileType.Metadata),
//                        ValidationMessages.GenerateErrorFileNotFoundInZip("two.csv", FileType.Data),
//                    ]);
//            }
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_DuplicateDataSetTitlesAndFileNames_ReturnsValidationError()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-invalid-duplicate-names.zip");

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                result
//                    .AssertLeft()
//                    .AssertBadRequestWithValidationErrors([
//                        ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique("Duplicate title"),
//                        ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique("one"),
//                    ]);
//            }
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_Fail_DataSetNamesCsvFilesnamesShouldNotEndDotCsv()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-invalid-filename-contains-extension.zip");

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext);
//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                result
//                    .AssertLeft()
//                    .AssertBadRequestWithValidationErrors([
//                        ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldNotEndDotCsv("one.csv")
//                    ]);
//            }
//        }

//        [Fact]
//        public async Task ValidateBulkDataZipFiles_IndexFileMissingReferenceToZipFile_ReturnsValidationError()
//        {
//            var releaseVersionId = Guid.NewGuid();
//            var zipFormFile = CreateFormFileFromResource("bulk-data-zip-invalid-unused-files.zip");

//            var dataSetValidatorService = new Mock<IDataSetValidatorService>(Strict);

//            dataSetValidatorService.Setup(mock => mock.ValidateDataSetFilesForUpload(
//                    releaseVersionId,
//                    It.IsAny<ZipDataSetFile>(),
//                    It.IsAny<Stream>(),
//                    It.IsAny<Stream>()))
//                .ReturnsAsync([]);

//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
//            {
//                var service = SetupDataZipValidationService(
//                    contentDbContext: contentDbContext,
//                    dataSetValidatorService: dataSetValidatorService.Object);

//                var result = await service.ValidateBulkDataZipFiles(
//                    releaseVersionId,
//                    zipFormFile);

//                result
//                    .AssertLeft()
//                    .AssertBadRequestWithValidationErrors([
//                        ValidationMessages.GenerateErrorZipContainsUnusedFiles(["two.csv", "two.meta.csv"]),
//                    ]);
//            }

//            VerifyAllMocks(dataSetValidatorService);
//        }

//        [Fact]
//        public async Task IsValidZipFile_InvalidFileType_ReturnsValidationError()
//        {
//            // Arrange
//            var contentDbContextId = Guid.NewGuid().ToString();
//            await using var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId);

//            var filename = "test-data.csv";
//            var zipFormFile = CreateFormFileFromResource("test-data.csv", filename);

//            var fileTypeService = new Mock<IFileTypeService>(Strict);
//            fileTypeService
//                .Setup(s => s.HasValidZipFileMeta(zipFormFile))
//                .ReturnsAsync(false);

//            var service = SetupDataZipValidationService(
//                contentDbContext: contentDbContext,
//                fileTypeService: fileTypeService.Object);

//            // Act
//            var result = await service.IsValidZipFile(zipFormFile);

//            // Assert
//            AssertHasErrors(result, [
//                ValidationMessages.GenerateErrorMustBeZipFile(filename),
//            ]);

//            VerifyAllMocks(fileTypeService);
//        }

//        private static DataSetZipValidationService SetupDataZipValidationService(
//            ContentDbContext? contentDbContext = null,
//            IFileTypeService? fileTypeService = null,
//            IDataSetValidatorService? dataSetValidatorService = null)
//        {
//            return new DataSetZipValidationService(
//                contentDbContext ?? Mock.Of<ContentDbContext>(Strict),
//                fileTypeService ?? Mock.Of<IFileTypeService>(Strict),
//                dataSetValidatorService ?? Mock.Of<IDataSetValidatorService>(Strict)
//            );
//        }
//    }
//}
