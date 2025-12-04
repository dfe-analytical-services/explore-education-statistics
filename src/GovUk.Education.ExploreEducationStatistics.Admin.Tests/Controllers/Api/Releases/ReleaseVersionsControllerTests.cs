#nullable enable
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using ErrorViewModel = GovUk.Education.ExploreEducationStatistics.Common.ViewModels.ErrorViewModel;
using File = System.IO.File;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Releases;

public class ReleaseVersionsControllerUnitTests
{
    private readonly Guid _releaseVersionId = Guid.NewGuid();

    [Fact]
    public async Task UploadDataSet_Success_ReturnsOkResult()
    {
        // Arrange
        var dataFile = MockFile("datafile.csv");
        var metaFile = MockFile("metafile.csv");

        var expectedVm = DataSetUploadMockBuilder.BuildViewModel();

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        releaseDataFileService
            .Setup(service =>
                service.Upload(
                    _releaseVersionId,
                    ItIsFileMatch(dataFile),
                    ItIsFileMatch(metaFile),
                    "Data set title",
                    default
                )
            )
            .ReturnsAsync(new List<DataSetUploadViewModel> { expectedVm });

        // Act
        var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

        var response = await controller.UploadDataSet(
            new()
            {
                ReleaseVersionId = _releaseVersionId,
                Title = "Data set title",
                DataFile = dataFile,
                MetaFile = metaFile,
            },
            cancellationToken: default
        );

        // Assert
        VerifyAllMocks(releaseDataFileService);

        var responseVms = response.AssertOkResult();
        var responseVm = Assert.Single(responseVms);
        Assert.Equivalent(expectedVm, responseVm);
    }

    [Fact]
    public async Task UploadDataSet_InvalidRequest_ReturnsValidationProblems()
    {
        // Arrange
        var dataFile = MockFile("datafile.csv");
        var metaFile = MockFile("metafile.csv");

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        releaseDataFileService
            .Setup(service =>
                service.Upload(
                    _releaseVersionId,
                    ItIsFileMatch(dataFile),
                    ItIsFileMatch(metaFile),
                    "Data set title",
                    default
                )
            )
            .ReturnsAsync(ValidationActionResult(CannotOverwriteFile));

        var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

        // Act
        var result = await controller.UploadDataSet(
            new()
            {
                ReleaseVersionId = _releaseVersionId,
                Title = "Data set title",
                DataFile = dataFile,
                MetaFile = metaFile,
            },
            cancellationToken: default
        );

        // Assert
        VerifyAllMocks(releaseDataFileService);

        result.AssertValidationProblem(CannotOverwriteFile);
    }

    [Fact]
    public async Task UploadDataSetAsZip_Success_ReturnsOkResult()
    {
        // Arrange
        var dataSetZipFile = MockFile("dataSetZip.zip", isZip: true);

        var expectedVm = DataSetUploadMockBuilder.BuildViewModel();

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        releaseDataFileService
            .Setup(service =>
                service.UploadFromZip(_releaseVersionId, It.IsAny<IManagedStreamZipFile>(), "Data set title", default)
            )
            .ReturnsAsync(new List<DataSetUploadViewModel> { expectedVm });

        // Act
        var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

        var response = await controller.UploadDataSetAsZip(
            new()
            {
                ReleaseVersionId = _releaseVersionId,
                Title = "Data set title",
                ZipFile = dataSetZipFile,
            },
            cancellationToken: default
        );

        // Assert
        VerifyAllMocks(releaseDataFileService);

        var responseVms = response.AssertOkResult();
        var responseVm = Assert.Single(responseVms);
        Assert.Equivalent(expectedVm, responseVm);
    }

    [Fact]
    public async Task UploadDataSetAsBulkZip_Success_ReturnsOkResult()
    {
        // Arrange
        var dataSetBulkZipFile = MockFile("dataSetZip.zip", isZip: true);

        var expectedVm = DataSetUploadMockBuilder.BuildViewModel();

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        releaseDataFileService
            .Setup(service => service.UploadFromBulkZip(_releaseVersionId, It.IsAny<IManagedStreamZipFile>(), default))
            .ReturnsAsync(new List<DataSetUploadViewModel> { expectedVm });

        // Act
        var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

        var response = await controller.UploadDataSetAsBulkZip(
            new() { ReleaseVersionId = _releaseVersionId, ZipFile = dataSetBulkZipFile },
            cancellationToken: default
        );

        // Assert
        VerifyAllMocks(releaseDataFileService);

        var responseVms = response.AssertOkResult();
        var responseVm = Assert.Single(responseVms);
        Assert.Equivalent(expectedVm, responseVm);
    }

