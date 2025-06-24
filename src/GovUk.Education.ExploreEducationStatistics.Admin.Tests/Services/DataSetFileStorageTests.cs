#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using LinqToDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Semver;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;
using DataSet = GovUk.Education.ExploreEducationStatistics.Admin.Models.DataSet;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetFileStorageTests
{
    private readonly DataFixture _fixture = new();

    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        Email = "test@test.com"
    };

    [Fact]
    public async Task UploadDataSet_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var subjectId = Guid.NewGuid();

        var dataImportService = new Mock<IDataImportService>(Strict);
        var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()))
            .Generate();

        releaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id))
            .Returns(Task.FromResult(subjectId));

        var dataFile = _fixture
            .DefaultFile()
            .WithFilename(dataFileName)
            .WithCreatedByUser(_user)
            .Generate();

        var metaFile = _fixture
            .DefaultFile()
            .WithFilename(metaFileName)
            .Generate();

        var releaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(dataFile)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(releaseFile);
            contentDbContext.Files.Add(metaFile);
            await contentDbContext.SaveChangesAsync();

            releaseDataFileRepository
                .Setup(mock => mock.Create(
                    releaseVersion.Id,
                    subjectId,
                    dataFileName,
                    434,
                    FileType.Data,
                    _user.Id,
                    dataSetName,
                    null,
                    null,
                    0))
                .Returns(Task.FromResult(dataFile));

            releaseDataFileRepository
                .Setup(mock => mock.Create(
                    releaseVersion.Id,
                    subjectId,
                    metaFileName,
                    157,
                    FileType.Metadata,
                    _user.Id,
                    null, null, null, 0))
                .Returns(Task.FromResult(metaFile));

            privateBlobStorageService
                .Setup(mock => mock.UploadStream(
                    It.IsAny<IBlobContainer>(),
                    It.IsAny<string>(),
                    It.IsAny<MemoryStream>(),
                    It.IsAny<string>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            dataImportService
               .Setup(s => s.Import(
                   It.IsAny<Guid>(),
                   It.IsAny<File>(),
                   It.IsAny<File>(),
                   null))
               .ReturnsAsync(new DataImport
               {
                   Status = QUEUED,
                   MetaFile = metaFile,
                   TotalRows = 123,
               });
        }

        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
        {
            EnableReplacementOfPublicApiDataSets = false
        });

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseDataFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataImportService: dataImportService.Object,
                releaseVersionRepository: releaseVersionRepository.Object,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                featureFlags: featureFlagOptions,
                addDefaultUser: false
            );

            var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
            var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

            var dataSet = new DataSet
            {
                Title = dataSetName,
                DataFile = dataSetFile,
                MetaFile = metaSetFile,
            };

            // Act
            var uploadSummary = await service.UploadDataSet(
                releaseVersion.Id,
                dataSet,
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(privateBlobStorageService, dataImportService, releaseVersionRepository, releaseDataFileRepository);

            Assert.True(uploadSummary.Id.HasValue);
            Assert.Equal(dataSetName, uploadSummary.Name);
            Assert.Equal(dataFileName, uploadSummary.FileName);
            Assert.Equal("csv", uploadSummary.Extension);
            Assert.True(uploadSummary.MetaFileId.HasValue);
            Assert.Equal(metaFileName, uploadSummary.MetaFileName);
            Assert.Equal(123, uploadSummary.Rows);
            Assert.Equal("466 Kb", uploadSummary.Size);
            uploadSummary.Created.AssertUtcNow();
            Assert.Equal("test@test.com", uploadSummary.UserName);
            Assert.Equal(QUEUED, uploadSummary.Status);
            Assert.Null(uploadSummary.PublicApiDataSetId);
            Assert.Null(uploadSummary.PublicApiDataSetVersion);
        }
    }

    [Fact]
    public async Task UploadDataSetsToTemporaryStorage_ReturnsUploadDetails()
    {
        // Arrange
        var dataSetName = "Test Data Set";

        var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSet = new DataSet
        {
            Title = dataSetName,
            DataFile = dataSetFile,
            MetaFile = metaSetFile,
        };

        await using var contentDbContext = InMemoryApplicationDbContext();
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var userService = new Mock<IUserService>(Strict);

        privateBlobStorageService
            .Setup(mock => mock.UploadStream(
                It.IsAny<IBlobContainer>(),
                It.IsAny<string>(),
                It.IsAny<MemoryStream>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(new UserProfileFromClaims(_user.Email, "Test", "Test"));

        var service = SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: privateBlobStorageService.Object,
            userService: userService.Object);

        // Act
        var result = await service.UploadDataSetsToTemporaryStorage(
            Guid.NewGuid(),
            [dataSet],
            cancellationToken: default);

        // Assert
        privateBlobStorageService.Verify();

        var uploadDetails = Assert.Single(result);
        Assert.Equal(dataSetName, uploadDetails.DataSetTitle);
        Assert.NotEqual(Guid.Empty, uploadDetails.DataFileId);
        Assert.Equal("test-data.csv", uploadDetails.DataFileName);
        Assert.Equal(434, uploadDetails.DataFileSizeInBytes);
        Assert.NotEqual(Guid.Empty, uploadDetails.MetaFileId);
        Assert.Equal("test-data.meta.csv", uploadDetails.MetaFileName);
        Assert.Equal(157, uploadDetails.MetaFileSizeInBytes);
        Assert.Equal(DataSetUploadStatus.SCREENING, uploadDetails.Status);
        Assert.Equal(_user.Email, uploadDetails.UploadedBy);
        Assert.Null(uploadDetails.ReplacingFileId);
    }

    [Fact]
    public async Task CreateOrReplaceExistingDataSetUpload_CreateNew_ReturnsUploadDetails()
    {
        // Arrange
        var dataSetUpload = new DataSetUploadMockBuilder().BuildEntity();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var dataSetUploadRepository = new Mock<IDataSetUploadRepository>(Strict);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseDataFileService(
                contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataSetUploadRepository: dataSetUploadRepository.Object);

            // Act
            await service.CreateOrReplaceExistingDataSetUpload(dataSetUpload.ReleaseVersionId, dataSetUpload, cancellationToken: default);
        }

        // Assert
        privateBlobStorageService.Verify(mock => mock.DeleteBlob(
            PrivateReleaseTempFiles,
            It.IsAny<string>()),
            Times.Never());

        dataSetUploadRepository.Verify(mock => mock.Delete(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
            Times.Never());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var result = await contentDbContext.DataSetUploads.FindAsync(dataSetUpload.Id);

            Assert.NotNull(result);
            Assert.Equivalent(dataSetUpload, result);
        }
    }

    [Fact]
    public async Task CreateOrReplaceExistingDataSetUpload_ReplaceExisting_ReturnsUploadDetails()
    {
        // Arrange
        var releaseVersionId = Guid.NewGuid();

        var existingDataSetUpload = new DataSetUploadMockBuilder()
            .WithReleaseVersionId(releaseVersionId)
            .WithFailingTests()
            .BuildEntity();

        var newDataSetUpload = new DataSetUploadMockBuilder()
            .WithReleaseVersionId(releaseVersionId)
            .BuildEntity();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var dataSetUploadRepository = new Mock<IDataSetUploadRepository>(Strict);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetUploads.Add(existingDataSetUpload);
            await contentDbContext.SaveChangesAsync();

            dataSetUploadRepository
                .Setup(mock => mock.Delete(
                    releaseVersionId,
                    existingDataSetUpload.Id,
                    It.IsAny<CancellationToken>()))
                .Callback(() => contentDbContext.DataSetUploads.Remove(existingDataSetUpload))
                .ReturnsAsync(Unit.Instance);

            var service = SetupReleaseDataFileService(
                contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataSetUploadRepository: dataSetUploadRepository.Object);

            // Act
            await service.CreateOrReplaceExistingDataSetUpload(releaseVersionId, newDataSetUpload, cancellationToken: default);
        }

        // Assert
        MockUtils.VerifyAllMocks(privateBlobStorageService, dataSetUploadRepository);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var allUploads = await contentDbContext.DataSetUploads.ToListAsync();
            var upload = Assert.Single(allUploads);

            Assert.NotNull(upload);
            Assert.Equivalent(newDataSetUpload, upload);
        }
    }

    [Fact]
    public async Task AddScreenerResultToUpload_UploadNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var contentDbContext = InMemoryApplicationDbContext();
        var service = SetupReleaseDataFileService(contentDbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await service.AddScreenerResultToUpload(Guid.NewGuid(), screenerResult: null!, cancellationToken: default));

        Assert.Equal("Sequence contains no elements", exception.Message);
    }

    [Theory]
    [InlineData(TestResult.PASS)]
    [InlineData(TestResult.WARNING)]
    [InlineData(TestResult.FAIL)]
    public async Task AddScreenerResultToUpload_WithTestResult_UpdatesStatusCorrectly(TestResult testResult)
    {
        // Arrange
        var builder = new DataSetUploadMockBuilder();

        var dataSetUpload = testResult switch
        {
            TestResult.PASS => builder.BuildEntity(),
            TestResult.WARNING => builder
                .WithWarningTests()
                .BuildEntity(),
            TestResult.FAIL => builder
                .WithFailingTests()
                .BuildEntity(),
            _ => throw new ArgumentOutOfRangeException(nameof(testResult))
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetUploads.Add(dataSetUpload);
            await contentDbContext.SaveChangesAsync();

            var service = SetupReleaseDataFileService(contentDbContext);

            // Act
            await service.AddScreenerResultToUpload(dataSetUpload.Id, dataSetUpload.ScreenerResult!, cancellationToken: default);
        }

        // Assert
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedDataSetUpload = await contentDbContext.DataSetUploads.FindAsync(dataSetUpload.Id);

            Assert.NotNull(updatedDataSetUpload);

            switch (testResult)
            {
                case TestResult.PASS:
                    Assert.Equal(DataSetUploadStatus.PENDING_IMPORT, updatedDataSetUpload.Status);
                    break;
                case TestResult.WARNING:
                    Assert.Equal(DataSetUploadStatus.PENDING_REVIEW, updatedDataSetUpload.Status);
                    break;
                case TestResult.FAIL:
                    Assert.Equal(DataSetUploadStatus.FAILED_SCREENING, updatedDataSetUpload.Status);
                    break;
                default:
                    break;
            }

            Assert.Equivalent(dataSetUpload.ScreenerResult, updatedDataSetUpload.ScreenerResult);
        }
    }

    [Fact]
    public async Task MoveDataSetsToPermanentStorage_WithReplacement_ReturnsReleaseFile()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var subjectId = Guid.NewGuid();

        var dataImportService = new Mock<IDataImportService>(Strict);
        var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()))
            .Generate();

        releaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id))
            .Returns(Task.FromResult(subjectId));

        var originalDataFile = _fixture
            .DefaultFile()
            .WithFilename(dataFileName)
            .WithType(FileType.Data)
            .Generate();

        var originalReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFile(originalDataFile)
            .Generate();

        var newDataFile = _fixture
            .DefaultFile()
            .WithFilename(dataFileName)
            .WithType(FileType.Data)
            .WithReplacing(originalDataFile)
            .Generate();

        var metaFile = _fixture
            .DefaultFile()
            .WithFilename(metaFileName)
            .Generate();

        var expectedReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFile(newDataFile)
            .Generate();

        releaseDataFileRepository
            .Setup(mock => mock.Create(
                releaseVersion.Id,
                subjectId,
                dataFileName,
                434,
                FileType.Data,
                _user.Id,
                dataSetName,
                originalDataFile,
                null,
                0))
            .Returns(Task.FromResult(newDataFile));

        releaseDataFileRepository
            .Setup(mock => mock.Create(
                releaseVersion.Id,
                subjectId,
                metaFileName,
                157,
                FileType.Metadata,
                _user.Id,
                null, null, null, 0))
            .Returns(Task.FromResult(metaFile));

        privateBlobStorageService
            .Setup(mock => mock.MoveBlob(
                PrivateReleaseTempFiles,
                It.IsAny<string>(),
                It.IsAny<string>(),
                PrivateReleaseFiles))
            .Returns(Task.FromResult(true));

        dataImportService
           .Setup(s => s.Import(
               It.IsAny<Guid>(),
               newDataFile,
               metaFile,
               null))
           .ReturnsAsync(new DataImport
           {
               Status = QUEUED,
               MetaFile = metaFile,
           });

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, expectedReleaseFile);
            contentDbContext.Files.AddRange(originalDataFile, metaFile);
            await contentDbContext.SaveChangesAsync();
        }

        var dataSet = new DataSetUpload
        {
            ReleaseVersionId = releaseVersion.Id,
            DataSetTitle = dataSetName,
            DataFileId = Guid.NewGuid(),
            DataFileName = dataFileName,
            DataFileSizeInBytes = 434,
            MetaFileId = Guid.NewGuid(),
            MetaFileName = metaFileName,
            MetaFileSizeInBytes = 157,
            Status = DataSetUploadStatus.SCREENING,
            UploadedBy = _user.Email,
            ReplacingFileId = originalDataFile.Id,
        };

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseDataFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataImportService: dataImportService.Object,
                releaseVersionRepository: releaseVersionRepository.Object,
                releaseDataFileRepository: releaseDataFileRepository.Object
            );

            // Act
            var uploadSummaries = await service.MoveDataSetsToPermanentStorage(
                releaseVersion.Id,
                [dataSet],
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(privateBlobStorageService, dataImportService, releaseVersionRepository, releaseDataFileRepository);

            var uploadSummary = Assert.Single(uploadSummaries);
            Assert.Equal(expectedReleaseFile.ReleaseVersionId, uploadSummary.ReleaseVersionId);
            Assert.Equivalent(expectedReleaseFile.File, uploadSummary.File);
            Assert.Equal(expectedReleaseFile.Order, uploadSummary.Order);
        }
    }

    [Fact]
    public async Task RetrieveDataSetFileFromTemporaryStorage_Success_ReturnsFileStreamResult()
    {
        // Arrange
        var dataSetUploadId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();

        await using var contentDbContext = InMemoryApplicationDbContext();
        contentDbContext.DataSetUploads.Add(new()
        {
            Id = dataSetUploadId,
            ReleaseVersionId = releaseVersionId,
            DataSetTitle = "Test Data Set",
            DataFileId = Guid.NewGuid(),
            DataFileName = "data.csv",
            DataFileSizeInBytes = 123,
            MetaFileName = "meta.csv",
            MetaFileSizeInBytes = 456,
            MetaFileId = Guid.NewGuid(),
            UploadedBy = _user.Email,
            Status = DataSetUploadStatus.SCREENING,
        });

        await contentDbContext.SaveChangesAsync();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService
            .Setup(mock => mock.DownloadToStream(
                PrivateReleaseTempFiles,
                It.IsAny<string>(),
                It.IsAny<MemoryStream>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Stream.Null);

        var service = SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: privateBlobStorageService.Object);

        // Act
        var result = await service.RetrieveDataSetFileFromTemporaryStorage(
            releaseVersionId,
            dataSetUploadId,
            FileType.Data,
            cancellationToken: default);

        // Assert
        privateBlobStorageService.Verify();
        var fileStreamResult = result.AssertRight();
        Assert.Equal(ContentTypes.Csv, fileStreamResult.ContentType);
        Assert.Equal(Stream.Null, fileStreamResult.FileStream);
    }

    [Fact]
    public async Task RetrieveDataSetFileFromTemporaryStorage_UploadNotFound_ReturnsNotFound()
    {
        // Arrange
        await using var contentDbContext = InMemoryApplicationDbContext();

        var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

        // Act
        var result = await service.RetrieveDataSetFileFromTemporaryStorage(
            releaseVersionId: Guid.NewGuid(),
            dataSetUploadId: Guid.NewGuid(),
            FileType.Data,
            cancellationToken: default);

        // Assert
        result
            .AssertLeft()
            .AssertNotFoundResult();
    }

    [Fact]
    public async Task RetrieveDataSetFileFromTemporaryStorage_InvalidFileType_ThrowsInvalidEnumArgumentException()
    {
        // Arrange
        var dataSetUploadId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();

        await using var contentDbContext = InMemoryApplicationDbContext();
        contentDbContext.DataSetUploads.Add(new()
        {
            Id = dataSetUploadId,
            ReleaseVersionId = releaseVersionId,
            DataSetTitle = "Test Data Set",
            DataFileId = Guid.NewGuid(),
            DataFileName = "data.csv",
            DataFileSizeInBytes = 123,
            MetaFileName = "meta.csv",
            MetaFileSizeInBytes = 456,
            MetaFileId = Guid.NewGuid(),
            UploadedBy = _user.Email,
            Status = DataSetUploadStatus.SCREENING,
        });

        await contentDbContext.SaveChangesAsync();

        var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidEnumArgumentException>(async ()
            => await service.RetrieveDataSetFileFromTemporaryStorage(
                releaseVersionId,
                dataSetUploadId,
                FileType.Image,
                cancellationToken: default));

        Assert.Equal($"The value of argument 'fileType' ({(int)FileType.Image}) is invalid for Enum type '{nameof(FileType)}'. (Parameter 'fileType')", exception.Message);
    }

    private DataSetFileStorage SetupReleaseDataFileService(
        ContentDbContext contentDbContext,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null,
        IDataSetUploadRepository? dataSetUploadRepository = null,
        IDataImportService? dataImportService = null,
        IUserService? userService = null,
        IDataSetVersionService? dataSetVersionService = null,
        IDataSetService? dataSetService = null,
        IOptions<FeatureFlagsOptions>? featureFlags = null,
        bool addDefaultUser = true)
    {
        if (addDefaultUser)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();
        }

        return new DataSetFileStorage(
            contentDbContext,
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
            releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>(Strict),
            dataSetUploadRepository ?? Mock.Of<IDataSetUploadRepository>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object,
            dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
            dataSetService ?? Mock.Of<IDataSetService>(Strict),
            featureFlags ?? Mock.Of<IOptions<FeatureFlagsOptions>>(Strict),
            Mock.Of<ILogger<DataSetFileStorage>>(Strict));
    }

      
    [Fact]
    public async Task UploadDataSet_ReplaceInitialDraft_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString()))
        {
            var context = await SetupTestContext(
                dataSetName: dataSetName,
                dataFileName: dataFileName,
                metaFileName: metaFileName,
                contentDbContext: contentDbContext,
                contentDbContextId: contentDbContextId,
                isPublished: false,
                version: new SemVersion(1, 0, 0)
            );
            
            context.DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                    context.DatasetVersion.DataSetId,
                    context.DatasetVersion.SemVersion(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(context.DatasetVersion);
            
            context.DataSetVersionService.Setup(mock => mock.DeleteVersion(
                    context.DatasetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);
            
            context.DataSetService.Setup(mock => mock.CreateDataSet(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSetViewModel
                {
                    Title = dataSetName,
                    Summary = null,
                    DraftVersion = new DataSetDraftVersionViewModel
                    {
                        Id = default,
                        Version = null,
                        Status = DataSetVersionStatus.Processing,
                        Type = DataSetVersionType.Minor,
                        File = null,
                        ReleaseVersion = null,
                        Notes = null
                    },
                    LatestLiveVersion = null,
                    PreviousReleaseIds = null,
                    Id = Guid.NewGuid(),
                    Status = DataSetStatus.Draft
                });
           
            var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
            var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

            var dataSet = new DataSet
            {
                Title = dataSetName, DataFile = dataSetFile, MetaFile = metaSetFile, ReplacingFile = context.DataFileReplace
            };
          
            var service = CreateService(context, contentDbContext);
            // Act
            var uploadSummary = await service.UploadDataSet(
                context.ReleaseVersion.Id,
                dataSet,
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(context.DataSetService, context.DataSetVersionService, context.DataImportService,
                context.ReleaseVersionRepository, context.ReleaseDataFileRepository);

            AssertUploadSummary(uploadSummary, dataSetName, dataFileName, metaFileName);
        }
    }
    
    [Fact]
    public async Task UploadDataSet_ReplaceDraft_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString()))
        {
            var context = await SetupTestContext(
                dataSetName: dataSetName,
                dataFileName: dataFileName,
                metaFileName: metaFileName,
                contentDbContext: contentDbContext,
                contentDbContextId: contentDbContextId,
                isPublished: false,
                version: new SemVersion(2, 0, 0)
            );
            
            context.DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                    context.DatasetVersion.DataSetId,
                    context.DatasetVersion.SemVersion(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(context.DatasetVersion);
            
            context.DataSetVersionService.Setup(mock => mock.DeleteVersion(
                    context.DatasetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);
            
            context.DataSetVersionService.Setup(mock => mock.CreateNextVersion(
                    context.ReleaseFile.Id,
                    context.DatasetVersion.DataSetId,
                    null, // We don't create a patch version if the data set was just a draft data set
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSetVersionSummaryViewModel
                {
                    Id = Guid.NewGuid(),
                    Version = "2.0.1",
                    Status = DataSetVersionStatus.Processing,
                    Type = DataSetVersionType.Minor,
                    ReleaseVersion = null,
                    File = null
                });
           
            var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
            var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

            var dataSet = new DataSet
            {
                Title = dataSetName, DataFile = dataSetFile, MetaFile = metaSetFile, ReplacingFile = context.DataFileReplace
            };
          
            var service = CreateService(context, contentDbContext);
            // Act
            var uploadSummary = await service.UploadDataSet(
                context.ReleaseVersion.Id,
                dataSet,
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(context.DataSetService, context.DataSetVersionService, context.DataImportService,
                context.ReleaseVersionRepository, context.ReleaseDataFileRepository);

            AssertUploadSummary(uploadSummary, dataSetName, dataFileName, metaFileName);
        }
    }

    [Fact]
    public async Task UploadDataSet_ReplacePublishByPatch_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString()))
        {
            var context = await SetupTestContext(
                dataSetName: dataSetName,
                dataFileName: dataFileName,
                metaFileName: metaFileName,
                contentDbContext: contentDbContext,
                contentDbContextId: contentDbContextId,
                isPublished: true,
                version: new SemVersion(2, 0, 0)
            );
            
            context.DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                    context.DatasetVersion.DataSetId,
                    context.DatasetVersion.SemVersion(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(context.DatasetVersion);
            
            context.DataSetVersionService.Setup(mock => mock.CreateNextVersion(
                    context.ReleaseFile.Id,
                    context.DatasetVersion.DataSetId,
                    context.DatasetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataSetVersionSummaryViewModel
                {
                    Id = Guid.NewGuid(),
                    Version = "2.0.1",
                    Status = DataSetVersionStatus.Processing,
                    Type = DataSetVersionType.Minor,
                    ReleaseVersion = null,
                    File = null
                });

            var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
            var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

            var dataSet = new DataSet
            {
                Title = dataSetName, DataFile = dataSetFile, MetaFile = metaSetFile, ReplacingFile = context.DataFileReplace
            };
          
            var service = CreateService(context, contentDbContext);
            // Act
            var uploadSummary = await service.UploadDataSet(
                context.ReleaseVersion.Id,
                dataSet,
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(context.DataSetService, context.DataSetVersionService, context.DataImportService,
                context.ReleaseVersionRepository, context.ReleaseDataFileRepository);

            AssertUploadSummary(uploadSummary, dataSetName, dataFileName, metaFileName);
        }
    }

    private static void AssertUploadSummary(
        DataFileInfo uploadSummary,
        string dataSetName,
        string dataFileName,
        string metaFileName)
    {
        Assert.True(uploadSummary.Id.HasValue);
        Assert.Equal(dataSetName, uploadSummary.Name);
        Assert.Equal(dataFileName, uploadSummary.FileName);
        Assert.Equal("csv", uploadSummary.Extension);
        Assert.True(uploadSummary.MetaFileId.HasValue);
        Assert.Equal(metaFileName, uploadSummary.MetaFileName);
        Assert.Equal(123, uploadSummary.Rows);
        Assert.Equal("466 Kb", uploadSummary.Size);
        uploadSummary.Created.AssertUtcNow();
        Assert.Equal("test@test.com", uploadSummary.UserName);
        Assert.Equal(QUEUED, uploadSummary.Status);
    }
    
    public class TestContext
    {
        public Mock<IDataImportService> DataImportService { get; set; }
        public Mock<IReleaseVersionRepository> ReleaseVersionRepository { get; set; }
        public Mock<IReleaseDataFileRepository> ReleaseDataFileRepository { get; set; }
        public Mock<IPrivateBlobStorageService> PrivateBlobStorageService { get; set; }
        public Mock<IDataSetVersionService> DataSetVersionService { get; set; }
        public Mock<IDataSetService> DataSetService { get; set; }
        public ReleaseVersion ReleaseVersion { get; set; }
        public File DataFile { get; set; }
        public File DataFileReplace { get; set; }
        public File MetaFile { get; set; }
        public ReleaseFile ReleaseFile { get; set; }
        public ReleaseFile ReleaseFileReplace { get; set; }
        public DataSetVersion DatasetVersion { get; set; }
        public Guid SubjectId { get; set; }
        public string ContentDbContextId { get; set; }
    }

    public async Task<TestContext> SetupTestContext(
        string dataSetName, 
        string dataFileName,
        string metaFileName,
        ContentDbContext contentDbContext,
        Guid contentDbContextId,
        SemVersion version,
        bool isPublished = false)
    {
        var context = new TestContext
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid(),
            ContentDbContextId = contentDbContextId.ToString()
        };
        
        SetupCommonFiles(context, dataFileName, metaFileName, isPublished, version);
        await SetupDbContext(context, contentDbContext);
        SetupCommonMocks(context, metaFileName, dataSetName);

        return context;
    }

    private void SetupCommonMocks(TestContext context, string metaFileName, string dataSetName)
    {
        context.ReleaseDataFileRepository
            .Setup(mock => mock.Create(
                context.ReleaseVersion.Id,
                context.SubjectId,
                context.DataFile.Filename,
                434,
                FileType.Data,
                _user.Id,
                dataSetName,
                context.DataFileReplace,
                null,
                1))
            .Returns(Task.FromResult(context.DataFile));
        
        context.ReleaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(context.ReleaseVersion.Id))
            .Returns(Task.FromResult(context.SubjectId));

        context.ReleaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(context.ReleaseVersion.Id))
            .Returns(Task.FromResult(context.SubjectId));

        context.ReleaseDataFileRepository
            .Setup(mock => mock.Create(
                context.ReleaseVersion.Id,
                context.SubjectId,
                metaFileName,
                157,
                FileType.Metadata,
                _user.Id,
                null, null, null, 0))
            .Returns(Task.FromResult(context.MetaFile));

        context.PrivateBlobStorageService
            .Setup(mock => mock.UploadStream(
                It.IsAny<IBlobContainer>(),
                It.IsAny<string>(),
                It.IsAny<MemoryStream>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        context.DataImportService
            .Setup(s => s.Import(
                It.IsAny<Guid>(),
                It.IsAny<File>(),
                It.IsAny<File>(),
                null))
            .ReturnsAsync(new DataImport { Status = QUEUED, MetaFile = context.MetaFile, TotalRows = 123, });
    }

    private void SetupCommonFiles(
        TestContext context,
        string dataFileName,
        string metaFileName,
        bool isPublished,
        SemVersion dataSetVersionNumber)
    {
        context.DataFile = _fixture
            .DefaultFile()
            .WithType(FileType.Data)
            .WithFilename(dataFileName)
            .WithCreatedByUser(_user)
            .Generate();

        context.DataFileReplace = _fixture
            .DefaultFile()
            .WithType(FileType.Data)
            .WithFilename(dataFileName)
            .WithCreatedByUser(_user)
            .Generate();

        context.MetaFile = _fixture
            .DefaultFile()
            .WithFilename(metaFileName)
            .Generate();

        var status = isPublished ? DataSetVersionStatus.Published : DataSetVersionStatus.Draft;

        context.DatasetVersion = _fixture
            .DefaultDataSetVersion()
            .WithDataSet(_fixture.DefaultDataSet())
            .WithVersionNumber(dataSetVersionNumber.Major, dataSetVersionNumber.Minor, dataSetVersionNumber.Patch)
            .WithStatus(status)
            .Generate();
        
        context.ReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()))
            .Generate();

        context.ReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(context.ReleaseVersion)
            .WithFile(context.DataFile)
            .WithPublicApiDataSetId(context.DatasetVersion.DataSetId)
            .WithPublicApiDataSetVersion(context.DatasetVersion.SemVersion())
            .Generate();
        
        context.ReleaseFileReplace = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(context.ReleaseVersion)
            .WithFile(context.DataFileReplace)
            .WithPublicApiDataSetId(context.DatasetVersion.DataSetId)
            .WithPublicApiDataSetVersion(context.DatasetVersion.SemVersion())
            .Generate();
    }

    private async Task SetupDbContext(TestContext context, ContentDbContext contentDbContext)
    {
        contentDbContext.ReleaseVersions.Add(context.ReleaseVersion);
        contentDbContext.ReleaseFiles.AddRange(context.ReleaseFile, context.ReleaseFileReplace);
        contentDbContext.Files.Add(context.MetaFile);
        await contentDbContext.SaveChangesAsync();
    }

    private DataSetFileStorage CreateService(TestContext context, ContentDbContext contentDbContext)
    {
        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions
        {
            EnableReplacementOfPublicApiDataSets = true
        });

        return SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: context.PrivateBlobStorageService.Object,
            dataImportService: context.DataImportService.Object,
            releaseVersionRepository: context.ReleaseVersionRepository.Object,
            releaseDataFileRepository: context.ReleaseDataFileRepository.Object,
            featureFlags: featureFlagOptions,
            dataSetVersionService: context.DataSetVersionService.Object,
            dataSetService: context.DataSetService.Object,
            addDefaultUser: false
        );
    }
}
