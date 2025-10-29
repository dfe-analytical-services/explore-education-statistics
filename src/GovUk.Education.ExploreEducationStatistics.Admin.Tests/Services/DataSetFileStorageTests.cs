#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using JetBrains.Annotations;
using LinqToDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
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

    private readonly User _user = new DataFixture().DefaultUser().WithId(Guid.NewGuid());

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
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
            .Generate();

        releaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id))
            .Returns(Task.FromResult(subjectId));

        var dataFile = _fixture.DefaultFile().WithFilename(dataFileName).WithCreatedByUser(_user).Generate();

        var metaFile = _fixture.DefaultFile().WithFilename(metaFileName).Generate();

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
                .Setup(mock =>
                    mock.Create(
                        releaseVersion.Id,
                        subjectId,
                        dataFileName,
                        434,
                        FileType.Data,
                        _user.Id,
                        dataSetName,
                        null,
                        0
                    )
                )
                .Returns(Task.FromResult(dataFile));

            releaseDataFileRepository
                .Setup(mock =>
                    mock.Create(
                        releaseVersion.Id,
                        subjectId,
                        metaFileName,
                        157,
                        FileType.Metadata,
                        _user.Id,
                        null,
                        null,
                        0
                    )
                )
                .Returns(Task.FromResult(metaFile));

            privateBlobStorageService
                .Setup(mock =>
                    mock.UploadStream(
                        It.IsAny<IBlobContainer>(),
                        It.IsAny<string>(),
                        It.IsAny<Stream>(),
                        It.IsAny<string>(),
                        ContentEncodings.Gzip,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            dataImportService
                .Setup(s => s.Import(It.IsAny<Guid>(), It.IsAny<File>(), It.IsAny<File>()))
                .ReturnsAsync(
                    new DataImport
                    {
                        Status = QUEUED,
                        MetaFile = metaFile,
                        TotalRows = 123,
                    }
                );
        }

        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(
            new FeatureFlagsOptions() { EnableReplacementOfPublicApiDataSets = false }
        );

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
            var uploadSummary = await service.UploadDataSet(releaseVersion.Id, dataSet, cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(
                privateBlobStorageService,
                dataImportService,
                releaseVersionRepository,
                releaseDataFileRepository
            );

            Assert.True(uploadSummary.Id.HasValue);
            Assert.Equal(dataSetName, uploadSummary.Name);
            Assert.Equal(dataFileName, uploadSummary.FileName);
            Assert.Equal("csv", uploadSummary.Extension);
            Assert.True(uploadSummary.MetaFileId.HasValue);
            Assert.Equal(metaFileName, uploadSummary.MetaFileName);
            Assert.Equal(123, uploadSummary.Rows);
            Assert.Equal("466 Kb", uploadSummary.Size);
            uploadSummary.Created.AssertUtcNow();
            Assert.Equal(_user.Email, uploadSummary.UserName);
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
            .Setup(mock =>
                mock.UploadStream(
                    It.IsAny<IBlobContainer>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    ContentEncodings.Gzip,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        userService
            .Setup(mock => mock.GetProfileFromClaims())
            .Returns(new UserProfileFromClaims(_user.Email, "Test", "Test"));

        var service = SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: privateBlobStorageService.Object,
            userService: userService.Object
        );

        // Act
        var result = await service.UploadDataSetsToTemporaryStorage(
            Guid.NewGuid(),
            [dataSet],
            cancellationToken: default
        );

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
        Assert.Equal(_user.Email.ToLower(), uploadDetails.UploadedBy);
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
                dataSetUploadRepository: dataSetUploadRepository.Object
            );

            // Act
            await service.CreateOrReplaceExistingDataSetUpload(
                dataSetUpload.ReleaseVersionId,
                dataSetUpload,
                cancellationToken: default
            );
        }

        // Assert
        privateBlobStorageService.Verify(
            mock => mock.DeleteBlob(PrivateReleaseTempFiles, It.IsAny<string>()),
            Times.Never()
        );

        dataSetUploadRepository.Verify(
            mock => mock.Delete(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never()
        );

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

        var newDataSetUpload = new DataSetUploadMockBuilder().WithReleaseVersionId(releaseVersionId).BuildEntity();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var dataSetUploadRepository = new Mock<IDataSetUploadRepository>(Strict);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetUploads.Add(existingDataSetUpload);
            await contentDbContext.SaveChangesAsync();

            dataSetUploadRepository
                .Setup(mock => mock.Delete(releaseVersionId, existingDataSetUpload.Id, It.IsAny<CancellationToken>()))
                // ReSharper disable once AccessToDisposedClosure - Note: this call (and hence the reference to contentDbContext) only happens in the Act phase below, before disposal.
                .Callback(() => contentDbContext.DataSetUploads.Remove(existingDataSetUpload))
                .ReturnsAsync(Unit.Instance);

            var service = SetupReleaseDataFileService(
                contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataSetUploadRepository: dataSetUploadRepository.Object
            );

            // Act
            await service.CreateOrReplaceExistingDataSetUpload(
                releaseVersionId,
                newDataSetUpload,
                cancellationToken: default
            );
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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AddScreenerResultToUpload(Guid.NewGuid(), screenerResult: null!, cancellationToken: default)
        );

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
            TestResult.WARNING => builder.WithWarningTests().BuildEntity(),
            TestResult.FAIL => builder.WithFailingTests().BuildEntity(),
            _ => throw new ArgumentOutOfRangeException(nameof(testResult)),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.DataSetUploads.Add(dataSetUpload);
            await contentDbContext.SaveChangesAsync();

            var service = SetupReleaseDataFileService(contentDbContext);

            // Act
            await service.AddScreenerResultToUpload(
                dataSetUpload.Id,
                dataSetUpload.ScreenerResult!,
                cancellationToken: default
            );
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
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
            .Generate();

        releaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id))
            .Returns(Task.FromResult(subjectId));

        var originalDataFile = _fixture.DefaultFile().WithFilename(dataFileName).WithType(FileType.Data).Generate();

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

        var metaFile = _fixture.DefaultFile().WithFilename(metaFileName).Generate();

        var expectedReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFile(newDataFile)
            .Generate();

        releaseDataFileRepository
            .Setup(mock =>
                mock.Create(
                    releaseVersion.Id,
                    subjectId,
                    dataFileName,
                    434,
                    FileType.Data,
                    _user.Id,
                    dataSetName,
                    originalDataFile,
                    0
                )
            )
            .Returns(Task.FromResult(newDataFile));

        releaseDataFileRepository
            .Setup(mock =>
                mock.Create(releaseVersion.Id, subjectId, metaFileName, 157, FileType.Metadata, _user.Id, null, null, 0)
            )
            .Returns(Task.FromResult(metaFile));

        privateBlobStorageService
            .Setup(mock =>
                mock.MoveBlob(PrivateReleaseTempFiles, It.IsAny<string>(), It.IsAny<string>(), PrivateReleaseFiles)
            )
            .Returns(Task.FromResult(true));

        dataImportService
            .Setup(s => s.Import(It.IsAny<Guid>(), newDataFile, metaFile))
            .ReturnsAsync(new DataImport { Status = QUEUED, MetaFile = metaFile });

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
                releaseDataFileRepository: releaseDataFileRepository.Object,
                featureFlags: Microsoft.Extensions.Options.Options.Create(
                    new FeatureFlagsOptions() { EnableReplacementOfPublicApiDataSets = false }
                )
            );

            // Act
            var uploadSummaries = await service.MoveDataSetsToPermanentStorage(
                releaseVersion.Id,
                [dataSet],
                cancellationToken: default
            );

            // Assert
            MockUtils.VerifyAllMocks(
                privateBlobStorageService,
                dataImportService,
                releaseVersionRepository,
                releaseDataFileRepository
            );

            var uploadSummary = Assert.Single(uploadSummaries);
            Assert.Equal(expectedReleaseFile.ReleaseVersionId, uploadSummary.ReleaseVersionId);
            Assert.Equivalent(expectedReleaseFile.File, uploadSummary.File);
            Assert.Equal(expectedReleaseFile.Order, uploadSummary.Order);
        }
    }

    [Fact]
    public async Task GetTemporaryFileDownloadToken_Success_ReturnsBlobDownloadToken()
    {
        // Arrange
        var dataSetUpload = new DataSetUpload
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = Guid.NewGuid(),
            DataSetTitle = "Test Data Set",
            DataFileId = Guid.NewGuid(),
            DataFileName = "data.csv",
            DataFileSizeInBytes = 123,
            MetaFileName = "meta.csv",
            MetaFileSizeInBytes = 456,
            MetaFileId = Guid.NewGuid(),
            UploadedBy = _user.Email,
            Status = DataSetUploadStatus.SCREENING,
        };

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.DataSetUploads.Add(dataSetUpload);

        await contentDbContext.SaveChangesAsync();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var expectedBlobPath =
            $"{FileStoragePathUtils.FilesPath(dataSetUpload.ReleaseVersionId, FileType.Data)}{dataSetUpload.DataFileId}";

        privateBlobStorageService.SetupGetDownloadToken(
            container: PrivateReleaseTempFiles,
            contentType: ContentTypes.Csv,
            filename: dataSetUpload.DataFileName,
            path: expectedBlobPath,
            cancellationToken: default
        );

        var service = SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: privateBlobStorageService.Object
        );

        // Act
        var result = await service.GetTemporaryFileDownloadToken(
            dataSetUpload.ReleaseVersionId,
            dataSetUpload.Id,
            FileType.Data,
            cancellationToken: default
        );

        // Assert
        privateBlobStorageService.Verify();
        var token = result.AssertRight();

        Assert.Equal(ContentTypes.Csv, token.ContentType);
        Assert.Equal(dataSetUpload.DataFileName, token.Filename);
        Assert.Equal("token", token.Token);
        Assert.Equal(PrivateReleaseTempFiles.Name, token.ContainerName);
        Assert.Equal(expectedBlobPath, token.Path);
    }

    [Fact]
    public async Task GetTemporaryFileDownloadToken_UploadNotFound_ReturnsNotFound()
    {
        // Arrange
        await using var contentDbContext = InMemoryApplicationDbContext();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var service = SetupReleaseDataFileService(
            privateBlobStorageService: privateBlobStorageService.Object,
            contentDbContext: contentDbContext
        );

        // Act
        var result = await service.GetTemporaryFileDownloadToken(
            releaseVersionId: Guid.NewGuid(),
            dataSetUploadId: Guid.NewGuid(),
            FileType.Data,
            cancellationToken: default
        );

        // Assert
        result.AssertLeft().AssertNotFoundResult();
    }

    [Fact]
    public async Task GetTemporaryFileDownloadToken_InvalidFileType_ThrowsBadResult()
    {
        // Arrange
        var dataSetUpload = new DataSetUpload
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = Guid.NewGuid(),
            DataSetTitle = "Test Data Set",
            DataFileId = Guid.NewGuid(),
            DataFileName = "data.csv",
            DataFileSizeInBytes = 123,
            MetaFileName = "meta.csv",
            MetaFileSizeInBytes = 456,
            MetaFileId = Guid.NewGuid(),
            UploadedBy = _user.Email,
            Status = DataSetUploadStatus.SCREENING,
        };

        await using var contentDbContext = InMemoryApplicationDbContext();

        contentDbContext.DataSetUploads.Add(dataSetUpload);

        await contentDbContext.SaveChangesAsync();

        var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

        // Act & Assert
        var result = await service.GetTemporaryFileDownloadToken(
            dataSetUpload.ReleaseVersionId,
            dataSetUpload.Id,
            FileType.Image,
            cancellationToken: default
        );

        result.AssertBadRequestWithErrorViewModels(
            expectedErrorViewModels:
            [
                new ErrorViewModel
                {
                    Message = "Invalid fileType value Image for temporary data set upload file type",
                    Path = "fileType",
                    Detail = "Image",
                },
            ]
        );
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
        bool addDefaultUser = true
    )
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
            Mock.Of<ILogger<DataSetFileStorage>>(Strict)
        );
    }

    [Fact]
    public async Task UploadDataSet_ReplaceInitialDraft_ApiDataSet_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString());
        var testFixture = await SetupTestFixture(
            dataSetName: dataSetName,
            dataFileName: dataFileName,
            metaFileName: metaFileName,
            contentDbContext: contentDbContext,
            isPublished: false,
            version: new SemVersion(1, 0, 0)
        );

        testFixture.SetupInitialDataSetRecreation(dataSetName);

        var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSet = new DataSet
        {
            Title = dataSetName,
            DataFile = dataSetFile,
            MetaFile = metaSetFile,
            ReplacingFile = testFixture.DataFileReplace,
        };

        var service = CreateServiceForApiPatchReplacement(testFixture, contentDbContext);

        // Act
        var uploadSummary = await service.UploadDataSet(
            testFixture.ReleaseVersion.Id,
            dataSet,
            cancellationToken: default
        );

        // Assert
        MockUtils.VerifyAllMocks(
            testFixture.DataSetService,
            testFixture.DataSetVersionService,
            testFixture.DataImportService,
            testFixture.ReleaseVersionRepository,
            testFixture.ReleaseDataFileRepository
        );

        AssertUploadSummary(uploadSummary, dataSetName, dataFileName, metaFileName);
    }

    [Fact]
    public async Task UploadDataSet_ReplaceDraft_ApiDataSet_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString());
        var testFixture = await SetupTestFixture(
            dataSetName: dataSetName,
            dataFileName: dataFileName,
            metaFileName: metaFileName,
            contentDbContext: contentDbContext,
            isPublished: false,
            version: new SemVersion(2, 0, 0)
        );

        testFixture.SetupDraftDataSetVersionRecreation();

        var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSet = new DataSet
        {
            Title = dataSetName,
            DataFile = dataSetFile,
            MetaFile = metaSetFile,
            ReplacingFile = testFixture.DataFileReplace,
        };

        var service = CreateServiceForApiPatchReplacement(testFixture, contentDbContext);

        // Act
        var uploadSummary = await service.UploadDataSet(
            testFixture.ReleaseVersion.Id,
            dataSet,
            cancellationToken: default
        );

        // Assert
        MockUtils.VerifyAllMocks(
            testFixture.DataSetService,
            testFixture.DataSetVersionService,
            testFixture.DataImportService,
            testFixture.ReleaseVersionRepository,
            testFixture.ReleaseDataFileRepository
        );

        AssertUploadSummary(uploadSummary, dataSetName, dataFileName, metaFileName);
    }

    [Fact]
    public async Task UploadDataSet_ReplacePublishedViaPatchApiDataSet_ReturnsUploadSummary()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString());
        var testFixture = await SetupTestFixture(
            dataSetName: dataSetName,
            dataFileName: dataFileName,
            metaFileName: metaFileName,
            contentDbContext: contentDbContext,
            isPublished: true,
            version: new SemVersion(2, 0, 0)
        );

        testFixture.SetupPatchDataSetVersionCreation();

        var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSet = new DataSet
        {
            Title = dataSetName,
            DataFile = dataSetFile,
            MetaFile = metaSetFile,
            ReplacingFile = testFixture.DataFileReplace,
        };

        var service = CreateServiceForApiPatchReplacement(testFixture, contentDbContext);

        // Act
        var uploadSummary = await service.UploadDataSet(
            testFixture.ReleaseVersion.Id,
            dataSet,
            cancellationToken: default
        );

        // Assert
        MockUtils.VerifyAllMocks(
            testFixture.DataSetService,
            testFixture.DataSetVersionService,
            testFixture.DataImportService,
            testFixture.ReleaseVersionRepository,
            testFixture.ReleaseDataFileRepository
        );

        AssertUploadSummary(uploadSummary, dataSetName, dataFileName, metaFileName);
    }

    [Fact]
    public async Task MoveDataSetsToPermanentStorage_ReplacePublishedViaPatchApiDataSet_ReturnsReleaseFile()
    {
        // Arrange
        var dataSetName = "Test Data Set";
        var dataFileName = "test-data.csv";
        var metaFileName = "test-data.meta.csv";
        var contentDbContextId = Guid.NewGuid();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString());

        var testFixture = await DataSetFileStorageTestFixture.CreateZipUploadDataSetTestFixture(
            fixture: _fixture,
            user: _user,
            dataSetName: dataSetName,
            dataFileName: dataFileName,
            metaFileName: metaFileName,
            contentDbContext: contentDbContext,
            isPublished: true,
            version: new SemVersion(2, 0, 0)
        );

        var dataSet = new DataSetUpload
        {
            DataFileId = Guid.NewGuid(),
            DataFileName = dataFileName,
            DataFileSizeInBytes = testFixture.DataFile.ContentLength,
            MetaFileId = Guid.NewGuid(),
            MetaFileName = metaFileName,
            MetaFileSizeInBytes = 157,
            ReplacingFileId = testFixture.DataFile.Id,
            ReleaseVersionId = testFixture.ReleaseVersion.Id,
            DataSetTitle = dataSetName,
            Status = DataSetUploadStatus.SCREENING,
            UploadedBy = _user.Email,
        };

        var service = CreateServiceForApiPatchReplacement(testFixture, contentDbContext);

        // Act
        var uploadSummaries = await service.MoveDataSetsToPermanentStorage(
            testFixture.ReleaseVersion.Id,
            [dataSet],
            cancellationToken: default
        );

        // Assert
        MockUtils.VerifyAllMocks(
            testFixture.DataSetService,
            testFixture.DataSetVersionService,
            testFixture.DataImportService,
            testFixture.ReleaseVersionRepository,
            testFixture.ReleaseDataFileRepository
        );

        var uploadSummary = Assert.Single(uploadSummaries);
        Assert.Equal(testFixture.ReleaseFile.ReleaseVersionId, uploadSummary.ReleaseVersionId);
        Assert.Equivalent(testFixture.ReleaseFile.File, uploadSummary.File);
        Assert.Equal(testFixture.ReleaseFile.Order, uploadSummary.Order);
    }

    [Fact]
    public async Task MoveDataSetsToPermanentStorage_ReplaceMultiplePublishedViaPatchApiDataSet_ReturnsReleaseFile()
    {
        // Arrange
        string[] dataFileNames = ["test-data-1.csv", "test-data-2.csv"];
        string[] metaFileNames = ["test-data-1.meta.csv", "test-data-2.meta.csv"];
        string[] dataSetNames = ["test data 1", "test data 2"];
        var contentDbContextId = Guid.NewGuid();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId.ToString());
        var testFixture = await DataSetFileStorageTestFixture.CreateBulkZipUploadDataSetTestFixture(
            fixture: _fixture,
            user: _user,
            dataSetName: dataSetNames,
            dataFileName: dataFileNames,
            metaFileName: metaFileNames,
            contentDbContext: contentDbContext,
            isPublished: true,
            version: new SemVersion(2, 0, 0)
        );

        var releaseFilesAndDataSets = testFixture
            .ReleaseFilesReplacing!.Zip(dataSetNames, (releaseFile, dataSetName) => (releaseFile, dataSetName))
            .ToArray();

        var dataSets = new DataSetUpload[dataSetNames.Length];
        for (var i = 0; i < releaseFilesAndDataSets.Length; i++)
        {
            dataSets[i] = new DataSetUpload
            {
                DataFileSizeInBytes = releaseFilesAndDataSets[i].releaseFile.File.ContentLength,
                MetaFileSizeInBytes = 157,
                ReleaseVersionId = testFixture.ReleaseVersion.Id,
                DataSetTitle = releaseFilesAndDataSets[i].dataSetName,
                Status = DataSetUploadStatus.SCREENING,
                DataFileId = Guid.NewGuid(),
                DataFileName = dataFileNames[i],
                MetaFileId = Guid.NewGuid(),
                MetaFileName = metaFileNames[i],
                ReplacingFileId = releaseFilesAndDataSets[i].releaseFile.FileId,
                UploadedBy = _user.Email,
            };
        }

        var service = CreateServiceForApiPatchReplacement(testFixture, contentDbContext);

        // Act
        var uploadSummaries = await service.MoveDataSetsToPermanentStorage(
            testFixture.ReleaseVersion.Id,
            [.. dataSets],
            cancellationToken: default
        );

        // Assert
        MockUtils.VerifyAllMocks(
            testFixture.DataSetService,
            testFixture.DataSetVersionService,
            testFixture.DataImportService,
            testFixture.ReleaseVersionRepository,
            testFixture.ReleaseDataFileRepository
        );

        Assert.Equal(2, uploadSummaries.Count);
        Assert.Equal(testFixture.ReleaseFile.ReleaseVersionId, uploadSummaries[0].ReleaseVersionId);
        Assert.Equal(dataFileNames, uploadSummaries.Select(x => x.File.Filename));
    }

    [UsedImplicitly]
    private void AssertUploadSummary(
        DataFileInfo uploadSummary,
        string dataSetName,
        string dataFileName,
        string metaFileName
    )
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
        Assert.Equal(_user.Email, uploadSummary.UserName);
        Assert.Equal(QUEUED, uploadSummary.Status);
    }

    private async Task<DataSetFileStorageTestFixture> SetupTestFixture(
        string dataSetName,
        string dataFileName,
        string metaFileName,
        ContentDbContext contentDbContext,
        SemVersion version,
        bool isPublished = false
    ) =>
        await DataSetFileStorageTestFixture.CreateUploadDataSetTestFixture(
            _fixture,
            _user,
            dataSetName,
            dataFileName,
            metaFileName,
            contentDbContext,
            version,
            isPublished
        );

    private DataSetFileStorage CreateServiceForApiPatchReplacement(
        DataSetFileStorageTestFixture fileStorageTestFixture,
        ContentDbContext contentDbContext
    )
    {
        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(
            new FeatureFlagsOptions { EnableReplacementOfPublicApiDataSets = true }
        );

        return SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: fileStorageTestFixture.PrivateBlobStorageService.Object,
            dataImportService: fileStorageTestFixture.DataImportService.Object,
            releaseVersionRepository: fileStorageTestFixture.ReleaseVersionRepository.Object,
            releaseDataFileRepository: fileStorageTestFixture.ReleaseDataFileRepository.Object,
            featureFlags: featureFlagOptions,
            dataSetVersionService: fileStorageTestFixture.DataSetVersionService.Object,
            dataSetService: fileStorageTestFixture.DataSetService.Object,
            addDefaultUser: false
        );
    }
}
