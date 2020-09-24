using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ReleasesControllerTests
    {
        private static readonly ApplicationUser ApplicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com"
        };

        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async void Create_Release_Returns_Ok()
        {
            var mocks = Mocks();

            mocks.ReleaseService.Setup(s => s.CreateReleaseAsync(It.IsAny<CreateReleaseViewModel>()))
                .ReturnsAsync(new Either<ActionResult, ReleaseViewModel>(new ReleaseViewModel()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.CreateReleaseAsync(new CreateReleaseViewModel(), _publicationId);
            AssertOkResult(result);
        }

        [Fact]
        public async Task AddAncillaryFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var testFile = new FileInfo
            {
                Extension = "doc",
                Name = "File name",
                Path = "ancillaryFile.doc",
                Size = "1 Kb"
            };

            var mocks = Mocks();
            var ancillaryFile = MockFile("ancillaryFile.doc");
            mocks.FileStorageService
                .Setup(service =>
                    service.UploadFile(_releaseId, ancillaryFile, "File name",
                        ReleaseFileTypes.Ancillary, false))
                .ReturnsAsync(new Either<ActionResult, FileInfo>(testFile));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.AddAncillaryFileAsync(_releaseId, "File name", ancillaryFile);
            AssertOkResult(result);
        }

        [Fact]
        public async Task GetAncillaryFilesAsync_Returns_A_List_Of_Files()
        {
            IEnumerable<FileInfo> testFiles = new[]
            {
                new FileInfo
                {
                    Extension = "doc",
                    Name = "Ancillary 1",
                    Path = "file1.doc",
                    Size = "1 Kb"
                },
                new FileInfo
                {
                    Extension = "doc",
                    Name = "Ancillary 2",
                    Path = "file2.doc",
                    Size = "1 Kb"
                }
            };
            var mocks = Mocks();
            mocks.FileStorageService.Setup(s => s.ListFiles(_releaseId, ReleaseFileTypes.Ancillary))
                .ReturnsAsync(new Either<ActionResult, IEnumerable<FileInfo>>(testFiles));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.GetAncillaryFilesAsync(_releaseId);
            AssertOkResult(result);
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
                Path = "datafile.csv"
            };

            mocks.FileStorageService
                .Setup(service => service.UploadDataFiles(_releaseId,
                    dataFile,
                    metaFile,
                    ApplicationUser.Email,
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
            var dataFileInfoResult = AssertOkResult(result);
            Assert.Equal("Subject name", dataFileInfoResult.Name);
            Assert.Equal("datafile.csv", dataFileInfoResult.Path);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_ValidationProblem()
        {
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            mocks.FileStorageService
                .Setup(service => service.UploadDataFiles(_releaseId,
                    dataFile,
                    metaFile,
                    ApplicationUser.Email,
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
            AssertValidationProblem(result, CannotOverwriteFile);
        }

        [Fact]
        public async Task GetDataFilesAsync_Returns_A_List_Of_Files()
        {
            IEnumerable<DataFileInfo> testFiles = new[]
            {
                new DataFileInfo
                {
                    Extension = "csv",
                    Name = "Release a file 1",
                    Path = "file1.csv",
                    Size = "1 Kb"
                },
                new DataFileInfo
                {
                    Extension = "csv",
                    Name = "Release a file 2",
                    Path = "file2.csv",
                    Size = "1 Kb"
                }
            };

            var mocks = Mocks();

            mocks.FileStorageService.Setup(s => s.ListDataFiles(_releaseId))
                .ReturnsAsync(new Either<ActionResult, IEnumerable<DataFileInfo>>(testFiles));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.GetDataFilesAsync(_releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Contains(unboxed, f => f.Name == "Release a file 1");
            Assert.Contains(unboxed, f => f.Name == "Release a file 2");
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_OK()
        {
            var mocks = Mocks();

            var releaseFileReferenceId = Guid.NewGuid();

            mocks.ReleaseService
                .Setup(service => service.RemoveDataFilesAsync(_releaseId, releaseFileReferenceId))
                .ReturnsAsync(Unit.Instance);
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.DeleteDataFiles(_releaseId, releaseFileReferenceId);
            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_ValidationProblem()
        {
            var mocks = Mocks();

            var releaseFileReferenceId = Guid.NewGuid();

            mocks.ReleaseService
                .Setup(service => service.RemoveDataFilesAsync(_releaseId, releaseFileReferenceId))
                .ReturnsAsync(ValidationActionResult(UnableToFindMetadataFileToDelete));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.DeleteDataFiles(_releaseId, releaseFileReferenceId);
            AssertValidationProblem(result, UnableToFindMetadataFileToDelete);
        }

        [Fact]
        public async void UpdateRelease_Returns_Ok()
        {
            var mocks = Mocks();

            mocks.ReleaseService
                .Setup(s => s.UpdateRelease(
                    It.Is<Guid>(id => id.Equals(_releaseId)),
                    It.IsAny<UpdateReleaseViewModel>())
                )
                .ReturnsAsync(new ReleaseViewModel {Id = _releaseId});
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.UpdateRelease(new UpdateReleaseViewModel(), _releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Equal(_releaseId, unboxed.Id);
        }

        [Fact]
        public async void Get_Releases_For_Publication_Returns_Ok()
        {
            var mocks = Mocks();
            var templateReleaseResult =
                new Either<ActionResult, TitleAndIdViewModel>(new TitleAndIdViewModel());
            mocks.ReleaseService
                .Setup(s => s.GetLatestReleaseAsync(It.Is<Guid>(id => id == _releaseId)))
                .Returns<Guid>(x => Task.FromResult(templateReleaseResult));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.GetTemplateReleaseAsync(_releaseId);
            AssertOkResult(result);
        }

        private static IFormFile MockFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>();
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

        private static T AssertOkResult<T>(ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<T>(result.Value);
            return result.Value;
        }

        private static (
            Mock<IReleaseService> ReleaseService,
            Mock<IReleaseFilesService> FileStorageService,
            Mock<IReleaseStatusService> ReleaseStatusService,
            Mock<UserManager<ApplicationUser>> UserManager,
            Mock<IDataBlockService> DataBlockService) Mocks()
        {
            return (new Mock<IReleaseService>(),
                    new Mock<IReleaseFilesService>(),
                    new Mock<IReleaseStatusService>(),
                    MockUserManager(),
                    new Mock<IDataBlockService>()
                );
        }

        private static ReleasesController ReleasesControllerWithMocks((
            Mock<IReleaseService> ReleaseService,
            Mock<IReleaseFilesService> FileStorageService,
            Mock<IReleaseStatusService> ReleaseStatusService,
            Mock<UserManager<ApplicationUser>> UserManager,
            Mock<IDataBlockService> DataBlockService
            ) mocks)
        {
            return new ReleasesController(
                mocks.ReleaseService.Object,
                mocks.FileStorageService.Object,
                mocks.ReleaseStatusService.Object,
                mocks.UserManager.Object,
                mocks.DataBlockService.Object);
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            mock.Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(ApplicationUser);

            return mock;
        }
    }
}