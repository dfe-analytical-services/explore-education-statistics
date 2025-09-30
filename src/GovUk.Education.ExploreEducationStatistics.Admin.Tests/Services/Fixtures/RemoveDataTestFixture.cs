#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Fixtures;

public class RemoveDataSetTestFixture
{
    public Mock<ILogger<ReleaseVersionService>> Logger { get; set; }
    public Mock<IReleaseDataFileService> ReleaseDataFileService { get; set; }
    public Mock<IDataImportService> DataImportService { get; set; }
    public Mock<IDataSetVersionService> DataSetVersionService { get; set; }
    public Mock<IFootnoteRepository> FootnoteRepository { get; set; }
    public Mock<IDataBlockService> DataBlockService { get; set; }
    public Mock<IReleaseSubjectRepository> ReleaseSubjectRepository { get; set; }
    public Mock<IPrivateBlobCacheService> PrivateCacheService { get; set; }
    public ReleaseVersion ReleaseVersion { get; private set; } = null!;
    public Subject Subject { get; private set; } = null!;
    public File File { get; private set; } = null!;
    public ReleaseFile ReleaseFile { get; private set; } = null!;
    public DataSetVersion TestDataSetVersion { get; set; }
    public DataSet TestDataSet { get; set; }

    private RemoveDataSetTestFixture(
        Mock<IReleaseDataFileService>? releaseDataFileService = null,
        Mock<IDataImportService>? dataImportService = null,
        Mock<IDataSetVersionService>? dataSetVersionService = null,
        Mock<IFootnoteRepository>? footnoteRepository = null,
        Mock<IDataBlockService>? dataBlockService = null,
        Mock<IReleaseSubjectRepository>? releaseSubjectRepository = null,
        Mock<IPrivateBlobCacheService>? privateBlobCacheService = null,
        Mock<ILogger<ReleaseVersionService>>? logger = null
    )
    {
        ReleaseDataFileService =
            releaseDataFileService ?? new Mock<IReleaseDataFileService>(Strict);
        DataImportService = dataImportService ?? new Mock<IDataImportService>(Strict);
        DataSetVersionService = dataSetVersionService ?? new Mock<IDataSetVersionService>(Strict);
        FootnoteRepository = footnoteRepository ?? new Mock<IFootnoteRepository>(Strict);
        DataBlockService = dataBlockService ?? new Mock<IDataBlockService>(Strict);
        ReleaseSubjectRepository =
            releaseSubjectRepository ?? new Mock<IReleaseSubjectRepository>(Strict);
        PrivateCacheService = privateBlobCacheService ?? new Mock<IPrivateBlobCacheService>(Strict);
        Logger = logger ?? new Mock<ILogger<ReleaseVersionService>>(Strict);
    }

    public static async Task<RemoveDataSetTestFixture> CreateApiLinkedValidationError(
        DataFixture dataFixture,
        DataSetVersionStatus dataSetVersionStatus,
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext
    )
    {
        var instance = await InitializeApiLinkedData(
            dataFixture,
            dataSetVersionStatus,
            statisticsDbContext,
            contentDbContext
        );

        instance.SetUpMocksForApiLinkedValidationErrors();

        return instance;
    }

    public static async Task<RemoveDataSetTestFixture> CreateApiLinkedThrows404VersionException(
        DataFixture dataFixture,
        DataSetVersionStatus dataSetVersionStatus,
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext
    )
    {
        var instance = await InitializeApiLinkedData(
            dataFixture,
            dataSetVersionStatus,
            statisticsDbContext,
            contentDbContext
        );

        instance.SetUpMocksForApiLinkedExceptions();

        return instance;
    }

    public static async Task<RemoveDataSetTestFixture> CreateApiLinkedToRelease(
        DataFixture dataFixture,
        DataSetVersionStatus dataSetVersionStatus,
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext,
        Guid? replacedById = null,
        Guid? replacingId = null,
        bool releaseVersionPublished = false
    )
    {
        var instance = await InitializeApiLinkedData(
            dataFixture,
            dataSetVersionStatus,
            statisticsDbContext,
            contentDbContext
        );

        instance.SetupApiLinkedDataFixtures(
            dataFixture,
            dataSetVersionStatus,
            isPublished: releaseVersionPublished
        );
        instance.SetupApiLinkedReplaceByIds(replacedById, replacingId);
        await instance.SetupStatsAndContentDbContext(statisticsDbContext, contentDbContext);
        instance.SetUpMocksForCheckApiLinkedServices();
        instance.SetUpMocksForApiLinkedValidationErrors();

        return instance;
    }

