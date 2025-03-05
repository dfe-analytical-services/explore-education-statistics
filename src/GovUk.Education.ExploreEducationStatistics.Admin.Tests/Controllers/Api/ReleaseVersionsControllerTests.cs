#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using ErrorViewModel = GovUk.Education.ExploreEducationStatistics.Common.ViewModels.ErrorViewModel;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ReleaseVersionsControllerUnitTests
    {
        private readonly Guid _releaseVersionId = Guid.NewGuid();
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async Task UploadDataSet_Success()
        {
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            var dataFileInfo = new DataFileInfo
            {
                Name = "Data set title",
                FileName = "data-set.csv",
                Size = "1 Kb",
            };

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            releaseDataFileService
                .Setup(service => service.Upload(_releaseVersionId,
                    dataFile,
                    metaFile,
                    "Data set title",
                    null))
                .ReturnsAsync(dataFileInfo);

            // Call the method under test
            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            var result = await controller.UploadDataSet(releaseVersionId: _releaseVersionId,
                replacingFileId: null,
                title: "Data set title",
                file: dataFile,
                metaFile: metaFile);

            VerifyAllMocks(releaseDataFileService);

            var dataFileInfoResult = result.AssertOkResult();
            Assert.Equal("Data set title", dataFileInfoResult.Name);
        }

        [Fact]
        public async Task UploadDataSet_Fail_ValidationProblem()
        {
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            releaseDataFileService
                .Setup(service => service.Upload(_releaseVersionId,
                    dataFile,
                    metaFile,
                    "Data set title",
                    null))
                .ReturnsAsync(ValidationActionResult(CannotOverwriteFile));

            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            // Call the method under test
            var result = await controller.UploadDataSet(releaseVersionId: _releaseVersionId,
                replacingFileId: null,
                title: "Data set title",
                file: dataFile,
                metaFile: metaFile);

            VerifyAllMocks(releaseDataFileService);

            result.AssertValidationProblem(CannotOverwriteFile);
        }

        [Fact]
        public async Task GetDataFileInfo_Returns_A_List_Of_Files()
        {
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
                }
            };

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);

            releaseDataFileService
                .Setup(s => s.ListAll(_releaseVersionId))
                .ReturnsAsync(testFiles);

            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            // Call the method under test
            var result = await controller.GetDataFileInfo(_releaseVersionId);
            VerifyAllMocks(releaseDataFileService);

            var unboxed = result.AssertOkResult();
            Assert.Contains(unboxed, f => f.Name == "Release a file 1");
            Assert.Contains(unboxed, f => f.Name == "Release a file 2");
        }

        [Fact]
        public async Task DeleteDataFiles_Success()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            var fileId = Guid.NewGuid();

            releaseVersionService
                .Setup(service => service.RemoveDataFiles(_releaseVersionId, fileId))
                .ReturnsAsync(Unit.Instance);

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.DeleteDataFiles(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(releaseVersionService);

            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDataFiles_Fail_UnableToFindMetaFileToDelete()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            var fileId = Guid.NewGuid();

            releaseVersionService
                .Setup(service => service.RemoveDataFiles(_releaseVersionId, fileId))
                .ReturnsAsync(ValidationActionResult(UnableToFindMetadataFileToDelete));

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.DeleteDataFiles(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(releaseVersionService);

            result.AssertValidationProblem(UnableToFindMetadataFileToDelete);
        }

        [Fact]
        public async Task UpdateReleaseVersion_Returns_Ok()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            releaseVersionService
                .Setup(s => s.UpdateReleaseVersion(
                    It.Is<Guid>(id => id.Equals(_releaseVersionId)),
                    It.IsAny<ReleaseVersionUpdateRequest>())
                )
                .ReturnsAsync(new ReleaseVersionViewModel { Id = _releaseVersionId });

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            // Method under test
            var result = await controller.UpdateReleaseVersion(new ReleaseVersionUpdateRequest(), _releaseVersionId);
            VerifyAllMocks(releaseVersionService);

            var unboxed = result.AssertOkResult();
            Assert.Equal(_releaseVersionId, unboxed.Id);
        }

        [Fact]
        public async Task GetTemplateRelease_Returns_Ok()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            var templateReleaseResult =
                new Either<ActionResult, IdTitleViewModel>(new IdTitleViewModel());

            releaseVersionService
                .Setup(s => s.GetLatestPublishedRelease(It.Is<Guid>(id => id == _releaseVersionId)))
                .ReturnsAsync(templateReleaseResult);

            var controller = BuildController(releaseVersionService.Object);

            // Method under test
            var result = await controller.GetTemplateRelease(_releaseVersionId);
            VerifyAllMocks(releaseVersionService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task CancelFileImport()
        {
            var importService = new Mock<IDataImportService>(Strict);

            var fileId = Guid.NewGuid();

            importService
                .Setup(s => s.CancelImport(_releaseVersionId, fileId))
                .ReturnsAsync(Unit.Instance);

            var controller = BuildController(importService: importService.Object);

            var result = await controller.CancelFileImport(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(importService);

            result.AssertAccepted();
        }

        [Fact]
        public async Task CancelFileImport_Fail_Forbidden()
        {
            var importService = new Mock<IDataImportService>(Strict);

            var fileId = Guid.NewGuid();

            importService
                .Setup(s => s.CancelImport(_releaseVersionId, fileId))
                .ReturnsAsync(new ForbidResult());

            var controller = BuildController(importService: importService.Object);

            var result = await controller.CancelFileImport(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(importService);

            result.AssertForbidden();
        }

        [Fact]
        public async Task GetDeleteDataFilePlan()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            var fileId = Guid.NewGuid();

            var deleteDataFilePlan = new DeleteDataFilePlanViewModel();

            releaseVersionService
                .Setup(s => s.GetDeleteDataFilePlan(_releaseVersionId, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteDataFilePlan);

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.GetDeleteDataFilePlan(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(releaseVersionService);

            result.AssertOkResult(deleteDataFilePlan);
        }

        [Fact]
        public async Task GetDeleteReleaseVersionPlan()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            var deleteReleasePlan = new DeleteReleasePlanViewModel();

            releaseVersionService
                .Setup(s => s.GetDeleteReleaseVersionPlan(_releaseVersionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteReleasePlan);

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.GetDeleteReleaseVersionPlan(_releaseVersionId, It.IsAny<CancellationToken>());
            VerifyAllMocks(releaseVersionService);

            result.AssertOkResult(deleteReleasePlan);
        }

        [Fact]
        public async Task DeleteReleaseVersion_Returns_NoContent()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            releaseVersionService
                .Setup(service => service.DeleteReleaseVersion(_releaseVersionId, default))
                .ReturnsAsync(Unit.Instance);

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.DeleteReleaseVersion(_releaseVersionId, default);
            VerifyAllMocks(releaseVersionService);

            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteReleaseVersion_Returns_NotFound()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            releaseVersionService
                .Setup(service => service.DeleteReleaseVersion(_releaseVersionId, default))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.DeleteReleaseVersion(_releaseVersionId, default);
            VerifyAllMocks(releaseVersionService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task DeleteReleaseVersion_Returns_ValidationProblem()
        {
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            releaseVersionService
                .Setup(service => service.DeleteReleaseVersion(_releaseVersionId, default))
                .ReturnsAsync(ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = "error code",
                    Path = "error path"
                }));

            var controller = BuildController(releaseVersionService: releaseVersionService.Object);

            var result = await controller.DeleteReleaseVersion(_releaseVersionId, default);
            VerifyAllMocks(releaseVersionService);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "error path",
                expectedCode: "error code");
        }

        [Fact]
        public async Task CreateReleaseStatus()
        {
            var releaseApprovalService = new Mock<IReleaseApprovalService>(Strict);
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            var request = new ReleaseStatusCreateRequest();
            var returnedReleaseViewModel = new ReleaseVersionViewModel();

            releaseApprovalService
                .Setup(s => s.CreateReleaseStatus(_releaseVersionId, request))
                .ReturnsAsync(Unit.Instance);

            releaseVersionService
                .Setup(s => s.GetRelease(_releaseVersionId))
                .ReturnsAsync(returnedReleaseViewModel);

            var controller = BuildController(
                releaseApprovalService: releaseApprovalService.Object,
                releaseVersionService: releaseVersionService.Object);

            // Call the method under test
            var result = await controller.CreateReleaseStatus(request, _releaseVersionId);
            VerifyAllMocks(releaseApprovalService, releaseVersionService);

            result.AssertOkResult(returnedReleaseViewModel);
        }

        [Fact]
        public async Task ListReleasesForApproval()
        {
            var releases = ListOf(new ReleaseVersionSummaryViewModel
            {
                Id = Guid.NewGuid()
            });

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

            releaseVersionService
                .Setup(s => s.ListUsersReleasesForApproval())
                .ReturnsAsync(releases);

            var controller = BuildController(
                releaseVersionService: releaseVersionService.Object);

            var result = await controller.ListUsersReleasesForApproval();
            VerifyAllMocks(releaseVersionService);

            result.AssertOkResult(releases);
        }

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            var originalReleaseVersionId = Guid.NewGuid();

            var amendmentCreatedResponse = new IdViewModel(Guid.NewGuid());

            var releaseAmendmentService = new Mock<IReleaseAmendmentService>(Strict);

            releaseAmendmentService
                .Setup(s => s.CreateReleaseAmendment(originalReleaseVersionId))
                .ReturnsAsync(amendmentCreatedResponse);

            var controller = BuildController(
                releaseAmendmentService: releaseAmendmentService.Object);

            var result = await controller.CreateReleaseAmendment(originalReleaseVersionId);
            VerifyAllMocks(releaseAmendmentService);

            result.AssertOkResult(amendmentCreatedResponse);
        }

        [Fact]
        public async Task UploadBulkZipDataSetsToTempStorage()
        {
            // Arrange
            var dataSetFiles = new List<ArchiveDataSetFileViewModel>();

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);

            releaseDataFileService
                .Setup(s => s.ValidateAndUploadBulkZip(
                    It.IsAny<Guid>(),
                    It.IsAny<IFormFile>(),
                    default))
                .ReturnsAsync(dataSetFiles);

            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            // Act
            var result = await controller.UploadBulkZipDataSetsToTempStorage(
                Guid.NewGuid(),
                MockFile("bulk.zip"),
                default);

            // Assert
            VerifyAllMocks(releaseDataFileService);

            result.AssertOkResult(dataSetFiles);
        }

        [Fact]
        public async Task ImportBulkZipDataSetsFromTempStorage()
        {
            // Arrange
            var dataFileInfo = new List<DataFileInfo>
            {
                new() { FileName = "one.csv", Name = "Data set title", Size = "1024" },
            };

            var importRequests = new List<ArchiveDataSetFileViewModel>
            {
                new(){ DataFileId = Guid.NewGuid(), MetaFileId = Guid.NewGuid(), Title = "Data set title", DataFilename = "one.csv", MetaFilename = "one.meta.csv", DataFileSize = 1024, MetaFileSize = 128 }
            };

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);

            releaseDataFileService
                .Setup(s => s.SaveDataSetsFromTemporaryBlobStorage(
                    It.IsAny<Guid>(),
                    It.IsAny<List<ArchiveDataSetFileViewModel>>(),
                    default))
                .ReturnsAsync(dataFileInfo);

            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            // Act
            var result = await controller.ImportBulkZipDataSetsFromTempStorage(
                Guid.NewGuid(),
                importRequests,
                default);

            // Assert
            VerifyAllMocks(releaseDataFileService);

            result.AssertOkResult(dataFileInfo);
        }

        private static IFormFile MockFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>(Strict);
            var stream = "test content".ToStream();
            fileMock.Setup(formFile => formFile.OpenReadStream()).Returns(stream);
            fileMock.Setup(formFile => formFile.FileName).Returns(fileName);
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
            IDataImportService? importService = null)
        {
            return new ReleaseVersionsController(
                releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
                releaseAmendmentService ?? Mock.Of<IReleaseAmendmentService>(Strict),
                releaseApprovalService ?? Mock.Of<IReleaseApprovalService>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releaseStatusService ?? Mock.Of<IReleasePublishingStatusService>(Strict),
                releaseChecklistService ?? Mock.Of<IReleaseChecklistService>(Strict),
                importService ?? Mock.Of<IDataImportService>(Strict));
        }
    }

    public abstract class ReleaseVersionsControllerIntegrationTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
    {
        public class UpdateReleaseTests(TestApplicationFactory testApp) : ReleaseVersionsControllerIntegrationTests(testApp)
        {
            [Fact]
            public async Task Success()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases(
                        [
                            DataFixture
                                .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                                .WithLabel(null)
                        ]);

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var newYear = 2021;
                var newTimePeriodCoverage = TimeIdentifier.AcademicYearQ1;
                var newLabel = "initial";
                var newPreReleaseAccessList = "new-list";
                var expectedNewSlug = "2021-22-q1-initial";

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: newYear,
                    timePeriodCoverage: newTimePeriodCoverage,
                    label: newLabel,
                    preReleaseAccessList: newPreReleaseAccessList);

                var viewModel = response.AssertOk<ReleaseVersionViewModel>();

                var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

                var updatedPublication = contentDbContext.Publications
                    .Include(p => p.Releases)
                    .ThenInclude(r => r.Versions)
                    .Single(p => p.Id == publication.Id);

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
                Assert.Equal(publication.Id, releaseVersion.PublicationId);
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
            public async Task LabelAndSlugChanged(
                string? newLabel,
                string? expectedNewLabel,
                string expectedNewSlug)
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases(
                        [
                            DataFixture
                                .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020)
                                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                                .WithLabel(null)
                        ]);

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: newLabel);

                var viewModel = response.AssertOk<ReleaseVersionViewModel>();

                var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

                var updatedPublication = contentDbContext.Publications
                    .Include(p => p.Releases)
                    .ThenInclude(r => r.Versions)
                    .Single(p => p.Id == publication.Id);

                Assert.Equal(expectedNewSlug, viewModel.Slug);

                var release = Assert.Single(updatedPublication.Releases);
                Assert.Equal(expectedNewLabel, release.Label);
                Assert.Equal(expectedNewSlug, release.Slug);
            }

            [Fact]
            public async Task ReleaseVersionNotFirstVersion_YearChanged()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases(
                        DataFixture
                            .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                            .WithLabel(null)
                            .GenerateList(2)
                        );

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[1].Id,
                    year: 2021,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: null);

                var validationProblem = response.AssertValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(UpdateRequestForPublishedReleaseVersionInvalid.ToString(), error.Code);
            }

            [Fact]
            public async Task ReleaseVersionNotFirstVersion_TimePeriodCoverageChanged()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases(
                        DataFixture
                            .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                            .WithLabel(null)
                            .GenerateList(2)
                        );

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[1].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYearQ1,
                    label: null);

                var validationProblem = response.AssertValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(UpdateRequestForPublishedReleaseVersionInvalid.ToString(), error.Code);
            }

            [Fact]
            public async Task ReleaseVersionNotFirstVersion_LabelChanged()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases(
                        DataFixture
                            .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                            .WithLabel(null)
                            .GenerateList(2)
                        );

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[1].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: "initial");

                var validationProblem = response.AssertValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(UpdateRequestForPublishedReleaseVersionInvalid.ToString(), error.Code);
            }

            [Fact]
            public async Task ReleaseVersionIsPublished()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases([
                        DataFixture
                            .DefaultRelease(publishedVersions: 1)
                        ]);

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: null);

                response.AssertForbidden();
            }

            [Fact]
            public async Task ReleaseVersionNotFound()
            {
                var response = await UpdateRelease(
                    releaseVersionId: Guid.NewGuid(),
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: null);

                response.AssertNotFound();
            }

            [Fact]
            public async Task UserDoesNotHavePermission()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases([
                        DataFixture
                            .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        ]);

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: null,
                    client: client);

                response.AssertForbidden();
            }

            [Fact]
            public async Task ReleaseTypeInvalid()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases([
                        DataFixture
                            .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        ]);

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: null,
                    type: ReleaseType.ExperimentalStatistics);

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
                string existingReleaseSlug)
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases(
                        DataFixture
                            .DefaultRelease(publishedVersions: 0, draftVersion: true)
                            .WithSlug(existingReleaseSlug)
                            .GenerateList(2)
                        );

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: year,
                    timePeriodCoverage: timePeriodCoverage,
                    label: label);

                var validationProblem = response.AssertValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(SlugNotUnique.ToString(), error.Code);
            }

            [Fact]
            public async Task LabelOver50Characters()
            {
                Publication publication = DataFixture.DefaultPublication()
                    .WithReleases([
                            DataFixture
                                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        ]);

                await TestApp.AddTestData<ContentDbContext>(
                    context => context.Publications.Add(publication));

                var response = await UpdateRelease(
                    releaseVersionId: publication.Releases[0].Versions[0].Id,
                    year: 2020,
                    timePeriodCoverage: TimeIdentifier.AcademicYear,
                    label: new string('a', 51));

                var validationProblem = response.AssertValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal($"The field {nameof(ReleaseCreateRequest.Label)} must be a string or array type with a maximum length of '50'.", error.Message);
                Assert.Equal(nameof(ReleaseCreateRequest.Label), error.Path);
            }

            private WebApplicationFactory<TestStartup> BuildApp(
                ClaimsPrincipal? user = null)
            {
                return TestApp.SetUser(user ?? DataFixture.BauUser());
            }

            private async Task<HttpResponseMessage> UpdateRelease(
                Guid releaseVersionId,
                int year,
                TimeIdentifier timePeriodCoverage,
                string? label = null,
                ReleaseType? type = ReleaseType.OfficialStatistics,
                string? preReleaseAccessList = "",
                HttpClient? client = null)
            {
                client ??= BuildApp().CreateClient();

                var request = new
                {
                    Type = type,
                    Year = year,
                    TimePeriodCoverage = new { Value = timePeriodCoverage.GetEnumValue() },
                    Label = label,
                    PreReleaseAccessList = preReleaseAccessList
                };

                return await client.PatchAsJsonAsync($"api/releaseVersions/{releaseVersionId}", request);
            }
        }
    }
}
