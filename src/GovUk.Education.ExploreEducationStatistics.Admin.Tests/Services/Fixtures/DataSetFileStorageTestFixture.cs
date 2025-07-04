#nullable enable
using static Moq.MockBehavior;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Moq;
using Semver;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Fixtures;

public class DataSetFileStorageTestFixture
{
    public Mock<IDataImportService> DataImportService { get; private init; }
    public Mock<IReleaseVersionRepository> ReleaseVersionRepository { get; private init; }
    public Mock<IReleaseDataFileRepository> ReleaseDataFileRepository { get; private init; }
    public Mock<IPrivateBlobStorageService> PrivateBlobStorageService { get; private init; }
    public Mock<IDataSetVersionService> DataSetVersionService { get; private init; }
    public Mock<IDataSetService> DataSetService { get; private init; }
    public ReleaseVersion ReleaseVersion { get; private set; } = null!;
    public File DataFile { get; private set; } = null!;
    public File DataFileReplace { get; private set; } = null!;
    private File MetaFile { get; set; } = null!;
    public ReleaseFile ReleaseFile { get; private set; } = null!;
    private ReleaseFile ReleaseFileReplace { get; set; } = null!;
    private DataSetVersion TestDatasetVersion { get; set; } = null!;
    private Guid SubjectId { get; init; }

    private DataSetFileStorageTestFixture(
        Mock<IDataImportService>? dataImportService = null,
        Mock<IReleaseVersionRepository>? releaseVersionRepository = null,
        Mock<IReleaseDataFileRepository>? releaseDataFileRepository = null,
        Mock<IPrivateBlobStorageService>? privateBlobStorageService = null,
        Mock<IDataSetVersionService>? dataSetVersionService = null,
        Mock<IDataSetService>? dataSetService = null)
    {
        DataImportService = dataImportService ?? new Mock<IDataImportService>(Strict);
        ReleaseVersionRepository = releaseVersionRepository ?? new Mock<IReleaseVersionRepository>(Strict);
        ReleaseDataFileRepository = releaseDataFileRepository ?? new Mock<IReleaseDataFileRepository>(Strict);
        PrivateBlobStorageService = privateBlobStorageService ?? new Mock<IPrivateBlobStorageService>(Strict);
        DataSetVersionService = dataSetVersionService ?? new Mock<IDataSetVersionService>(Strict);
        DataSetService = dataSetService ?? new Mock<IDataSetService>(Strict);
    }
    
    /// <summary>
    /// Creates test data and mocks for DataSetFileStorage.UploadDataSet
    /// </summary>
    /// <param name="fixture">Used to generate mock data</param>
    /// <param name="user">Used to mock the user</param>
    /// <param name="dataSetName">Used as mock data</param>
    /// <param name="dataFileName">Used as mock data</param>
    /// <param name="metaFileName">Used as mock data</param>
    /// <param name="contentDbContext">Used to save mocked data</param>
    /// <param name="version">Used to mock the version of the api data set</param>
    /// <param name="isPublished">Used to signal whether to mock the api data set version as published or not</param>
    /// <returns>Abstracted test fixture which is used by test cases to execute tests</returns>
    public static async Task<DataSetFileStorageTestFixture> CreateUploadDataSetTestFixture(
        DataFixture fixture,
        User user,
        string dataSetName,
        string dataFileName,
        string metaFileName,
        ContentDbContext contentDbContext,
        SemVersion version,
        bool isPublished = false)
    {
        var testFixture = new DataSetFileStorageTestFixture
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid()
        };

        await testFixture.SetupCommonData(fixture, user, dataFileName, metaFileName, isPublished, version, contentDbContext);
        testFixture.SetupCommonMocks(user, metaFileName, dataSetName);