    [Fact]
    public async Task DeleteDataSetUpload_Success_ReturnsNoContent()
    {
        // Arrange
        var dataSetUploadId = Guid.NewGuid();

        var dataSetUploadRepository = new Mock<IDataSetUploadRepository>(Strict);

        dataSetUploadRepository
            .Setup(mock => mock.Delete(_releaseVersionId, dataSetUploadId, default))
            .ReturnsAsync(Unit.Instance);

        var controller = BuildController(dataSetUploadRepository: dataSetUploadRepository.Object);

        // Act
        var response = await controller.DeleteDataSetUpload(
            _releaseVersionId,
            dataSetUploadId,
            cancellationToken: default
        );

        // Assert
        VerifyAllMocks(dataSetUploadRepository);
        response.AssertNoContent();
    }

    [Fact]
    public async Task ImportDataSetsFromTempStorage_Success_ReturnsNoContent()
    {
        // Arrange
        var expectedVm1 = DataSetUploadMockBuilder.BuildViewModel();
        var expectedVm2 = DataSetUploadMockBuilder.BuildViewModel();
        var expectedVm3 = DataSetUploadMockBuilder.BuildViewModel();

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        releaseDataFileService
            .Setup(service =>
                service.SaveDataSetsFromTemporaryBlobStorage(
                    _releaseVersionId,
                    new List<Guid> { expectedVm1.Id, expectedVm2.Id, expectedVm3.Id },
                    default
                )
            )
            .ReturnsAsync(Unit.Instance);

        // Act
        var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

        var response = await controller.ImportDataSetsFromTempStorage(
            _releaseVersionId,
            [expectedVm1.Id, expectedVm2.Id, expectedVm3.Id],
            cancellationToken: default
        );

        // Assert
        VerifyAllMocks(releaseDataFileService);
        response.AssertNoContent();
    }

    [Fact]
    public async Task GetDataFileInfo_Returns_A_List_Of_Files()
    {
        // Arrange
        var testFiles = new List<DataFileInfo>
        {
            new()
            {
                FileName = "file1.csv",
                Name = "Release a file 1",
                Size = "1 Kb",
            },
            new()
            {
                FileName = "file2.csv",
                Name = "Release a file 2",
                Size = "1 Kb",
            },
        };

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);

        releaseDataFileService.Setup(s => s.ListAll(_releaseVersionId)).ReturnsAsync(testFiles);

        var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

        // Act
        var result = await controller.GetDataFileInfo(_releaseVersionId);

        // Assert
        VerifyAllMocks(releaseDataFileService);

