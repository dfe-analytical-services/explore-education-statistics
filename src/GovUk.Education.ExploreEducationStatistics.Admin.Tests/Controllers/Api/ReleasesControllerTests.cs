#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ReleasesControllerTests
    {
        private readonly Guid _releaseVersionId = Guid.NewGuid();
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async Task Create_Release_Returns_Ok()
        {
            var returnedViewModel = new ReleaseViewModel();

            var releaseService = new Mock<IReleaseService>(Strict);

            releaseService
                .Setup(s => s.CreateRelease(It.IsAny<ReleaseCreateRequest>()))
                .ReturnsAsync(returnedViewModel);

            var controller = BuildController(releaseService.Object);

            // Call the method under test
            var result = await controller.CreateRelease(new ReleaseCreateRequest(), _publicationId);
            VerifyAllMocks(releaseService);

            result.AssertOkResult(returnedViewModel);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            var dataFileInfo = new DataFileInfo
            {
                Name = "Subject name",
                FileName = "subject-name.csv",
                Size = "1 Kb",
            };

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            releaseDataFileService
                .Setup(service => service.Upload(_releaseVersionId,
                    dataFile,
                    metaFile,
                    "Subject name",
                    null))
                .ReturnsAsync(dataFileInfo);

            // Call the method under test
            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            var result = await controller.AddDataFilesAsync(releaseVersionId: _releaseVersionId,
                replacingFileId: null,
                subjectName: "Subject name",
                file: dataFile,
                metaFile: metaFile);

            VerifyAllMocks(releaseDataFileService);

            var dataFileInfoResult = result.AssertOkResult();
            Assert.Equal("Subject name", dataFileInfoResult.Name);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_ValidationProblem()
        {
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            releaseDataFileService
                .Setup(service => service.Upload(_releaseVersionId,
                    dataFile,
                    metaFile,
                    "Subject name",
                    null))
                .ReturnsAsync(ValidationActionResult(CannotOverwriteFile));

            var controller = BuildController(releaseDataFileService: releaseDataFileService.Object);

            // Call the method under test
            var result = await controller.AddDataFilesAsync(releaseVersionId: _releaseVersionId,
                replacingFileId: null,
                subjectName: "Subject name",
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
        public async Task DeleteDataFilesAsync_Returns_OK()
        {
            var releaseService = new Mock<IReleaseService>(Strict);

            var fileId = Guid.NewGuid();

            releaseService
                .Setup(service => service.RemoveDataFiles(_releaseVersionId, fileId))
                .ReturnsAsync(Unit.Instance);

            var controller = BuildController(releaseService: releaseService.Object);

            var result = await controller.DeleteDataFiles(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(releaseService);

            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_ValidationProblem()
        {
            var releaseService = new Mock<IReleaseService>(Strict);

            var fileId = Guid.NewGuid();

            releaseService
                .Setup(service => service.RemoveDataFiles(_releaseVersionId, fileId))
                .ReturnsAsync(ValidationActionResult(UnableToFindMetadataFileToDelete));

            var controller = BuildController(releaseService: releaseService.Object);

            var result = await controller.DeleteDataFiles(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(releaseService);

            result.AssertValidationProblem(UnableToFindMetadataFileToDelete);
        }

        [Fact]
        public async Task UpdateRelease_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>(Strict);

            releaseService
                .Setup(s => s.UpdateReleaseVersion(
                    It.Is<Guid>(id => id.Equals(_releaseVersionId)),
                    It.IsAny<ReleaseUpdateRequest>())
                )
                .ReturnsAsync(new ReleaseViewModel { Id = _releaseVersionId });

            var controller = BuildController(releaseService: releaseService.Object);

            // Method under test
            var result = await controller.UpdateRelease(new ReleaseUpdateRequest(), _releaseVersionId);
            VerifyAllMocks(releaseService);

            var unboxed = result.AssertOkResult();
            Assert.Equal(_releaseVersionId, unboxed.Id);
        }

        [Fact]
        public async Task GetTemplateRelease_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>(Strict);

            var templateReleaseResult =
                new Either<ActionResult, IdTitleViewModel>(new IdTitleViewModel());

            releaseService
                .Setup(s => s.GetLatestPublishedRelease(It.Is<Guid>(id => id == _releaseVersionId)))
                .ReturnsAsync(templateReleaseResult);

            var controller = BuildController(releaseService.Object);

            // Method under test
            var result = await controller.GetTemplateRelease(_releaseVersionId);
            VerifyAllMocks(releaseService);

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
        public async Task CancelFileImportButNotAllowed()
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
            var releaseService = new Mock<IReleaseService>(Strict);

            var fileId = Guid.NewGuid();

            var deleteDataFilePlan = new DeleteDataFilePlan();

            releaseService
                .Setup(s => s.GetDeleteDataFilePlan(_releaseVersionId, fileId))
                .ReturnsAsync(deleteDataFilePlan);

            var controller = BuildController(releaseService: releaseService.Object);

            var result = await controller.GetDeleteDataFilePlan(releaseVersionId: _releaseVersionId,
                fileId: fileId);
            VerifyAllMocks(releaseService);

            result.AssertOkResult(deleteDataFilePlan);
        }

        [Fact]
        public async Task GetDeleteReleasePlan()
        {
            var releaseService = new Mock<IReleaseService>(Strict);

            var deleteReleasePlan = new DeleteReleasePlan();

            releaseService
                .Setup(s => s.GetDeleteReleasePlan(_releaseVersionId))
                .ReturnsAsync(deleteReleasePlan);

            var controller = BuildController(releaseService: releaseService.Object);

            var result = await controller.GetDeleteReleasePlan(_releaseVersionId);
            VerifyAllMocks(releaseService);

            result.AssertOkResult(deleteReleasePlan);
        }

        [Fact]
        public async Task CreateReleaseStatus()
        {
            var releaseApprovalService = new Mock<IReleaseApprovalService>(Strict);
            var releaseService = new Mock<IReleaseService>(Strict);

            var request = new ReleaseStatusCreateRequest();
            var returnedReleaseViewModel = new ReleaseViewModel();

            releaseApprovalService
                .Setup(s => s.CreateReleaseStatus(_releaseVersionId, request))
                .ReturnsAsync(Unit.Instance);

            releaseService
                .Setup(s => s.GetRelease(_releaseVersionId))
                .ReturnsAsync(returnedReleaseViewModel);

            var controller = BuildController(
                releaseApprovalService: releaseApprovalService.Object,
                releaseService: releaseService.Object);

            // Call the method under test
            var result = await controller.CreateReleaseStatus(request, _releaseVersionId);
            VerifyAllMocks(releaseApprovalService, releaseService);

            result.AssertOkResult(returnedReleaseViewModel);
        }

        [Fact]
        public async Task ListReleasesForApproval()
        {
            var releases = ListOf(new ReleaseSummaryViewModel
            {
                Id = Guid.NewGuid()
            });

            var releaseService = new Mock<IReleaseService>(Strict);

            releaseService
                .Setup(s => s.ListUsersReleasesForApproval())
                .ReturnsAsync(releases);

            var controller = BuildController(
                releaseService: releaseService.Object);

            var result = await controller.ListUsersReleasesForApproval();
            VerifyAllMocks(releaseService);

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

        private static IFormFile MockFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>(Strict);
            var stream = "test content".ToStream();
            fileMock.Setup(formFile => formFile.OpenReadStream()).Returns(stream);
            fileMock.Setup(formFile => formFile.FileName).Returns(fileName);
            fileMock.Setup(formFile => formFile.Length).Returns(stream.Length);
            return fileMock.Object;
        }

        private static ReleasesController BuildController(
            IReleaseService? releaseService = null,
            IReleaseAmendmentService? releaseAmendmentService = null,
            IReleaseApprovalService? releaseApprovalService = null,
            IReleaseDataFileService? releaseDataFileService = null,
            IReleasePublishingStatusService? releaseStatusService = null,
            IReleaseChecklistService? releaseChecklistService = null,
            IDataImportService? importService = null)
        {
            return new ReleasesController(
                releaseService ?? Mock.Of<IReleaseService>(Strict),
                releaseAmendmentService ?? Mock.Of<IReleaseAmendmentService>(Strict),
                releaseApprovalService ?? Mock.Of<IReleaseApprovalService>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releaseStatusService ?? Mock.Of<IReleasePublishingStatusService>(Strict),
                releaseChecklistService ?? Mock.Of<IReleaseChecklistService>(Strict),
                importService ?? Mock.Of<IDataImportService>(Strict));
        }
    }
}
