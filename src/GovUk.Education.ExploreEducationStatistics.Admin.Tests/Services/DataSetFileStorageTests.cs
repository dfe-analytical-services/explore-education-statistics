#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.Extensions.Logging;
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

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseDataFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataImportService: dataImportService.Object,
                releaseVersionRepository: releaseVersionRepository.Object,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                enableReplacementOfPublicApiDataSets: false,
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
        }
    }

    [Fact]
    public async Task UploadDataSetsToTemporaryStorage_ReturnsUploadSummary()
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

        privateBlobStorageService
            .Setup(mock => mock.UploadStream(
                It.IsAny<IBlobContainer>(),
                It.IsAny<string>(),
                It.IsAny<MemoryStream>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: privateBlobStorageService.Object,
            enableReplacementOfPublicApiDataSets: false);

        // Act
        var uploadSummaries = await service.UploadDataSetsToTemporaryStorage(
            Guid.NewGuid(),
            [dataSet],
            cancellationToken: default);

        // Assert
        privateBlobStorageService.Verify();

        var uploadSummary = Assert.Single(uploadSummaries);
        Assert.Equal(dataSetName, uploadSummary.Title);
        Assert.NotEqual(Guid.Empty, uploadSummary.DataFileId);
        Assert.Equal("test-data.csv", uploadSummary.DataFileName);
        Assert.Equal(434, uploadSummary.DataFileSize);
        Assert.NotEqual(Guid.Empty, uploadSummary.MetaFileId);
        Assert.Equal("test-data.meta.csv", uploadSummary.MetaFileName);
        Assert.Equal(157, uploadSummary.MetaFileSize);
        Assert.Null(uploadSummary.ReplacingFileId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]

    public async Task MoveDataSetsToPermanentStorage_WithReplacement_ReturnsReleaseFile(bool apiVersionReplacementEnabled)
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
        var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);

        
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
        
        var dataSetVersion = _fixture
            .DefaultDataSetVersion(filters: 1,
                indicators: 1,
                locations: 1,
                timePeriods: 2)
            .WithDataSet(_fixture.DefaultDataSet())
            .WithRelease(_fixture.DefaultDataSetVersionRelease()
                .WithReleaseFileId(originalDataFile.Id))
            .WithStatus(DataSetVersionStatus.Processing)
            .Generate();

        var originalReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFile(originalDataFile)
            .WithPublicApiDataSetId(dataSetVersion.DataSetId)
            .WithPublicApiDataSetVersion(dataSetVersion.SemVersion())
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

        if (apiVersionReplacementEnabled)
        {
            dataSetVersionService
                .Setup(srv =>
                    srv.GetDataSetVersion(dataSetVersion.DataSetId, dataSetVersion.SemVersion(), It.IsAny<CancellationToken>())
                ).ReturnsAsync(dataSetVersion);

            dataSetVersionService
                .Setup(srv =>
                    srv.CreateNextVersion(It.IsAny<Guid>(),  dataSetVersion.DataSetId, dataSetVersion.Id, It.IsAny<CancellationToken>())
                ).ReturnsAsync(new DataSetVersionSummaryViewModel
                {
                    Id = default,
                    Version = "1.0.1",
                    Status = DataSetVersionStatus.Processing,
                    Type = DataSetVersionType.Major,
                    ReleaseVersion = null,
                    File = null
                });
        }
        
        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, expectedReleaseFile);
            contentDbContext.Files.AddRange(originalDataFile, metaFile);
            await contentDbContext.SaveChangesAsync();
        }

        var dataSet = new DataSetUploadResultViewModel
        {
            Title = dataSetName,
            DataFileId = Guid.NewGuid(),
            DataFileName = dataFileName,
            DataFileSize = 434,
            MetaFileId = Guid.NewGuid(),
            MetaFileName = metaFileName,
            MetaFileSize = 157,
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
                enableReplacementOfPublicApiDataSets: apiVersionReplacementEnabled,
                dataSetVersionService: dataSetVersionService.Object
            );

            // Act
            var uploadSummaries = await service.MoveDataSetsToPermanentStorage(
                releaseVersion.Id,
                [dataSet],
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(dataSetVersionService, privateBlobStorageService, dataImportService, releaseVersionRepository, releaseDataFileRepository);

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
        bool enableReplacementOfPublicApiDataSets = false,
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
            Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions
            {
                EnableReplacementOfPublicApiDataSets = enableReplacementOfPublicApiDataSets
            }),
            Mock.Of<ILogger<DataSetFileStorage>>(Strict));
    }
}