        var unboxed = result.AssertOkResult();
        Assert.Contains(unboxed, f => f.Name == "Release a file 1");
        Assert.Contains(unboxed, f => f.Name == "Release a file 2");
    }

    [Fact]
    public async Task DeleteDataFiles_Success()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var fileId = Guid.NewGuid();

        releaseVersionService
            .Setup(service => service.RemoveDataFiles(_releaseVersionId, fileId))
            .ReturnsAsync(Unit.Instance);

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.DeleteDataFiles(releaseVersionId: _releaseVersionId, fileId: fileId);

        // Assert
        VerifyAllMocks(releaseVersionService);

        Assert.IsAssignableFrom<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDataFiles_Fail_UnableToFindMetaFileToDelete()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var fileId = Guid.NewGuid();

        releaseVersionService
            .Setup(service => service.RemoveDataFiles(_releaseVersionId, fileId))
            .ReturnsAsync(ValidationActionResult(UnableToFindMetadataFileToDelete));

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.DeleteDataFiles(releaseVersionId: _releaseVersionId, fileId: fileId);

        // Assert
        VerifyAllMocks(releaseVersionService);

        result.AssertValidationProblem(UnableToFindMetadataFileToDelete);
    }

    [Fact]
    public async Task UpdateReleaseVersion_Returns_Ok()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        releaseVersionService
            .Setup(s =>
                s.UpdateReleaseVersion(
                    It.Is<Guid>(id => id.Equals(_releaseVersionId)),
                    It.IsAny<ReleaseVersionUpdateRequest>()
                )
            )
            .ReturnsAsync(new ReleaseVersionViewModel { Id = _releaseVersionId });

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.UpdateReleaseVersion(new ReleaseVersionUpdateRequest(), _releaseVersionId);

        // Assert
        VerifyAllMocks(releaseVersionService);

        var unboxed = result.AssertOkResult();
        Assert.Equal(_releaseVersionId, unboxed.Id);
    }

    [Fact]
    public async Task GetTemplateRelease_Returns_Ok()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var templateReleaseResult = new Either<ActionResult, IdTitleViewModel>(new IdTitleViewModel());

        releaseVersionService
            .Setup(s => s.GetLatestPublishedRelease(It.Is<Guid>(id => id == _releaseVersionId)))
            .ReturnsAsync(templateReleaseResult);

        var controller = BuildController(releaseVersionService.Object);

        // Act
        var result = await controller.GetTemplateRelease(_releaseVersionId);

        // Assert
        VerifyAllMocks(releaseVersionService);

        result.AssertOkResult();
    }

    [Fact]
    public async Task CancelFileImport()
    {
        // Arrange
        var importService = new Mock<IDataImportService>(Strict);

        var fileId = Guid.NewGuid();

        importService.Setup(s => s.CancelImport(_releaseVersionId, fileId)).ReturnsAsync(Unit.Instance);

        var controller = BuildController(importService: importService.Object);

        // Act
        var result = await controller.CancelFileImport(releaseVersionId: _releaseVersionId, fileId: fileId);

        // Assert
        VerifyAllMocks(importService);

        result.AssertAccepted();
    }

    [Fact]
    public async Task CancelFileImport_Fail_Forbidden()
    {
        // Arrange
        var importService = new Mock<IDataImportService>(Strict);

        var fileId = Guid.NewGuid();

        importService.Setup(s => s.CancelImport(_releaseVersionId, fileId)).ReturnsAsync(new ForbidResult());

        var controller = BuildController(importService: importService.Object);

        // Act
        var result = await controller.CancelFileImport(releaseVersionId: _releaseVersionId, fileId: fileId);

        // Assert
        VerifyAllMocks(importService);

        result.AssertForbidden();
    }

    [Fact]
    public async Task GetDeleteDataFilePlan()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var fileId = Guid.NewGuid();

        var deleteDataFilePlan = new DeleteDataFilePlanViewModel();

        releaseVersionService
            .Setup(s => s.GetDeleteDataFilePlan(_releaseVersionId, fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteDataFilePlan);

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.GetDeleteDataFilePlan(releaseVersionId: _releaseVersionId, fileId: fileId);

        // Assert
        VerifyAllMocks(releaseVersionService);

        result.AssertOkResult(deleteDataFilePlan);
    }

    [Fact]
    public async Task GetDeleteReleaseVersionPlan()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var deleteReleasePlan = new DeleteReleasePlanViewModel();

        releaseVersionService
            .Setup(s => s.GetDeleteReleaseVersionPlan(_releaseVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteReleasePlan);

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.GetDeleteReleaseVersionPlan(_releaseVersionId, It.IsAny<CancellationToken>());

        // Assert
        VerifyAllMocks(releaseVersionService);

        result.AssertOkResult(deleteReleasePlan);
    }

    [Fact]
    public async Task DeleteReleaseVersion_Returns_NoContent()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        releaseVersionService
            .Setup(service => service.DeleteReleaseVersion(_releaseVersionId, default))
            .ReturnsAsync(Unit.Instance);

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.DeleteReleaseVersion(_releaseVersionId, default);

        // Assert
        VerifyAllMocks(releaseVersionService);

        Assert.IsAssignableFrom<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteReleaseVersion_Returns_NotFound()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        releaseVersionService
            .Setup(service => service.DeleteReleaseVersion(_releaseVersionId, default))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.DeleteReleaseVersion(_releaseVersionId, default);

        // Assert
        VerifyAllMocks(releaseVersionService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task DeleteReleaseVersion_Returns_ValidationProblem()
    {
        // Arrange
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        releaseVersionService
            .Setup(service => service.DeleteReleaseVersion(_releaseVersionId, default))
            .ReturnsAsync(
                ValidationUtils.ValidationResult(new ErrorViewModel { Code = "error code", Path = "error path" })
            );

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.DeleteReleaseVersion(_releaseVersionId, default);

        // Assert
        VerifyAllMocks(releaseVersionService);

        var validationProblem = result.AssertBadRequestWithValidationProblem();

        validationProblem.AssertHasError(expectedPath: "error path", expectedCode: "error code");
    }

    [Fact]
    public async Task CreateReleaseStatus()
    {
        // Arrange
        var releaseApprovalService = new Mock<IReleaseApprovalService>(Strict);
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var request = new ReleaseStatusCreateRequest();
        var returnedReleaseViewModel = new ReleaseVersionViewModel();

        releaseApprovalService
            .Setup(s => s.CreateReleaseStatus(_releaseVersionId, request))
            .ReturnsAsync(Unit.Instance);

        releaseVersionService.Setup(s => s.GetRelease(_releaseVersionId)).ReturnsAsync(returnedReleaseViewModel);

        var controller = BuildController(
            releaseApprovalService: releaseApprovalService.Object,
            releaseVersionService: releaseVersionService.Object
        );

        // Act
        var result = await controller.CreateReleaseStatus(request, _releaseVersionId);

        // Assert
        VerifyAllMocks(releaseApprovalService, releaseVersionService);

        result.AssertOkResult(returnedReleaseViewModel);
    }

    [Fact]
    public async Task ListReleasesForApproval()
    {
        // Arrange
        var releases = ListOf(new ReleaseVersionSummaryViewModel { Id = Guid.NewGuid() });

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        releaseVersionService.Setup(s => s.ListUsersReleasesForApproval()).ReturnsAsync(releases);

        var controller = BuildController(releaseVersionService: releaseVersionService.Object);

        // Act
        var result = await controller.ListUsersReleasesForApproval();

        // Assert
        VerifyAllMocks(releaseVersionService);

        result.AssertOkResult(releases);
    }

    [Fact]
    public async Task CreateReleaseAmendment()
    {
        // Arrange
        var originalReleaseVersionId = Guid.NewGuid();

        var amendmentCreatedResponse = new IdViewModel(Guid.NewGuid());

        var releaseAmendmentService = new Mock<IReleaseAmendmentService>(Strict);

        releaseAmendmentService
            .Setup(s => s.CreateReleaseAmendment(originalReleaseVersionId))
            .ReturnsAsync(amendmentCreatedResponse);

        var controller = BuildController(releaseAmendmentService: releaseAmendmentService.Object);

        // Act
        var result = await controller.CreateReleaseAmendment(originalReleaseVersionId);

        // Assert
        VerifyAllMocks(releaseAmendmentService);

        result.AssertOkResult(amendmentCreatedResponse);
    }

    private static IFormFile MockFile(string fileName, bool isZip = false)
    {
        var fileMock = new Mock<IFormFile>(Strict);
        Stream stream;

        if (isZip)
        {
            stream = new MemoryStream();

            // Write a simple ZIP archive with one entry
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);
            var entry = archive.CreateEntry("data-file.csv");
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            writer.Write("test content");
        }
        else
        {
            stream = "test content".ToStream();
        }

        fileMock.Setup(formFile => formFile.OpenReadStream()).Returns(stream);
        fileMock.Setup(formFile => formFile.FileName).Returns(fileName);
        fileMock.Setup(formFile => formFile.Name).Returns(fileName);
        fileMock.Setup(formFile => formFile.Length).Returns(stream.Length);
        return fileMock.Object;
    }

    private static ReleaseVersionsController BuildController(
        IReleaseVersionService? releaseVersionService = null,
        IReleaseAmendmentService? releaseAmendmentService = null,
        IReleaseApprovalService? releaseApprovalService = null,
        IReleaseDataFileService? releaseDataFileService = null,
        IReleasePublishingStatusService? releaseStatusService = null,
        IReleaseChecklistService? releaseChecklistService = null,
        IDataImportService? importService = null,
        IDataSetUploadRepository? dataSetUploadRepository = null,
        IDataSetFileStorage? dataSetFileStorage = null
    )
    {
        return new ReleaseVersionsController(
            releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            releaseAmendmentService ?? Mock.Of<IReleaseAmendmentService>(Strict),
            releaseApprovalService ?? Mock.Of<IReleaseApprovalService>(Strict),
            releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
            releaseStatusService ?? Mock.Of<IReleasePublishingStatusService>(Strict),
            releaseChecklistService ?? Mock.Of<IReleaseChecklistService>(Strict),
            importService ?? Mock.Of<IDataImportService>(Strict),
            dataSetUploadRepository ?? Mock.Of<IDataSetUploadRepository>(Strict),
            dataSetFileStorage ?? Mock.Of<IDataSetFileStorage>(Strict)
        );
    }

    private static IManagedStreamFile ItIsFileMatch(IFormFile dataFile)
    {
        return It.Is<IManagedStreamFile>(file => file.Name == dataFile.Name && file.Length == dataFile.Length);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class ReleaseVersionsControllerIntegrationTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(ReleaseVersionsControllerIntegrationTestsFixture))]