    private void SetUpMocksForApiLinkedExceptions()
    {
        SetUpMocksForApiLinkedValidationErrors();
        DataSetVersionService
            .Setup(service =>
                service.GetDataSetVersion(
                    ReleaseFile.PublicApiDataSetId!.Value,
                    ReleaseFile.PublicApiDataSetVersion!,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new NotFoundResult());
        Logger
            .Setup(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            )
            .Verifiable(Times.Once);
    }

    private void SetUpMocksForApiLinkedValidationErrors()
    {
        DataImportService
            .Setup(service => service.GetImport(File.Id))
            .ReturnsAsync(new DataImport { Status = DataImportStatus.COMPLETE });
        ReleaseSubjectRepository
            .Setup(service => service.DeleteReleaseSubject(ReleaseVersion.Id, Subject.Id, true))
            .Returns(Task.CompletedTask);
        ReleaseDataFileService
            .Setup(service => service.Delete(ReleaseVersion.Id, File.Id, false))
            .ReturnsAsync(Unit.Instance);
        PrivateCacheService
            .Setup(service =>
                service.DeleteItemAsync(
                    new PrivateSubjectMetaCacheKey(ReleaseVersion.Id, Subject.Id)
                )
            )
            .Returns(Task.CompletedTask);
    }

    private static async Task<RemoveDataSetTestFixture> InitializeApiLinkedData(
        DataFixture dataFixture,
        DataSetVersionStatus dataSetVersionStatus,
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext
    )
    {
        var instance = new RemoveDataSetTestFixture();
        instance.SetupApiLinkedDataFixtures(dataFixture, dataSetVersionStatus, isPublished: true);
        await instance.SetupStatsAndContentDbContext(statisticsDbContext, contentDbContext);
        return instance;
    }

    private void SetupApiLinkedReplaceByIds(Guid? replacedById, Guid? replacingId)
    {
        File.ReplacedById = replacedById;
        File.ReplacingId = replacingId;
    }

    private void SetupApiLinkedDataFixtures(
        DataFixture dataFixture,
        DataSetVersionStatus dataSetVersionStatus,
        bool isPublished = false
    )
    {
        TestDataSet = dataFixture.DefaultDataSet().Generate();
        TestDataSetVersion = dataFixture
            .DefaultDataSetVersion()
            .WithStatus(dataSetVersionStatus)
            .WithDataSet(TestDataSet)
            .Generate();
        ReleaseVersion = dataFixture.DefaultReleaseVersion().Generate();

        if (isPublished)
        {
            ReleaseVersion.Published = DateTime.Now.AddDays(-1);
        }

        Subject = dataFixture.DefaultSubject().Generate();
        File = dataFixture
            .DefaultFile()
            .WithSubjectId(Subject.Id)
            .WithType(FileType.Data)
            .Generate();
        ReleaseFile = dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(ReleaseVersion)
            .WithFile(File)
            .WithPublicApiDataSetId(TestDataSet.Id)
            .WithPublicApiDataSetVersion(TestDataSetVersion.SemVersion())
            .Generate();
    }

    private async Task SetupStatsAndContentDbContext(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext
    )
    {
        contentDbContext.ReleaseVersions.Add(ReleaseVersion);
        contentDbContext.ReleaseFiles.Add(ReleaseFile);
        await contentDbContext.SaveChangesAsync();

        statisticsDbContext.Subject.Add(Subject);
        await statisticsDbContext.SaveChangesAsync();
    }

    public void SetUpMocksForCheckApiLinkedServices()
    {
        var deleteDataBlockPlan = new DeleteDataBlockPlanViewModel();
        var footnote = new Footnote { Id = Guid.NewGuid() };
        DataSetVersionService
            .Setup(service =>
                service.GetDataSetVersion(
                    ReleaseFile.PublicApiDataSetId!.Value,
                    ReleaseFile.PublicApiDataSetVersion!,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(TestDataSetVersion);

        FootnoteRepository
            .Setup(service => service.GetFootnotes(ReleaseVersion.Id, Subject.Id))
            .ReturnsAsync([footnote]);

        DataBlockService
            .Setup(service =>
                service.GetDeletePlan(ReleaseVersion.Id, It.Is<Subject>(s => s.Id == Subject.Id))
            )
            .ReturnsAsync(deleteDataBlockPlan);

        DataBlockService
            .Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlanViewModel>()))
            .ReturnsAsync(Unit.Instance);
    }

    public void SetUpMocksForDeleteApiLinkedServices()
    {
        DataSetVersionService
            .Setup(service =>
                service.DeleteVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Unit.Instance);
    }
}
