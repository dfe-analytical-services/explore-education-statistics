#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
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
        const string subjectName = "Test Subject";
        const string dataFileName = "test-data.csv";
        const string metaFileName = "test-data.meta.csv";

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
            .Returns(Task.FromResult(Guid.NewGuid()));

        var dataFile = _fixture
            .DefaultFile()
            .WithFilename(dataFileName)
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
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        await using var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId);

        contentDbContext.ReleaseVersions.Add(releaseVersion);
        contentDbContext.ReleaseFiles.Add(releaseFile);
        contentDbContext.Files.Add(metaFile);
        await contentDbContext.SaveChangesAsync();

        releaseDataFileRepository
            .Setup(mock => mock.Create(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                dataFileName,
                It.IsAny<long>(),
                FileType.Data,
                It.IsAny<Guid>(),
                subjectName,
                It.IsAny<File?>(),
                null,
                It.IsAny<int>()))
            .Returns(Task.FromResult(dataFile));

        releaseDataFileRepository
            .Setup(mock => mock.Create(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                metaFileName,
                It.IsAny<long>(),
                FileType.Metadata,
                It.IsAny<Guid>(),
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
           });

        var service = SetupReleaseDataFileService(
            contentDbContext: contentDbContext,
            privateBlobStorageService: privateBlobStorageService.Object,
            dataImportService: dataImportService.Object,
            releaseVersionRepository: releaseVersionRepository.Object,
            releaseDataFileRepository: releaseDataFileRepository.Object
        );

        var dataSetFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaSetFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSet = new DataSet
        {
            Title = subjectName,
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
        Assert.Equal(subjectName, uploadSummary.Name);
        Assert.Equal(dataFileName, uploadSummary.FileName);
        Assert.Equal("csv", uploadSummary.Extension);
        Assert.True(uploadSummary.MetaFileId.HasValue);
        Assert.Equal(metaFileName, uploadSummary.MetaFileName);
        Assert.Null(uploadSummary.Rows);
        Assert.Equal("466 Kb", uploadSummary.Size);
        uploadSummary.Created.AssertUtcNow();
        Assert.Equal(QUEUED, uploadSummary.Status);
        Assert.Null(uploadSummary.PublicApiDataSetId);
        Assert.Null(uploadSummary.PublicApiDataSetVersion);
    }

    [Fact]
    public async Task UploadDataSetsToTemporaryStorage_ReturnsUploadSummary()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public async Task MoveDataSetsToPermanentStorage_ReturnsDataFileDetails()
    {
        // Arrange
        // Act
        // Assert
    }

    private DataSetFileStorage SetupReleaseDataFileService(
        ContentDbContext contentDbContext,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null,
        IDataImportService? dataImportService = null,
        IUserService? userService = null)
    {
        contentDbContext.Users.Add(_user);
        contentDbContext.SaveChanges();

        return new DataSetFileStorage(
            contentDbContext,
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
            releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
        );
    }
}