public class ReleaseVersionsControllerIntegrationTestsCollection
    : ICollectionFixture<ReleaseVersionsControllerIntegrationTestsFixture>;

[Collection(nameof(ReleaseVersionsControllerIntegrationTestsFixture))]
public abstract class ReleaseVersionsControllerIntegrationTests
{
    private static readonly DataFixture DataFixture = new();

    public class UpdateReleaseTests(ReleaseVersionsControllerIntegrationTestsFixture fixture)
        : ReleaseVersionsControllerIntegrationTests
    {
        [Fact]
        public async Task Success()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var newYear = 2021;
            var newTimePeriodCoverage = TimeIdentifier.AcademicYearQ1;
            var newLabel = "initial";
            var newPreReleaseAccessList = "new-list";
            var expectedNewSlug = "2021-22-q1-initial";

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: newYear,
                timePeriodCoverage: newTimePeriodCoverage,
                label: newLabel,
                preReleaseAccessList: newPreReleaseAccessList
            );

            // Assert
            var viewModel = response.AssertOk<ReleaseVersionViewModel>();

            var updatedPublication = await fixture
                .GetContentDbContext()
                .Publications.Include(p => p.Releases)
                    .ThenInclude(r => r.Versions)
                .SingleAsync(p => p.Id == publication.Id);