        return testFixture;
    }
    
    /// <summary>
    /// Used to Create test data for the ZIP uploading mechanisms provided by DataSetFileStorage method MoveDataSetsToPermanentStorage
    /// </summary>
    /// <param name="fixture">Used to generate mock data</param>
    /// <param name="user">Used to mock the user</param>
    /// <param name="dataSetName">Used as mock data</param>
    /// <param name="dataFileName">Used as mock data</param>
    /// <param name="metaFileName">Used as mock data</param>
    /// <param name="contentDbContext">Used to save mocked data</param>
    /// <param name="version">Used to mock the version of the api data set</param>
    /// <param name="isPublished">Used to signal whether to mock the api data set version as published or not</param>
    /// <returns>Abstracted test fixture which is used by test cases to execute tests</returns>
    public static async Task<DataSetFileStorageTestFixture> CreateZipUploadDataSetTestFixture(
        DataFixture fixture,
        User user,
        string dataSetName,
        string dataFileName,
        string metaFileName,
        ContentDbContext contentDbContext,
        SemVersion version,
        bool isPublished = false)
    {
        var testFixture = new DataSetFileStorageTestFixture
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid()
        };

        await testFixture.SetupCommonData(fixture, user, dataFileName, metaFileName, isPublished, version, contentDbContext);
        testFixture.SetupMocksForZipUpload(user, dataSetName, metaFileName);

        return testFixture;
    }
    
    /// <summary>
    /// Used to Create test data for the BULK ZIP uploading mechanisms provided by DataSetFileStorage method MoveDataSetsToPermanentStorage
    /// </summary>
    /// <param name="fixture">Used to generate mock data</param>
    /// <param name="user">Used to mock the user</param>
    /// <param name="dataSetName">Used as mock data</param>
    /// <param name="dataFileName">Used as mock data</param>
    /// <param name="metaFileName">Used as mock data</param>
    /// <param name="contentDbContext">Used to save mocked data</param>
    /// <param name="version">Used to mock the version of the api data set</param>
    /// <param name="replacingId">Mocked ID which replaces data file</param>
    /// <param name="isPublished">Used to signal whether to mock the api data set version as published or not</param>
    /// <returns>Abstracted test fixture which is used by test cases to execute tests</returns>
    public static async Task<DataSetFileStorageTestFixture> CreateBulkZipUploadDataSetTestFixture(
        DataFixture fixture,
        User user,
        string[] dataSetName,
        string[] dataFileName,
        string[] metaFileName,
        ContentDbContext contentDbContext,
        SemVersion version,
        Guid replacingId,
        bool isPublished = false)
    {
        var testFixture = new DataSetFileStorageTestFixture
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid()
        };

        File[] dataFiles = new File[dataFileName.Length];
        File[] metaFiles = new File[metaFileName.Length];
        DataSet[] dataSets = new DataSet[dataSetName.Length];
        DataSetVersion[] dataSetVersions = new DataSetVersion[dataSetName.Length];
        ReleaseFile[] releaseFiles = new ReleaseFile[dataFileName.Length + 1];
        var status = isPublished ? DataSetVersionStatus.Published : DataSetVersionStatus.Draft;
        
        for (var i = 0; i < dataSetName.Length; i++)
        {
            dataSets[i] = fixture
                .DefaultDataSet()
                .WithTitle(dataSetName[i])
                .Generate();
            dataSetVersions[i] = fixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSets[i])
                .WithVersionNumber(version.Major, version.Minor, version.Patch)
                .WithStatus(status)
                .Generate();
        }
        var releaseVersion = fixture
            .DefaultReleaseVersion()
            .WithRelease(fixture
                .DefaultRelease()
                .WithPublication(fixture.DefaultPublication()))
            .Generate();

        for (var i = 0; i < dataFileName.Length; i++)
        {
            dataFiles[i] = fixture
                .DefaultFile()
                .WithType(FileType.Data)
                .WithFilename(dataFileName[i])
                .WithCreatedByUser(user)
                .Generate();
            releaseFiles[i] = testFixture.ReleaseFile = fixture// set ReleaseFile to one of the itmes so we can verify the releaseVersionId later.
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(dataFiles[i])
                .WithPublicApiDataSetId(dataSetVersions[i].DataSetId)
                .WithPublicApiDataSetVersion(dataSetVersions[i].SemVersion())
                .Generate();
        }
        var dataFileReplacing = fixture
            .DefaultFile()
            .WithType(FileType.Data)
            .WithFilename(dataFileName[0])
            .WithCreatedByUser(user)
            .Generate();
        dataFileReplacing.Id = replacingId;
        var releaseFileReplacing = fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(dataFileReplacing)
            .WithPublicApiDataSetId(dataSetVersions[0].DataSetId)
            .WithPublicApiDataSetVersion(dataSetVersions[0].SemVersion())
            .Generate();
        
        for (var i = 0; i < metaFileName.Length; i++)
        {
            metaFiles[i] = fixture
                .DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename(metaFileName[i])
                .WithCreatedByUser(user)
                .Generate();
        }
        var fileAndDataSetName = dataFiles.Zip(dataSetName, 
                (file, name) => (file, name)).ToArray();
        
        releaseFiles[^1] = releaseFileReplacing;
        testFixture.ReleaseVersion = releaseVersion;
        testFixture.SetupMockBulkPatchDataSetVersionCreation(dataSetVersions);
        testFixture.SetupMocksForBulkUpload(fileAndDataSetName);
        for (var i = 0; i < metaFiles.Length; i++)
        {
            testFixture.ReleaseDataFileRepository
                .Setup(mock => mock.Create(
                    releaseVersion.Id,
                    testFixture.SubjectId,
                    metaFiles[i].Filename,
                    157,
                    FileType.Metadata,
                    user.Id,
                    null, null, null, 0))
                .Returns(Task.FromResult(metaFiles[i]));
        }
        testFixture.SetUpImportAndBlobMocks(user, releaseVersion);
        await testFixture.SetupContentDbContext(contentDbContext, releaseVersion, releaseFiles, metaFiles[0]);

        return testFixture;
    }

    private void SetupMocksForZipUpload(User user, string dataSetName, string metaFileName)
    {
        ReleaseDataFileRepository
            .Setup(mock => mock.Create(
                ReleaseVersion.Id,
                SubjectId,
                DataFile.Filename,
                DataFile.ContentLength,
                FileType.Data,
                user.Id,
                dataSetName, 
                ReleaseFile.File,
                null, 
                0 ))
            .Returns(Task.FromResult(DataFile));
        
        PrivateBlobStorageService
            .Setup(mock => mock.MoveBlob(
                BlobContainers.PrivateReleaseTempFiles,
                It.IsAny<string>(),
                It.IsAny<string>(),
                BlobContainers.PrivateReleaseFiles))
            .Returns(Task.FromResult(true));
        
        SetupPatchDataSetVersionCreation();
        SetUpImportAndBlobMocks(user, ReleaseVersion, metaFileName);
    }
    
    private void SetupMocksForBulkUpload((File, string)[] fileAndDataSetName)
    {
        for (var i = 0; i < fileAndDataSetName.Length; i++)
        {
            ReleaseDataFileRepository
                .Setup(mock => mock.Create(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    FileType.Data,
                    It.IsAny<Guid>(),
                    fileAndDataSetName[i].Item2,
                    It.IsAny<File>(),
                    null,
                    It.IsAny<int>()))
                .Returns(Task.FromResult(fileAndDataSetName[i].Item1));
        }

        PrivateBlobStorageService
            .Setup(mock => mock.MoveBlob(
                BlobContainers.PrivateReleaseTempFiles,
                It.IsAny<string>(),
                It.IsAny<string>(),
                BlobContainers.PrivateReleaseFiles))
            .Returns(Task.FromResult(true));
    }

    /// <summary>
    /// This sets up objects that are commonly used by tests and assigns these objects to properties of this class
    /// </summary>
    /// <param name="fixture">Used to generate objects</param>
    /// <param name="user">Mocks the user</param>
    /// <param name="dataFileName">Mocks of data file names</param>
    /// <param name="metaFileName">Mocks of metadata file names</param>
    /// <param name="isPublished">Mocks whether the api version is set to published or not</param>
    /// <param name="dataSetVersionNumber">Mocks the api version number</param>
    /// <param name="contentDbContext">This is used to save mocked data in memory for the context of the test</param>
    private async Task SetupCommonData(
        DataFixture fixture,
        User user,
        string dataFileName,
        string metaFileName,
        bool isPublished,
        SemVersion dataSetVersionNumber,
        ContentDbContext contentDbContext)
    {
        DataFile = fixture
            .DefaultFile()
            .WithType(FileType.Data)
            .WithFilename(dataFileName)
            .WithCreatedByUser(user)
            .Generate();
        DataFileReplace = fixture
            .DefaultFile()
            .WithType(FileType.Data)
            .WithFilename(dataFileName)
            .WithCreatedByUser(user)
            .Generate();
        MetaFile = fixture
            .DefaultFile()
            .WithFilename(metaFileName)
            .Generate();

        var status = isPublished ? DataSetVersionStatus.Published : DataSetVersionStatus.Draft;

        TestDatasetVersion = fixture
            .DefaultDataSetVersion()
            .WithDataSet(fixture.DefaultDataSet())
            .WithVersionNumber(dataSetVersionNumber.Major, dataSetVersionNumber.Minor, dataSetVersionNumber.Patch)
            .WithStatus(status)
            .Generate();
        ReleaseVersion = fixture
            .DefaultReleaseVersion()
            .WithRelease(fixture
                .DefaultRelease()
                .WithPublication(fixture.DefaultPublication()))
            .Generate();
        ReleaseFile = fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(ReleaseVersion)
            .WithFile(DataFile)
            .WithPublicApiDataSetId(TestDatasetVersion.DataSetId)
            .WithPublicApiDataSetVersion(TestDatasetVersion.SemVersion())
            .Generate();
        ReleaseFileReplace = fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(ReleaseVersion)
            .WithFile(DataFileReplace)
            .WithPublicApiDataSetId(TestDatasetVersion.DataSetId)
            .WithPublicApiDataSetVersion(TestDatasetVersion.SemVersion())
            .Generate();

        contentDbContext.ReleaseVersions.Add(ReleaseVersion);
        contentDbContext.ReleaseFiles.AddRange(ReleaseFile, ReleaseFileReplace);
        contentDbContext.Files.Add(MetaFile);
        await contentDbContext.SaveChangesAsync();
    }

    private async Task SetupContentDbContext(
        ContentDbContext contentDbContext, 
        ReleaseVersion releaseVersion, 
        ReleaseFile[] releaseFiles,
        File metaFile)
    {
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        contentDbContext.ReleaseFiles.AddRange(releaseFiles);
        contentDbContext.Files.Add(metaFile);
        await contentDbContext.SaveChangesAsync();
    }

    private void SetupCommonMocks(
        User user,
        string metaFileName,
        string dataSetName)
    {
        ReleaseDataFileRepository
            .Setup(mock => mock.Create(
                ReleaseVersion.Id,
                SubjectId,
                DataFile.Filename,
                434,
                FileType.Data,
                user.Id,
                dataSetName,
                DataFileReplace,
                null,
                1))
            .Returns(Task.FromResult(DataFile));

        SetUpImportAndBlobMocks(user, ReleaseVersion, metaFileName);
    }

    private void SetUpImportAndBlobMocks(User user, ReleaseVersion releaseVersion, string? metaFileName = null)
    {
        ReleaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id))
            .Returns(Task.FromResult(SubjectId));

        ReleaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersion.Id))
            .Returns(Task.FromResult(SubjectId));

        if (metaFileName is not null)
        {
            ReleaseDataFileRepository
                .Setup(mock => mock.Create(
                    ReleaseVersion.Id,
                    SubjectId,
                    metaFileName,
                    157,
                    FileType.Metadata,
                    user.Id,
                    null, null, null, 0))
                .Returns(Task.FromResult(MetaFile));
        }
        
        PrivateBlobStorageService
            .Setup(mock => mock.UploadStream(
                It.IsAny<IBlobContainer>(),
                It.IsAny<string>(),
                It.IsAny<MemoryStream>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DataImportService
            .Setup(s => s.Import(
                It.IsAny<Guid>(),
                It.IsAny<File>(),
                It.IsAny<File>(),
                null))
            .ReturnsAsync(new DataImport { Status = QUEUED, MetaFile = MetaFile, TotalRows = 123, });
    }

    public void SetupInitialDataSetRecreation(string dataSetName)
    {
        DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                TestDatasetVersion.DataSetId,
                TestDatasetVersion.SemVersion(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDatasetVersion);

        DataSetVersionService.Setup(mock => mock.DeleteVersion(
                TestDatasetVersion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        DataSetService.Setup(mock => mock.CreateDataSet(
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
    }

    public void SetupDraftDataSetVersionRecreation()
    {
        DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                TestDatasetVersion.DataSetId,
                TestDatasetVersion.SemVersion(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDatasetVersion);

        DataSetVersionService.Setup(mock => mock.DeleteVersion(
                TestDatasetVersion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        DataSetVersionService.Setup(mock => mock.CreateNextVersion(
                ReleaseFile.Id,
                TestDatasetVersion.DataSetId,
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
    }

    public void SetupPatchDataSetVersionCreation()
    {
        DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                TestDatasetVersion.DataSetId,
                TestDatasetVersion.SemVersion(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDatasetVersion);

        DataSetVersionService.Setup(mock => mock.CreateNextVersion(
                ReleaseFile.Id,
                TestDatasetVersion.DataSetId,
                TestDatasetVersion.Id,
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
    }

    private void SetupMockBulkPatchDataSetVersionCreation(DataSetVersion[] dataSetVersions)
    {
        for (var i = 0; i < dataSetVersions.Length; i++)
        {
            DataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                    It.IsAny<Guid>(),
                    It.IsAny<SemVersion>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSetVersions[i]);
        }
        
        DataSetVersionService.Setup(mock => mock.CreateNextVersion(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
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
    }
}
