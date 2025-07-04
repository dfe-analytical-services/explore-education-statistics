#nullable enable
using static Moq.MockBehavior;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using System;
using System.IO;
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
    public Mock<IDataImportService> DataImportService { get; set; }
    public Mock<IReleaseVersionRepository> ReleaseVersionRepository { get; set; }
    public Mock<IReleaseDataFileRepository> ReleaseDataFileRepository { get; set; }
    public Mock<IPrivateBlobStorageService> PrivateBlobStorageService { get; set; }
    public Mock<IDataSetVersionService> DataSetVersionService { get; set; }
    public Mock<IDataSetService> DataSetService { get; set; }
    public ReleaseVersion ReleaseVersion { get; set; } = null!;
    private File DataFile { get; set; } = null!;
    public File DataFileReplace { get; private set; } = null!;
    private File MetaFile { get; set; } = null!;
    public ReleaseFile ReleaseFile { get; private set; } = null!;
    private ReleaseFile ReleaseFileReplace { get; set; } = null!;
    public DataSetVersion TestDatasetVersion { get; private set; } = null!;
    private Guid SubjectId { get; init; }
    public string ContentDbContextId { get; set; } = null!;

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

    public static async Task<DataSetFileStorageTestFixture> InitializeUploadDataSetTestFixture(
        DataFixture fixture,
        User user,
        string dataSetName,
        string dataFileName,
        string metaFileName,
        ContentDbContext contentDbContext,
        Guid contentDbContextId,
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
            SubjectId = Guid.NewGuid(),
            ContentDbContextId = contentDbContextId.ToString()
        };

        testFixture.SetupCommonData(fixture, user, dataFileName, metaFileName, isPublished, version);
        await testFixture.SetupContentDbContext(contentDbContext);
        testFixture.SetupCommonMocks(user, metaFileName, dataSetName);

        return testFixture;
    }

    private void SetupCommonData(
        DataFixture fixture,
        User user,
        string dataFileName,
        string metaFileName,
        bool isPublished,
        SemVersion dataSetVersionNumber)
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
    }

    private async Task SetupContentDbContext(ContentDbContext contentDbContext)
    {
        contentDbContext.ReleaseVersions.Add(ReleaseVersion);
        contentDbContext.ReleaseFiles.AddRange(ReleaseFile, ReleaseFileReplace);
        contentDbContext.Files.Add(MetaFile);
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

        ReleaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(ReleaseVersion.Id))
            .Returns(Task.FromResult(SubjectId));

        ReleaseVersionRepository
            .Setup(mock => mock.CreateStatisticsDbReleaseAndSubjectHierarchy(ReleaseVersion.Id))
            .Returns(Task.FromResult(SubjectId));

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
}
