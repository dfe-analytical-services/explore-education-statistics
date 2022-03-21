using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ReleasesControllerTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async Task Create_Release_Returns_Ok()
        {
            var returnedViewModel = new ReleaseViewModel();
            
            var mocks = Mocks();

            mocks
                .ReleaseService
                .Setup(s => s.CreateRelease(It.IsAny<ReleaseCreateViewModel>()))
                .ReturnsAsync(returnedViewModel);
            
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.CreateRelease(new ReleaseCreateViewModel(), _publicationId);
            VerifyAllMocks(mocks.ReleaseService);
            
            result.AssertOkResult(returnedViewModel);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            var dataFileInfo = new DataFileInfo
            {
                Name = "Subject name",
            };

            mocks.ReleaseDataFilesService
                .Setup(service => service.Upload(_releaseId,
                    dataFile,
                    metaFile,
                    "test@example.com",
                    null,
                    "Subject name"))
                .ReturnsAsync(dataFileInfo);

            // Call the method under test
            var controller = ReleasesControllerWithMocks(mocks);
            var result = await controller.AddDataFilesAsync(releaseId: _releaseId,
                replacingFileId: null,
                subjectName: "Subject name",
                file: dataFile,
                metaFile: metaFile);
            VerifyAllMocks(mocks);
            
            var dataFileInfoResult = result.AssertOkResult();
            Assert.Equal("Subject name", dataFileInfoResult.Name);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_ValidationProblem()
        {
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            mocks.ReleaseDataFilesService
                .Setup(service => service.Upload(_releaseId,
                    dataFile,
                    metaFile,
                    "test@example.com",
                    null,
                    "Subject name"))
                .ReturnsAsync(ValidationActionResult(CannotOverwriteFile));

            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.AddDataFilesAsync(releaseId: _releaseId,
                replacingFileId: null,
                subjectName: "Subject name",
                file: dataFile,
                metaFile: metaFile);
            VerifyAllMocks(mocks);

            result.AssertBadRequest(CannotOverwriteFile);
        }

        [Fact]
        public async Task GetDataFilesAsync_Returns_A_List_Of_Files()
        {
            IEnumerable<DataFileInfo> testFiles = new[]
            {
                new DataFileInfo
                {
                    FileName = "file1.csv",
                    Name = "Release a file 1",
                    Size = "1 Kb"
                },
                new DataFileInfo
                {
                    FileName = "file2.csv",
                    Name = "Release a file 2",
                    Size = "1 Kb"
                }
            };

            var mocks = Mocks();

            mocks
                .ReleaseDataFilesService
                .Setup(s => s.ListAll(_releaseId))
                .ReturnsAsync(new Either<ActionResult, IEnumerable<DataFileInfo>>(testFiles));
            
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.GetDataFileInfo(_releaseId);
            VerifyAllMocks(mocks.ReleaseDataFilesService);

            var unboxed = result.AssertOkResult();
            Assert.Contains(unboxed, f => f.Name == "Release a file 1");
            Assert.Contains(unboxed, f => f.Name == "Release a file 2");
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_OK()
        {
            var mocks = Mocks();

            var fileId = Guid.NewGuid();

            mocks.ReleaseService.Setup(service => service.RemoveDataFiles(_releaseId, fileId))
                .ReturnsAsync(Unit.Instance);
            var controller = ReleasesControllerWithMocks(mocks);

            var result = await controller.DeleteDataFiles(_releaseId, fileId);
            VerifyAllMocks(mocks);

            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_ValidationProblem()
        {
            var mocks = Mocks();

            var fileId = Guid.NewGuid();

            mocks.ReleaseService
                .Setup(service => service.RemoveDataFiles(_releaseId, fileId))
                .ReturnsAsync(ValidationActionResult(UnableToFindMetadataFileToDelete));
            var controller = ReleasesControllerWithMocks(mocks);

            var result = await controller.DeleteDataFiles(_releaseId, fileId);
            VerifyAllMocks(mocks);

            result.AssertBadRequest(UnableToFindMetadataFileToDelete);
        }

        [Fact]
        public async Task UpdateRelease_Returns_Ok()
        {
            var mocks = Mocks();

            mocks.ReleaseService
                .Setup(s => s.UpdateRelease(
                    It.Is<Guid>(id => id.Equals(_releaseId)),
                    It.IsAny<ReleaseUpdateViewModel>())
                )
                .ReturnsAsync(new ReleaseViewModel {Id = _releaseId});
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.UpdateRelease(new ReleaseUpdateViewModel(), _releaseId);
            VerifyAllMocks(mocks);

            var unboxed = result.AssertOkResult();
            Assert.Equal(_releaseId, unboxed.Id);
        }

        [Fact]
        public async Task GetTemplateRelease_Returns_Ok()
        {
            var mocks = Mocks();
            var templateReleaseResult =
                new Either<ActionResult, TitleAndIdViewModel>(new TitleAndIdViewModel());
            mocks.ReleaseService
                .Setup(s => s.GetLatestPublishedRelease(It.Is<Guid>(id => id == _releaseId)))
                .Returns<Guid>(x => Task.FromResult(templateReleaseResult));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.GetTemplateRelease(_releaseId);
            VerifyAllMocks(mocks);

            result.AssertOkResult();
        }

        [Fact]
        public async Task CancelFileImport()
        {
            var mocks = Mocks();

            var fileId = Guid.NewGuid();

            mocks.ImportService
                .Setup(s => s.CancelImport(_releaseId, fileId))
                .ReturnsAsync(Unit.Instance);

            var controller = ReleasesControllerWithMocks(mocks);

            var result = await controller.CancelFileImport(_releaseId, fileId);
            VerifyAllMocks(mocks);
            
            result.AssertAccepted();
        }

        [Fact]
        public async Task CancelFileImportButNotAllowed()
        {
            var mocks = Mocks();
            
            var fileId = Guid.NewGuid();

            mocks.ImportService
                .Setup(s => s.CancelImport(_releaseId, fileId))
                .ReturnsAsync(new ForbidResult());

            var controller = ReleasesControllerWithMocks(mocks);

            var result = await controller.CancelFileImport(_releaseId, fileId);
            VerifyAllMocks(mocks);
            
            result.AssertForbidden();
        }

        [Fact]
        public async Task GetDeleteDataFilePlan()
        {
            var mocks = Mocks();
            
            var fileId = Guid.NewGuid();

            var deleteDataFilePlan = new DeleteDataFilePlan();
            
            mocks.ReleaseService
                .Setup(s => s.GetDeleteDataFilePlan(_releaseId, fileId))
                .ReturnsAsync(deleteDataFilePlan);

            var controller = ReleasesControllerWithMocks(mocks);

            var result = await controller.GetDeleteDataFilePlan(_releaseId, fileId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(deleteDataFilePlan);
        }

        [Fact]
        public async Task GetDeleteReleasePlan()
        {
            var mocks = Mocks();
            
            var deleteReleasePlan = new DeleteReleasePlan();
            
            mocks.ReleaseService
                .Setup(s => s.GetDeleteReleasePlan(_releaseId))
                .ReturnsAsync(deleteReleasePlan);

            var controller = ReleasesControllerWithMocks(mocks);

            var result = await controller.GetDeleteReleasePlan(_releaseId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(deleteReleasePlan);
        }
        
        [Fact]
        public async Task CreateReleaseStatus()
        {
            var releaseStatusViewModel = new ReleaseStatusCreateViewModel();
            var returnedReleaseViewModel = new ReleaseViewModel();
            
            var mocks = Mocks();

            mocks
                .ReleaseApprovalService
                .Setup(s => s.CreateReleaseStatus(_releaseId, releaseStatusViewModel))
                .ReturnsAsync(Unit.Instance);
            
            mocks
                .ReleaseService
                .Setup(s => s.GetRelease(_releaseId))
                .ReturnsAsync(returnedReleaseViewModel);
            
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.CreateReleaseStatus(releaseStatusViewModel, _releaseId);
            VerifyAllMocks(mocks);
            
            result.AssertOkResult(returnedReleaseViewModel);
        }

        private static IFormFile MockFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>(Strict);
            const string content = "test content";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            return fileMock.Object;
        }

        private static (
            Mock<IReleaseService> ReleaseService, 
            Mock<IReleaseApprovalService> ReleaseApprovalService, 
            Mock<IReleaseDataFileService> ReleaseDataFilesService, 
            Mock<IReleasePublishingStatusService> ReleasePublishingStatusService, 
            Mock<IReleaseChecklistService> ReleaseChecklistService, 
            Mock<IDataImportService> ImportService) Mocks()
        {
            return (new Mock<IReleaseService>(Strict),
                    new Mock<IReleaseApprovalService>(Strict),
                    new Mock<IReleaseDataFileService>(Strict),
                    new Mock<IReleasePublishingStatusService>(Strict),
                    new Mock<IReleaseChecklistService>(Strict),
                    new Mock<IDataImportService>(Strict)
                );
        }

        private static ReleasesController ReleasesControllerWithMocks(
            (
            Mock<IReleaseService> ReleaseService,
            Mock<IReleaseApprovalService> ReleaseApprovalService,
            Mock<IReleaseDataFileService> ReleaseDataFileService,
            Mock<IReleasePublishingStatusService> ReleaseStatusService,
            Mock<IReleaseChecklistService> ReleaseChecklistService,
            Mock<IDataImportService> ImportService
            ) mocks)
        {
            return new ReleasesController(
                mocks.ReleaseService.Object,
                mocks.ReleaseApprovalService.Object,
                mocks.ReleaseDataFileService.Object,
                mocks.ReleaseStatusService.Object,
                mocks.ReleaseChecklistService.Object,
                MockUserManager(new ApplicationUser
                {
                    Email = "test@example.com"
                }).Object,
                mocks.ImportService.Object);
        }
    }
}
