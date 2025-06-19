#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;
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
    public async Task CreateOrReplaceExistingDbRecord_CreateNew_ReturnsUploadDetails()
    {
        // Arrange
        var dataSetUpload = new DataSetUploadMockBuilder().BuildEntity();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

            privateBlobStorageService.Verify(mock => mock.DeleteBlob(
                PrivateReleaseTempFiles,
                It.IsAny<string>()),
                Times.Never());

            var service = SetupReleaseDataFileService(
                contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            // Act
            await service.CreateOrReplaceExistingDbRecord(dataSetUpload.ReleaseVersionId, dataSetUpload, cancellationToken: default);
        }

        // Assert
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var result = await contentDbContext.DataSetUploads.FindAsync(dataSetUpload.Id);

            Assert.NotNull(result);
            Assert.Equivalent(dataSetUpload, result);
        }
    }

    [Fact]
    public async Task CreateOrReplaceExistingDbRecord_ReplaceExisting_ReturnsUploadDetails()
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

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetUploads.Add(existingDataSetUpload);
            await contentDbContext.SaveChangesAsync();

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

            privateBlobStorageService
                .Setup(mock => mock.DeleteBlob(
                    PrivateReleaseTempFiles,
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = SetupReleaseDataFileService(
                contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            // Act
            await service.CreateOrReplaceExistingDbRecord(releaseVersionId, newDataSetUpload, cancellationToken: default);
        }

        // Assert
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var allUploads = await contentDbContext.DataSetUploads.ToListAsync();
            var upload = Assert.Single(allUploads);

            Assert.NotNull(upload);
            Assert.Equivalent(newDataSetUpload, upload);
        }
    }

    [Fact]
    public async Task AddScreenerResultToUpload_UploadNotFound_ThrowsExpectedException()
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

    private DataSetFileStorage SetupReleaseDataFileService(
        ContentDbContext contentDbContext,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null,
        IDataImportService? dataImportService = null,
        IUserService? userService = null,
        IDataSetVersionService? dataSetVersionService = null,
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
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object,
            dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
            featureFlags ?? Mock.Of<IOptions<FeatureFlagsOptions>>(Strict),
            Mock.Of<ILogger<DataSetFileStorage>>(Strict));
    }
}