            Assert.Equal(publication.Id, viewModel.PublicationId);
            Assert.Equal(newYear, viewModel.Year);
            Assert.Equal(newTimePeriodCoverage, viewModel.TimePeriodCoverage);
            Assert.Equal(expectedNewSlug, viewModel.Slug);

            var release = Assert.Single(updatedPublication.Releases);
            Assert.Equal(publication.Id, release.PublicationId);
            Assert.Equal(newYear, release.Year);
            Assert.Equal(newTimePeriodCoverage, release.TimePeriodCoverage);
            Assert.Equal(newLabel, release.Label);
            Assert.Equal(expectedNewSlug, release.Slug);

            var releaseVersion = Assert.Single(release.Versions);
            Assert.Equal(publication.Id, releaseVersion.Release.PublicationId);
            Assert.Equal(release.Id, releaseVersion.ReleaseId);

            var releaseSeriesItem = Assert.Single(updatedPublication.ReleaseSeries);
            Assert.Equal(release.Id, releaseSeriesItem.ReleaseId);
        }

        [Theory]
        [InlineData("initial", "initial", "2020-21-initial")]
        [InlineData("Initial", "Initial", "2020-21-initial")]
        [InlineData(" initial", "initial", "2020-21-initial")]
        [InlineData("initial ", "initial", "2020-21-initial")]
        [InlineData("initial 2", "initial 2", "2020-21-initial-2")]
        [InlineData("initial  2", "initial  2", "2020-21-initial-2")]
        [InlineData("", null, "2020-21")]
        [InlineData(" ", null, "2020-21")]
        [InlineData("  ", null, "2020-21")]
        [InlineData(null, null, "2020-21")]
        public async Task LabelAndSlugChanged(string? newLabel, string? expectedNewLabel, string expectedNewSlug)
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: newLabel
            );

            // Assert
            var viewModel = response.AssertOk<ReleaseVersionViewModel>();

            var updatedPublication = await fixture
                .GetContentDbContext()
                .Publications.Include(p => p.Releases)
                    .ThenInclude(r => r.Versions)
                .SingleAsync(p => p.Id == publication.Id);

            Assert.Equal(expectedNewSlug, viewModel.Slug);

            var release = Assert.Single(updatedPublication.Releases);
            Assert.Equal(expectedNewLabel, release.Label);
            Assert.Equal(expectedNewSlug, release.Slug);
        }

        [Fact]
        public async Task ReleaseVersionNotFirstVersion_YearChanged()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null)
                        .GenerateList(2)
                );

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[1].Id,
                year: 2021,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(UpdateRequestForPublishedReleaseVersionInvalid.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseVersionNotFirstVersion_TimePeriodCoverageChanged()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null)
                        .GenerateList(2)
                );

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[1].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYearQ1,
                label: null
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(UpdateRequestForPublishedReleaseVersionInvalid.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseVersionNotFirstVersion_LabelChanged()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null)
                        .GenerateList(2)
                );

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[1].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: "initial"
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(UpdateRequestForPublishedReleaseVersionInvalid.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseVersionIsPublished()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 1)]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null
            );

            // Assert
            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseVersionNotFound()
        {
            // Act
            var response = await UpdateRelease(
                releaseVersionId: Guid.NewGuid(),
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null
            );

            // Assert
            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null,
                user: OptimisedTestUsers.Authenticated
            );

            // Assert
            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseTypeInvalid()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null,
                type: ReleaseType.ExperimentalStatistics
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ValidationErrorMessages.ReleaseTypeInvalid.ToString(), error.Code);
        }

        [Theory]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "Initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " initial ", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, null, "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " ", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "  ", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYearQ1, "initial", "2020-21-q1-initial")]
        public async Task SlugNotUniqueToPublication(
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label,
            string existingReleaseSlug
        )
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .WithSlug(existingReleaseSlug)
                        .GenerateList(2)
                );

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(SlugNotUnique.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForNewSlugForDifferentReleaseInSamePublication()
        {
            Publication publication = DataFixture.DefaultPublication();

            Release targetRelease = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug("2020-21-initial")
                .WithPublication(publication);

            Release otherRelease = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("intermediate")
                .WithSlug("2020-21-intermediate")
                .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")])
                .WithPublication(publication);

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Releases.AddRange(targetRelease, otherRelease));

            var response = await UpdateRelease(
                releaseVersionId: targetRelease.Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: "final"
            );

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseSlugUsedByRedirect.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForNewSlugForReleaseInDifferentPublication()
        {
            Publication targetPublication = DataFixture.DefaultPublication();
            Release targetRelease = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug("2020-21-initial")
                .WithPublication(targetPublication);

            Publication otherPublication = DataFixture.DefaultPublication();
            Release otherRelease = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("intermediate")
                .WithSlug("2020-21-intermediate")
                .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")])
                .WithPublication(otherPublication);

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Releases.AddRange(targetRelease, otherRelease));

            var response = await UpdateRelease(
                releaseVersionId: targetRelease.Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: "final"
            );

            response.AssertOk();
        }

        [Fact]
        public async Task LabelOver50Characters()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UpdateRelease(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: new string('a', 51)
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(
                $"The field {nameof(ReleaseCreateRequest.Label)} must be a string or array type with a maximum length of '50'.",
                error.Message
            );
            Assert.Equal(nameof(ReleaseCreateRequest.Label), error.Path);
        }

        private async Task<HttpResponseMessage> UpdateRelease(
            Guid releaseVersionId,
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label = null,
            ReleaseType? type = ReleaseType.OfficialStatistics,
            string? preReleaseAccessList = "",
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var request = new
            {
                Type = type,
                Year = year,
                TimePeriodCoverage = new { Value = timePeriodCoverage.GetEnumValue() },
                Label = label,
                PreReleaseAccessList = preReleaseAccessList,
            };

            return await client.PatchAsJsonAsync($"api/releaseVersions/{releaseVersionId}", request);
        }
    }

    public class UploadDataSetTests(ReleaseVersionsControllerIntegrationTestsFixture fixture)
        : ReleaseVersionsControllerIntegrationTests
    {
        [Fact]
        public async Task UploadDataSet_InvalidRequest_ReturnsValidationProblems()
        {
            // Act
            var response = await UploadDataSet(
                releaseVersionId: Guid.Empty,
                dataSetTitle: "",
                dataFileName: "",
                metaFileName: ""
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            Assert.Equal("Must not be empty.", validationProblem.Errors[0].Message);
            Assert.Equal("Data set title cannot be empty", validationProblem.Errors[1].Message);
            Assert.Equal("File 'Data File' either empty or not found.", validationProblem.Errors[2].Message);
            Assert.Equal("File 'Meta File' either empty or not found.", validationProblem.Errors[3].Message);
        }

        [Fact(Skip = "EES-6171: Requires test setup for screener and storage containers")]
        public async Task UploadDataSet_ValidRequest_ReturnsViewModel()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UploadDataSet(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                dataSetTitle: "Test title",
                dataFileName: "test-data.csv",
                metaFileName: "test-data.meta.csv"
            );

            // Assert
            var uploadResult = response.AssertOk<List<DataSetUploadViewModel>>();
            var dataSet = Assert.Single(uploadResult);

            Assert.NotEqual(Guid.Empty, dataSet.Id);
            Assert.Equal("Test title", dataSet.DataSetTitle);
            Assert.Equal("test-data.csv", dataSet.DataFileName);
            Assert.Equal("434 Kb", dataSet.DataFileSize);
            Assert.Equal("test-data.meta.csv", dataSet.MetaFileName);
            Assert.Equal("157 Kb", dataSet.MetaFileSize);
            Assert.Equal("Pending import", dataSet.Status);
            Assert.Equal("", dataSet.UploadedBy);
            Assert.Equal("Passed", dataSet.ScreenerResult?.OverallResult);
            Assert.True(dataSet.PublicApiCompatible);
            Assert.Null(dataSet.ReplacingFileId);
        }

        [Fact(Skip = "EES-6171: Requires test setup for screener and storage containers")]
        public async Task UploadDataSetAsZip_ValidRequest_ReturnsDataFileInfo()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UploadDataSetAsZip(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                dataSetTitle: "Test title",
                fileName: "data-zip-valid.zip"
            );

            // Assert
            var uploadResult = response.AssertOk<List<DataSetUploadViewModel>>();
            var dataSet = Assert.Single(uploadResult);

            Assert.NotEqual(Guid.Empty, dataSet.Id);
            Assert.Equal("Test title", dataSet.DataSetTitle);
            Assert.Equal("csvfile.csv", dataSet.DataFileName);
            Assert.Equal("15933 Kb", dataSet.DataFileSize);
            Assert.Equal("csvfile.meta.csv", dataSet.MetaFileName);
            Assert.Equal("157 Kb", dataSet.MetaFileSize);
            Assert.Equal("Pending import", dataSet.Status);
            Assert.Equal("", dataSet.UploadedBy);
            Assert.Equal("Passed", dataSet.ScreenerResult?.OverallResult);
            Assert.True(dataSet.PublicApiCompatible);
            Assert.Null(dataSet.ReplacingFileId);
        }

        [Fact]
        public async Task UploadDataSetAsZip_InvalidRequest_ReturnsValidationProblems()
        {
            // Act
            var response = await UploadDataSetAsZip(releaseVersionId: Guid.Empty, dataSetTitle: "", fileName: "");

            // Assert
            var validationProblem = response.AssertValidationProblem();

            Assert.Equal("Must not be empty.", validationProblem.Errors[0].Message);
            Assert.Equal("Data set title cannot be empty", validationProblem.Errors[1].Message);
            Assert.Equal("File 'Zip File' either empty or not found.", validationProblem.Errors[2].Message);
        }

        [Fact(Skip = "EES-6171: Requires test setup for screener and storage containers")]
        public async Task UploadDataSetAsBulkZip_ValidRequest_ReturnsViewModel()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UploadDataSetAsBulkZip(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                fileName: "bulk-data-zip-valid.zip"
            );

            // Assert
            var uploadResultVms = response.AssertOk<List<DataSetUploadViewModel>>();
            var dataSet1 = uploadResultVms[0];
            var dataSet2 = uploadResultVms[1];

            Assert.NotEqual(Guid.Empty, dataSet1.Id);
            Assert.Equal("First data set", dataSet1.DataSetTitle);
            Assert.Equal("one.csv", dataSet1.DataFileName);
            Assert.Equal("696 Kb", dataSet1.DataFileSize);
            Assert.Equal("one.meta.csv", dataSet1.MetaFileName);
            Assert.Equal("210 Kb", dataSet1.MetaFileSize);
            Assert.Equal("Pending import", dataSet1.Status);
            Assert.Equal("", dataSet1.UploadedBy);
            Assert.Equal("Passed", dataSet1.ScreenerResult?.OverallResult);
            Assert.True(dataSet1.PublicApiCompatible);
            Assert.Null(dataSet1.ReplacingFileId);

            Assert.NotEqual(Guid.Empty, dataSet2.Id);
            Assert.Equal("Second data set", dataSet2.DataSetTitle);
            Assert.Equal("two.csv", dataSet2.DataFileName);
            Assert.Equal("2085 Kb", dataSet2.DataFileSize);
            Assert.Equal("two.meta.csv", dataSet2.MetaFileName);
            Assert.Equal("318 Kb", dataSet2.MetaFileSize);
            Assert.Equal("Pending import", dataSet2.Status);
            Assert.Equal("", dataSet2.UploadedBy);
            Assert.Equal("Passed", dataSet2.ScreenerResult?.OverallResult);
            Assert.True(dataSet2.PublicApiCompatible);
            Assert.Null(dataSet2.ReplacingFileId);
        }

        [Fact(Skip = "EES-6171: Requires test setup for screener and storage containers")]
        public async Task UploadDataSetAsBulkZip_ValidRequestWithReplacement_ReturnsViewModelWithReplacementId()
        {
            // Arrange
            var releaseFileToBeReplaced = DataFixture
                .DefaultReleaseFile()
                .WithName("First data set")
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .Generate();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.ReleaseFiles.Add(releaseFileToBeReplaced));

            // Act
            var response = await UploadDataSetAsBulkZip(
                releaseVersionId: releaseFileToBeReplaced.ReleaseVersionId,
                fileName: "bulk-data-zip-valid.zip"
            );

            // Assert
            var uploadResultVms = response.AssertOk<List<DataSetUploadViewModel>>();
            var dataSet1 = uploadResultVms[0];
            var dataSet2 = uploadResultVms[1];

            Assert.NotEqual(Guid.Empty, dataSet1.Id);
            Assert.Equal("First data set", dataSet1.DataSetTitle);
            Assert.Equal("one.csv", dataSet1.DataFileName);
            Assert.Equal("696 Kb", dataSet1.DataFileSize);
            Assert.Equal("one.meta.csv", dataSet1.MetaFileName);
            Assert.Equal("210 Kb", dataSet1.MetaFileSize);
            Assert.Equal("Pending import", dataSet1.Status);
            Assert.Equal("", dataSet1.UploadedBy);
            Assert.Equal("Passed", dataSet1.ScreenerResult?.OverallResult);
            Assert.True(dataSet1.PublicApiCompatible);
            Assert.NotNull(dataSet1.ReplacingFileId);

            Assert.NotEqual(Guid.Empty, dataSet2.Id);
            Assert.Equal("Second data set", dataSet2.DataSetTitle);
            Assert.Equal("two.csv", dataSet2.DataFileName);
            Assert.Equal("2085 Kb", dataSet2.DataFileSize);
            Assert.Equal("two.meta.csv", dataSet2.MetaFileName);
            Assert.Equal("318 Kb", dataSet2.MetaFileSize);
            Assert.Equal("Pending import", dataSet2.Status);
            Assert.Equal("", dataSet2.UploadedBy);
            Assert.True(dataSet2.PublicApiCompatible);
            Assert.Equal("Passed", dataSet1.ScreenerResult?.OverallResult);
            Assert.Null(dataSet2.ReplacingFileId);
        }

        [Fact]
        public async Task UploadDataSetAsBulkZip_InvalidRequest_ReturnsValidationProblems()
        {
            // Act
            var response = await UploadDataSetAsBulkZip(releaseVersionId: Guid.Empty, fileName: "");

            // Assert
            var validationProblem = response.AssertValidationProblem();

            Assert.Equal("Must not be empty.", validationProblem.Errors[0].Message);
            Assert.Equal("File 'Zip File' either empty or not found.", validationProblem.Errors[1].Message);
        }

        [Fact]
        public async Task UploadDataSetAsBulkZip_IndexFileMissing_ReturnsValidationProblems()
        {
            // Arrange
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel(null),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            // Act
            var response = await UploadDataSetAsBulkZip(
                releaseVersionId: publication.Releases[0].Versions[0].Id,
                fileName: "bulk-data-zip-invalid-no-datasetnames-csv.zip"
            );

            // Assert
            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(
                "For bulk imports, the ZIP must include dataset_names.csv",
                validationProblem.Errors[0].Message
            );
        }

        [Fact]
        public async Task ImportBulkZipDataSetsFromTempStorage_InvalidRequest_ReturnsValidationProblems()
        {
            // Act
            var response = await ImportBulkZipDataSetsFromTempStorage(releaseVersionId: Guid.Empty, dataSetFiles: []);

            // Assert
            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> UploadDataSet(
            Guid releaseVersionId,
            string dataSetTitle,
            string dataFileName,
            string metaFileName,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var dataFilePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources",
                dataFileName
            );

            var metaFilePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources",
                metaFileName
            );

            var multipartContent = new MultipartFormDataContent
            {
                { new StringContent(releaseVersionId.ToString()), "ReleaseVersionId" },
                { new StringContent(dataSetTitle), "Title" },
            };

            try
            {
                var dataFileContent = new ByteArrayContent(await File.ReadAllBytesAsync(dataFilePath));
                dataFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Csv);
                multipartContent.Add(dataFileContent, "DataFile", dataFileName);

                var metaFileContent = new ByteArrayContent(await File.ReadAllBytesAsync(metaFilePath));
                metaFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Csv);
                multipartContent.Add(metaFileContent, "MetaFile", metaFileName);
            }
            catch
            {
                multipartContent.Add(new ByteArrayContent([]), "DataFile", "empty.csv");
                multipartContent.Add(new ByteArrayContent([]), "MetaFile", "empty.csv");
            }

            return await client.PostAsync($"api/releaseVersions/data", multipartContent);
        }

        private async Task<HttpResponseMessage> UploadDataSetAsZip(
            Guid releaseVersionId,
            string dataSetTitle,
            string fileName,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var filePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources",
                fileName
            );

            var multipartContent = new MultipartFormDataContent
            {
                { new StringContent(releaseVersionId.ToString()), "ReleaseVersionId" },
                { new StringContent(dataSetTitle), "Title" },
            };

            try
            {
                var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Zip);
                multipartContent.Add(fileContent, "ZipFile", fileName);
            }
            catch
            {
                multipartContent.Add(new ByteArrayContent([]), "ZipFile", "empty.zip");
            }

            return await client.PostAsync($"api/releaseVersions/zip-data", multipartContent);
        }

        private async Task<HttpResponseMessage> UploadDataSetAsBulkZip(
            Guid releaseVersionId,
            string fileName,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var filePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources",
                fileName
            );

            var multipartContent = new MultipartFormDataContent
            {
                { new StringContent(releaseVersionId.ToString()), "ReleaseVersionId" },
            };

            try
            {
                var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Zip);
                multipartContent.Add(fileContent, "ZipFile", fileName);
            }
            catch
            {
                multipartContent.Add(new ByteArrayContent([]), "ZipFile", "empty.zip");
            }

            return await client.PostAsync($"api/releaseVersions/upload-bulk-zip-data", multipartContent);
        }

        private async Task<HttpResponseMessage> ImportBulkZipDataSetsFromTempStorage(
            Guid releaseVersionId,
            List<DataSetUploadViewModel> dataSetFiles,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);
            return await client.PostAsJsonAsync($"api/release/{releaseVersionId}/import-bulk-zip-data", dataSetFiles);
        }
    }
}
