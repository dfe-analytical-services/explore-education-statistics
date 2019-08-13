using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
{
    public class ReleaseControllerTests
    {
        [Fact]
        public async void Create_Release_Returns_Ok()
        {
            var publicationId = Guid.NewGuid();
            var mocks = Mocks();
            mocks.PublicationService.Setup(s => s.GetAsync(It.Is<Guid>(id => id == publicationId)))
                .Returns(Task.FromResult(new Publication()));
            mocks.ReleaseService.Setup(s => s.CreateReleaseAsync(It.IsAny<CreateReleaseViewModel>()))
                .Returns(Task.FromResult(new Either<ValidationResult, ReleaseViewModel>(new ReleaseViewModel())));
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.CreateReleaseAsync(new CreateReleaseViewModel(), publicationId);
            AssertOkResult<ReleaseViewModel>(result);
        }


        [Fact]
        public async Task AddAncillaryFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var releaseId = Guid.NewGuid();
            var mocks = Mocks();
            var ancillaryFile = MockFile("ancillaryFile.doc");
            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService
                .Setup(service =>
                    service.UploadFilesAsync(releaseId, ancillaryFile, "File name", ReleaseFileTypes.Ancillary))
                .Returns(Task.FromResult<Either<ValidationResult, IEnumerable<FileInfo>>>(new List<FileInfo>()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var actionResult = await controller.AddAncillaryFilesAsync(releaseId, "File name", ancillaryFile);
            var unboxed = AssertOkResult(actionResult);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task AddAncillaryFilesAsync_UploadsTheFiles_Returns_NotFound()
        {
            var mocks = Mocks();
            var ancillaryFile = MockFile("ancillaryFile.doc");
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var actionResult = await controller.AddAncillaryFilesAsync(Guid.NewGuid(), "File name", ancillaryFile);
            AssertNotFound(actionResult);
        }

        [Fact]
        public async Task GetAncillaryFilesAsync_Returns_A_List_Of_Files()
        {
            var releaseId = Guid.NewGuid();
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
            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService.Setup(s => s.ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary))
                .Returns(Task.FromResult(testFiles));
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.GetAncillaryFilesAsync(releaseId);
            var unboxed = AssertOkResult<IEnumerable<FileInfo>>(result);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task GetAncillaryFilesAsync_Returns_NotFound()
        {
            var mocks = Mocks();
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test 
            var result = await controller.GetAncillaryFilesAsync(Guid.NewGuid());
            AssertNotFound(result);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var releaseId = Guid.NewGuid();
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService
                .Setup(service => service.UploadDataFilesAsync(releaseId, dataFile, metaFile, "Subject name"))
                .Returns(Task.FromResult<Either<ValidationResult, IEnumerable<FileInfo>>>(new List<FileInfo>()));
            
            // Call the method under test
            var controller = ReleasesControllerWithMocks(mocks);
            var result = await controller.AddDataFilesAsync(releaseId, "Subject name", dataFile, metaFile);
            var unboxed = AssertOkResult(result);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_NotFound()
        {
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.AddDataFilesAsync(Guid.NewGuid(), "Subject name", dataFile, metaFile);
            AssertNotFound(result);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_ValidationProblem()
        {
            var releaseId = Guid.NewGuid();
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");
            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService
                .Setup(service => service.UploadDataFilesAsync(releaseId, dataFile, metaFile, "Subject name"))
                .Returns(Task.FromResult<Either<ValidationResult, IEnumerable<FileInfo>>>(
                    ValidationResult(CannotOverwriteFile)));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.AddDataFilesAsync(releaseId, "Subject name", dataFile, metaFile);
            AssertValidationProblem(result, CannotOverwriteFile);
        }

        [Fact]
        public async Task GetDataFilesAsync_Returns_A_List_Of_Files()
        {
            var releaseId = Guid.NewGuid();
            IEnumerable<FileInfo> testFiles = new[]
            {
                new FileInfo
                {
                    Extension = "csv",
                    Name = "Release a file 1",
                    Path = "file1.csv",
                    Size = "1 Kb"
                },
                new FileInfo
                {
                    Extension = "csv",
                    Name = "Release a file 2",
                    Path = "file2.csv",
                    Size = "1 Kb"
                }
            };

            var mocks = Mocks();
            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService.Setup(s => s.ListFilesAsync(releaseId, ReleaseFileTypes.Data))
                .Returns(Task.FromResult(testFiles));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.GetDataFilesAsync(releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Contains(unboxed, f => f.Name == "Release a file 1");
            Assert.Contains(unboxed, f => f.Name == "Release a file 2");
        }

        [Fact]
        public async Task GetDataFilesAsync_Returns_NotFound()
        {
            var mocks = Mocks();
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.GetDataFilesAsync(Guid.NewGuid());
            AssertNotFound(result);
            
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_OK()
        {
            var releaseId = Guid.NewGuid();
            var mocks = Mocks();
            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService
                .Setup(service => service.DeleteDataFileAsync(releaseId, "datafilename"))
                .Returns(Task.FromResult<Either<ValidationResult, IEnumerable<FileInfo>>>(new List<FileInfo>()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.DeleteDataFiles(releaseId, "datafilename");
            var unboxed = AssertOkResult(result);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_ValidationProblem()
        {
            var releaseId = Guid.NewGuid();
            var mocks = Mocks();
            mocks.ReleaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            mocks.FileStorageService
                .Setup(service => service.DeleteDataFileAsync(releaseId, "datafilename"))
                .Returns(Task.FromResult<Either<ValidationResult, IEnumerable<FileInfo>>>(
                    ValidationResult(UnableToFindMetadataFileToDelete)));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.DeleteDataFiles(releaseId, "datafilename");
            AssertValidationProblem(result, UnableToFindMetadataFileToDelete);
        }


        [Fact]
        public async void Edit_Release_Summary_Returns_Ok()
        {
            var mocks = Mocks();
            var releaseId = new Guid("95bf7743-fe6f-4b85-a28f-49f6f6b8735a");
            mocks.ReleaseService
                .Setup(s => s.EditReleaseSummaryAsync(It.IsAny<EditReleaseSummaryViewModel>()))
                .Returns<EditReleaseSummaryViewModel>(e => Task.FromResult(
                    new Either<ValidationResult, ReleaseViewModel>(new ReleaseViewModel {Id = e.Id})));
            mocks.ReleaseService
                .Setup(s => s.GetAsync(releaseId))
                .Returns(Task.FromResult(new Release()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.EditReleaseSummaryAsync(new EditReleaseSummaryViewModel(), releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Equal(releaseId, unboxed.Id);
        }

        [Fact]
        public async void Get_Release_Summary_Returns_Ok()
        {
            var mocks = Mocks();
            var releaseId = new Guid("95bf7743-fe6f-4b85-a28f-49f6f6b8735a");
            mocks.ReleaseService
                .Setup(s => s.GetReleaseSummaryAsync(It.IsAny<Guid>()))
                .Returns<Guid>(id => Task.FromResult(new EditReleaseSummaryViewModel {Id = id}));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.GetReleaseSummaryAsync(releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Equal(unboxed.Id, releaseId);
        }

        [Fact]
        public async void Get_Releases_For_Publication_Returns_Ok()
        {
            var mocks = Mocks();
            var releaseId = new Guid("fc570a6c-d230-40ae-a5e5-febab330fb12");
            mocks.ReleaseService
                .Setup(s => s.GetReleasesForPublicationAsync(It.Is<Guid>(id => id == releaseId)))
                .Returns<Guid>(x => Task.FromResult(new List<ReleaseViewModel>()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.GetReleaseForPublicationAsync(releaseId);
            var unboxed = AssertOkResult(result);
            Assert.NotNull(unboxed);
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
            Assert.IsAssignableFrom<ActionResult<T>>(result);
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.IsAssignableFrom<T>(okObjectResult?.Value);
            return okObjectResult?.Value as T;
        }
        
        private static void AssertNotFound<T>(ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<ActionResult<T>>(result);
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
            Assert.Null(result.Value);
        }
        
        private static ValidationProblemDetails AssertValidationProblem<T>(ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
            var badRequestObjectResult = result.Result as BadRequestObjectResult;
            Assert.IsAssignableFrom<ValidationProblemDetails>(badRequestObjectResult?.Value);
            var validationProblemDetails = badRequestObjectResult.Value as ValidationProblemDetails;
            return validationProblemDetails;
        }
        
        private static ValidationProblemDetails AssertValidationProblem<T>(ActionResult<T> result, ValidationErrorMessages message) where T : class
        {
            var validationProblem = AssertValidationProblem(result);
            Assert.True(validationProblem.Errors.ContainsKey(string.Empty));
            Assert.Contains(ValidationResult(message).ErrorMessage, validationProblem.Errors[string.Empty]);
            return validationProblem;
        }
        
        private static (Mock<IReleaseService> ReleaseService, Mock<IFileStorageService> FileStorageService,
            Mock<IImportService> ImportService, Mock<IPublicationService> PublicationService) Mocks()
        {
            return (new Mock<IReleaseService>(), new Mock<IFileStorageService>(), new Mock<IImportService>(),
                new Mock<IPublicationService>());
        }

        private static ReleasesController ReleasesControllerWithMocks(
            (Mock<IReleaseService> ReleaseService, Mock<IFileStorageService> FileStorageService, Mock<IImportService>
                ImportService, Mock<IPublicationService> PublicationService) mocks)
        {
            return new ReleasesController(mocks.ReleaseService.Object, mocks.FileStorageService.Object,
                mocks.ImportService.Object, mocks.PublicationService.Object);
        }
    }
}