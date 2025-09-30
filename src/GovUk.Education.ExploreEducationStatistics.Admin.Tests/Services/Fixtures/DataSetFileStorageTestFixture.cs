#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Semver;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
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
    public ReleaseFile[]? ReleaseFilesReplacing { get; set; }

    private DataSetFileStorageTestFixture(
        Mock<IDataImportService>? dataImportService = null,
        Mock<IReleaseVersionRepository>? releaseVersionRepository = null,
        Mock<IReleaseDataFileRepository>? releaseDataFileRepository = null,
        Mock<IPrivateBlobStorageService>? privateBlobStorageService = null,
        Mock<IDataSetVersionService>? dataSetVersionService = null,
        Mock<IDataSetService>? dataSetService = null
    )
    {
        DataImportService = dataImportService ?? new Mock<IDataImportService>(Strict);
        ReleaseVersionRepository =
            releaseVersionRepository ?? new Mock<IReleaseVersionRepository>(Strict);
        ReleaseDataFileRepository =
            releaseDataFileRepository ?? new Mock<IReleaseDataFileRepository>(Strict);
        PrivateBlobStorageService =
            privateBlobStorageService ?? new Mock<IPrivateBlobStorageService>(Strict);
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
        bool isPublished = false
    )
    {
        var testFixture = new DataSetFileStorageTestFixture
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid(),
        };

        await testFixture.SetupCommonData(
            fixture,
            user,
            dataFileName,
            metaFileName,
            isPublished,
            version,
            contentDbContext
        );
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
        bool isPublished = false
    )
    {
        var testFixture = new DataSetFileStorageTestFixture
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid(),
        };

        await testFixture.SetupCommonData(
            fixture,
            user,
            dataFileName,
            metaFileName,
            isPublished,
            version,
            contentDbContext
        );
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
        bool isPublished = false
    )
    {
        if (dataSetName.Length != dataFileName.Length || dataSetName.Length != metaFileName.Length)
        {
            throw new ArgumentException("All arrays must be of the same length");
        }

        var testFixture = new DataSetFileStorageTestFixture
        {
            DataImportService = new Mock<IDataImportService>(Strict),
            ReleaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict),
            ReleaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict),
            PrivateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict),
            DataSetVersionService = new Mock<IDataSetVersionService>(Strict),
            DataSetService = new Mock<IDataSetService>(Strict),
            SubjectId = Guid.NewGuid(),
        };

        var dataFiles = new File[dataFileName.Length];
        var metaFiles = new File[metaFileName.Length];
        var dataSets = new DataSet[dataSetName.Length];
        var dataSetVersions = new DataSetVersion[dataSetName.Length];
        var releaseFiles = new ReleaseFile[dataFileName.Length];
        var status = isPublished ? DataSetVersionStatus.Published : DataSetVersionStatus.Draft;

        for (var i = 0; i < dataSetName.Length; i++)
        {
            dataSets[i] = fixture.DefaultDataSet().WithTitle(dataSetName[i]).Generate();
            dataSetVersions[i] = fixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSets[i])
                .WithVersionNumber(version.Major, version.Minor, version.Patch)
                .WithStatus(status)
                .Generate();
        }

        var releaseVersion = fixture
            .DefaultReleaseVersion()
            .WithRelease(fixture.DefaultRelease().WithPublication(fixture.DefaultPublication()))
            .Generate();

        for (var i = 0; i < dataFileName.Length; i++)
        {
            dataFiles[i] = fixture
                .DefaultFile()
                .WithType(FileType.Data)
                .WithFilename(dataFileName[i])
                .WithCreatedByUser(user)
                .Generate();
            releaseFiles[i] = testFixture.ReleaseFile = fixture // set ReleaseFile to one of the items so we can verify the releaseVersionId later.
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(dataFiles[i])
                .WithPublicApiDataSetId(dataSetVersions[i].DataSetId)
                .WithPublicApiDataSetVersion(dataSetVersions[i].SemVersion())
                .Generate();
        }

        var releaseFileReplacing = CreateReplacingReleaseFiles(
            fixture,
            user,
            dataFileName,
            releaseVersion,
            dataSetVersions
        );

        testFixture.ReleaseFilesReplacing = releaseFileReplacing; // Used to create the input DataSet view model that the method under test takes

        for (var i = 0; i < metaFileName.Length; i++)
        {
            metaFiles[i] = fixture
                .DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename(metaFileName[i])
                .WithCreatedByUser(user)
                .Generate();
        }
        var fileAndDataSetName = dataFiles.Zip(dataSetName, (file, name) => (file, name)).ToArray();

        testFixture.ReleaseVersion = releaseVersion; //Used by some of the shared mock services set up

        testFixture.SetupMockBulkPatchDataSetVersionCreation(dataSetVersions, releaseFiles);
        testFixture.SetupMocksForBulkUpload(fileAndDataSetName);

        foreach (var metafile in metaFiles)
        {
            testFixture
                .ReleaseDataFileRepository.Setup(mock =>
                    mock.Create(
                        releaseVersion.Id,
                        testFixture.SubjectId,
                        metafile.Filename,
                        157,
                        FileType.Metadata,
                        user.Id,
                        null,
                        null,
                        null,
                        0
                    )
                )
                .Returns(Task.FromResult(metafile));
        }

        var releaseFilesToSave = releaseFileReplacing.Concat(releaseFiles).Distinct().ToArray();
        testFixture.SetUpImportAndBlobMocks(user, releaseVersion);
        await testFixture.SetupContentDbContext(
            contentDbContext,
            releaseVersion,
            releaseFilesToSave,
            metaFiles[0]
        );

        return testFixture;
    }

    private static ReleaseFile[] CreateReplacingReleaseFiles(
        DataFixture fixture,
        User user,
        string[] dataFileName,
        ReleaseVersion releaseVersion,
        DataSetVersion[] dataSetVersions
    )
    {
        var versionsAndFileNames = dataSetVersions.Zip(
            dataFileName,
            (version, fileName) => (version, fileName)
        );
        return
        [
            .. versionsAndFileNames.Select(versionAndFileName =>
            {
                var dataFileReplacing = fixture
                    .DefaultFile()
                    .WithType(FileType.Data)
                    .WithFilename(versionAndFileName.fileName)
                    .WithCreatedByUser(user)
                    .Generate();
                return fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(dataFileReplacing)
                    .WithPublicApiDataSetId(versionAndFileName.version.DataSetId)
                    .WithPublicApiDataSetVersion(versionAndFileName.version.SemVersion())
                    .Generate();
            }),
        ];
    }

    private void SetupMocksForZipUpload(User user, string dataSetName, string metaFileName)
    {
        ReleaseDataFileRepository
            .Setup(mock =>
                mock.Create(
                    ReleaseVersion.Id,
                    SubjectId,
                    DataFile.Filename,
                    DataFile.ContentLength,
                    FileType.Data,
                    user.Id,
                    dataSetName,
                    ReleaseFile.File,
                    null,
                    0
                )
            )
            .Returns(Task.FromResult(DataFile));

        PrivateBlobStorageService
            .Setup(mock =>
                mock.MoveBlob(
                    BlobContainers.PrivateReleaseTempFiles,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    BlobContainers.PrivateReleaseFiles
                )
            )
            .Returns(Task.FromResult(true));

        SetupPatchDataSetVersionCreation();
        SetUpImportAndBlobMocks(user, ReleaseVersion, metaFileName);
    }

    private void SetupMocksForBulkUpload((File, string)[] fileAndDataSetName)
    {
        for (var i = 0; i < fileAndDataSetName.Length; i++)
        {
            var dataSetName = fileAndDataSetName[i].Item2;
            ReleaseDataFileRepository
                .Setup(mock =>
                    mock.Create(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<string>(),
                        It.IsAny<long>(),
                        FileType.Data,
                        It.IsAny<Guid>(),
                        It.Is<string>(actual => actual == dataSetName),
                        It.IsAny<File>(),
                        null,
                        It.IsAny<int>()
                    )
                )
                .Returns(Task.FromResult(fileAndDataSetName[i].Item1));
        }

        PrivateBlobStorageService
            .Setup(mock =>
                mock.MoveBlob(
                    BlobContainers.PrivateReleaseTempFiles,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    BlobContainers.PrivateReleaseFiles
                )
            )
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
        ContentDbContext contentDbContext
    )
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
        MetaFile = fixture.DefaultFile().WithFilename(metaFileName).Generate();

        var status = isPublished ? DataSetVersionStatus.Published : DataSetVersionStatus.Draft;

        TestDatasetVersion = fixture
            .DefaultDataSetVersion()
            .WithDataSet(fixture.DefaultDataSet())
            .WithVersionNumber(
                dataSetVersionNumber.Major,
                dataSetVersionNumber.Minor,
                dataSetVersionNumber.Patch
            )
            .WithStatus(status)
            .Generate();
        ReleaseVersion = fixture
            .DefaultReleaseVersion()
            .WithRelease(fixture.DefaultRelease().WithPublication(fixture.DefaultPublication()))
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
        File metaFile
    )
    {
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        contentDbContext.ReleaseFiles.AddRange(releaseFiles);
        contentDbContext.Files.Add(metaFile);
        await contentDbContext.SaveChangesAsync();
    }

    private void SetupCommonMocks(User user, string metaFileName, string dataSetName)
    {
        ReleaseDataFileRepository
            .Setup(mock =>
                mock.Create(
                    ReleaseVersion.Id,
                    SubjectId,
                    DataFile.Filename,
                    434,
                    FileType.Data,
                    user.Id,
                    dataSetName,
                    DataFileReplace,
                    null,
                    1
                )
            )
            .Returns(Task.FromResult(DataFile));

        SetUpImportAndBlobMocks(user, ReleaseVersion, metaFileName);
    }

    private void SetUpImportAndBlobMocks(
        User user,
        ReleaseVersion releaseVersion,
        string? metaFileName = null
    )
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
                .Setup(mock =>
                    mock.Create(
                        ReleaseVersion.Id,
                        SubjectId,
                        metaFileName,
                        157,
                        FileType.Metadata,
                        user.Id,
                        null,
                        null,
                        null,
                        0
                    )
                )
                .Returns(Task.FromResult(MetaFile));
        }

        PrivateBlobStorageService
            .Setup(mock =>
                mock.UploadStream(
                    It.IsAny<IBlobContainer>(),
                    It.IsAny<string>(),
                    It.IsAny<MemoryStream>(),
                    It.IsAny<string>(),
                    ContentEncodings.Gzip,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        DataImportService
            .Setup(s => s.Import(It.IsAny<Guid>(), It.IsAny<File>(), It.IsAny<File>(), null))
            .ReturnsAsync(
                new DataImport
                {
                    Status = QUEUED,
                    MetaFile = MetaFile,
                    TotalRows = 123,
                }
            );
    }

    public void SetupInitialDataSetRecreation(string dataSetName)
    {
        DataSetVersionService
            .Setup(mock =>
                mock.GetDataSetVersion(
                    TestDatasetVersion.DataSetId,
                    TestDatasetVersion.SemVersion(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(TestDatasetVersion);

        DataSetVersionService
            .Setup(mock => mock.DeleteVersion(TestDatasetVersion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        DataSetService
            .Setup(mock => mock.CreateDataSet(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new DataSetViewModel
                {
                    Title = dataSetName,
                    Summary = "Summary",
                    DraftVersion = new DataSetDraftVersionViewModel
                    {
                        Id = Guid.NewGuid(),
                        Version = "1.0",
                        Status = DataSetVersionStatus.Processing,
                        Type = DataSetVersionType.Minor,
                        File = new IdTitleViewModel(),
                        ReleaseVersion = new IdTitleViewModel(),
                        Notes = "Some notes.",
                    },
                    LatestLiveVersion = null,
                    PreviousReleaseIds = [],
                    Id = Guid.NewGuid(),
                    Status = DataSetStatus.Draft,
                }
            );
    }

    public void SetupDraftDataSetVersionRecreation()
    {
        DataSetVersionService
            .Setup(mock =>
                mock.GetDataSetVersion(
                    TestDatasetVersion.DataSetId,
                    TestDatasetVersion.SemVersion(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(TestDatasetVersion);

        DataSetVersionService
            .Setup(mock => mock.DeleteVersion(TestDatasetVersion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        DataSetVersionService
            .Setup(mock =>
                mock.CreateNextVersion(
                    ReleaseFile.Id,
                    TestDatasetVersion.DataSetId,
                    null, // We don't create a patch version if the data set was just a draft data set
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DataSetVersionSummaryViewModel
                {
                    Id = Guid.NewGuid(),
                    Version = "2.0.1",
                    Status = DataSetVersionStatus.Processing,
                    Type = DataSetVersionType.Minor,
                    ReleaseVersion = new IdTitleViewModel(),
                    File = new IdTitleViewModel(),
                }
            );
    }

    public void SetupPatchDataSetVersionCreation()
    {
        DataSetVersionService
            .Setup(mock =>
                mock.GetDataSetVersion(
                    TestDatasetVersion.DataSetId,
                    TestDatasetVersion.SemVersion(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(TestDatasetVersion);

        DataSetVersionService
            .Setup(mock =>
                mock.CreateNextVersion(
                    ReleaseFile.Id,
                    TestDatasetVersion.DataSetId,
                    TestDatasetVersion.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DataSetVersionSummaryViewModel
                {
                    Id = Guid.NewGuid(),
                    Version = "2.0.1",
                    Status = DataSetVersionStatus.Processing,
                    Type = DataSetVersionType.Minor,
                    ReleaseVersion = new IdTitleViewModel(),
                    File = new IdTitleViewModel(),
                }
            );
    }

    private void SetupMockBulkPatchDataSetVersionCreation(
        DataSetVersion[] dataSetVersions,
        ReleaseFile[] releaseFiles
    )
    {
        foreach (var dataSetVersion in dataSetVersions)
        {
            DataSetVersionService
                .Setup<Task<Either<ActionResult, DataSetVersion>>>(mock =>
                    mock.GetDataSetVersion(
                        dataSetVersion.DataSetId,
                        new SemVersion(2, 0, 0),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(dataSetVersion);
        }

        var releaseFilesAndVersions = releaseFiles.Zip(
            dataSetVersions,
            (releaseFile, dataSetVersion) => (releaseFile, dataSetVersion)
        );
        foreach (var releaseFileAndVersion in releaseFilesAndVersions)
        {
            DataSetVersionService
                .Setup(mock =>
                    mock.CreateNextVersion(
                        releaseFileAndVersion.releaseFile.Id,
                        releaseFileAndVersion.releaseFile.PublicApiDataSetId!.Value,
                        releaseFileAndVersion.dataSetVersion.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new DataSetVersionSummaryViewModel
                    {
                        Id = Guid.NewGuid(),
                        Version = "2.0.1",
                        Status = DataSetVersionStatus.Processing,
                        Type = DataSetVersionType.Minor,
                        ReleaseVersion = new IdTitleViewModel(),
                        File = new IdTitleViewModel(),
                    }
                );
        }
    }
}
