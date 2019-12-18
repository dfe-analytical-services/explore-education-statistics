using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ReleaseControllerTests
    {
        private static readonly List<ApplicationUser> Users = new List<ApplicationUser>
        {
            new ApplicationUser()
            {
                Id = "1",
                Email = "test@example.com"
            }
        };

        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _publicationId = Guid.NewGuid();
        
        private readonly Task<Either<ActionResult, Release>> _releaseExistsResult 
            = Task.FromResult(new Either<ActionResult, Release>(new Release()));
        
        private readonly Task<Either<ActionResult, Publication>> _publicationExistsResult 
            = Task.FromResult(new Either<ActionResult, Publication>(new Publication()));
        
        private readonly Task<Either<ActionResult, Release>> _releaseNotFoundResult 
            = Task.FromResult(new Either<ActionResult, Release>(new NotFoundResult()));

        public ReleaseControllerTests()
        {
            SetupUser();
        }

        [Fact]
        public async void Create_Release_Returns_Ok()
        {
            var mocks = Mocks();
            SetupPublicationExistsResult(mocks.PublicationHelper);
            
            mocks.ReleaseService.Setup(s => s.CreateReleaseAsync(It.IsAny<CreateReleaseViewModel>()))
                .Returns(Task.FromResult(new Either<ActionResult, ReleaseViewModel>(new ReleaseViewModel())));
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.CreateReleaseAsync(new CreateReleaseViewModel(), _publicationId);
            AssertOkResult<ReleaseViewModel>(result);
        }

        [Fact]
        public async Task AddAncillaryFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var mocks = Mocks();
            SetupReleaseExistsResult(mocks.ReleaseHelper);
            
            var ancillaryFile = MockFile("ancillaryFile.doc");
            mocks.FileStorageService
                .Setup(service =>
                    service.UploadFilesAsync(_releaseId, ancillaryFile, "File name", ReleaseFileTypes.Ancillary, false))
                .Returns(Task.FromResult<Either<ActionResult, IEnumerable<FileInfo>>>(new List<FileInfo>()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var actionResult = await controller.AddAncillaryFilesAsync(_releaseId, "File name", ancillaryFile);
            var unboxed = AssertOkResult(actionResult);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task AddAncillaryFilesAsync_UploadsTheFiles_Returns_NotFound()
        {
            var mocks = Mocks();
            SetupReleaseNotFoundResult(mocks.ReleaseHelper);
            
            var ancillaryFile = MockFile("ancillaryFile.doc");
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var actionResult = await controller.AddAncillaryFilesAsync(_releaseId, "File name", ancillaryFile);
            AssertNotFound(actionResult);
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
            SetupReleaseExistsResult(mocks.ReleaseHelper);
            mocks.FileStorageService.Setup(s => s.ListFilesAsync(_releaseId, ReleaseFileTypes.Ancillary))
                .Returns(Task.FromResult(testFiles));
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.GetAncillaryFilesAsync(_releaseId);
            var unboxed = AssertOkResult<IEnumerable<FileInfo>>(result);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task GetAncillaryFilesAsync_Returns_NotFound()
        {
            var mocks = Mocks();
            SetupReleaseNotFoundResult(mocks.ReleaseHelper);

            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test 
            var result = await controller.GetAncillaryFilesAsync(_releaseId);
            AssertNotFound(result);
        }
        
        [Fact(Skip="Needs principal setting")]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");
            
            SetupReleaseExistsResult(mocks.ReleaseHelper);

            mocks.FileStorageService
                .Setup(service => service.UploadDataFilesAsync(_releaseId, dataFile, metaFile, "Subject name", false, "test user"))
                .Returns(Task.FromResult<Either<ActionResult, IEnumerable<FileInfo>>>(new List<FileInfo>()));

            // Call the method under test
            var controller = ReleasesControllerWithMocks(mocks);
            var result = await controller.AddDataFilesAsync(_releaseId, "Subject name", dataFile, metaFile);
            var unboxed = AssertOkResult(result);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_NotFound()
        {
            var mocks = Mocks();
            SetupReleaseNotFoundResult(mocks.ReleaseHelper);
            
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");
            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.AddDataFilesAsync(_releaseId, "Subject name", dataFile, metaFile);
            AssertNotFound(result);
        }

        [Fact(Skip="Needs principal setting")]
        public async Task AddDataFilesAsync_UploadsTheFiles_Returns_ValidationProblem()
        {
            var mocks = Mocks();
            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");
            
            SetupReleaseExistsResult(mocks.ReleaseHelper);

            mocks.FileStorageService
                .Setup(service => service.UploadDataFilesAsync(_releaseId, dataFile, metaFile, "Subject name", false, 
                    "test user"))
                .Returns(Task.FromResult<Either<ActionResult, IEnumerable<FileInfo>>>(
                    new BadRequestObjectResult(CannotOverwriteFile)));

            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.AddDataFilesAsync(_releaseId, "Subject name", dataFile, metaFile);
            AssertValidationProblem(result, CannotOverwriteFile);
        }

        [Fact]
        public async Task GetDataFilesAsync_Returns_A_List_Of_Files()
        {
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
            
            SetupReleaseExistsResult(mocks.ReleaseHelper);
            
            mocks.FileStorageService.Setup(s => s.ListFilesAsync(_releaseId, ReleaseFileTypes.Data))
                .Returns(Task.FromResult(testFiles));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.GetDataFilesAsync(_releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Contains(unboxed, f => f.Name == "Release a file 1");
            Assert.Contains(unboxed, f => f.Name == "Release a file 2");
        }

        [Fact]
        public async Task GetDataFilesAsync_Returns_NotFound()
        {
            var mocks = Mocks();
            SetupReleaseNotFoundResult(mocks.ReleaseHelper);

            var controller = ReleasesControllerWithMocks(mocks);
            
            // Call the method under test
            var result = await controller.GetDataFilesAsync(_releaseId);
            AssertNotFound(result);
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_OK()
        {
            var mocks = Mocks();
            
            SetupReleaseExistsResult(mocks.ReleaseHelper);
            
            mocks.FileStorageService
                .Setup(service => service.DeleteDataFileAsync(_releaseId, "datafilename"))
                .Returns(Task.FromResult<Either<ActionResult, IEnumerable<FileInfo>>>(new List<FileInfo>()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.DeleteDataFiles(_releaseId, "datafilename","subject title");
            var unboxed = AssertOkResult(result);
            Assert.NotNull(unboxed);
        }

        [Fact]
        public async Task DeleteDataFilesAsync_Returns_ValidationProblem()
        {
            var mocks = Mocks();
            
            SetupReleaseExistsResult(mocks.ReleaseHelper);
            
            mocks.FileStorageService
                .Setup(service => service.DeleteDataFileAsync(_releaseId, "datafilename"))
                .Returns(Task.FromResult<Either<ActionResult, IEnumerable<FileInfo>>>(
                    ValidationActionResult(UnableToFindMetadataFileToDelete)));
            var controller = ReleasesControllerWithMocks(mocks);

            // Call the method under test
            var result = await controller.DeleteDataFiles(_releaseId, "datafilename","subject title");
            AssertValidationProblem(result, UnableToFindMetadataFileToDelete);
        }

        [Fact]
        public async void Edit_Release_Summary_Returns_Ok()
        {
            var mocks = Mocks();
            
            SetupReleaseExistsResult(mocks.ReleaseHelper);
            
            mocks.ReleaseService
                .Setup(s => s.EditReleaseSummaryAsync(
                    It.Is<Guid>(id => id.Equals(_releaseId)), 
                    It.IsAny<UpdateReleaseSummaryRequest>())
                )
                .Returns(Task.FromResult(
                    new Either<ActionResult, ReleaseViewModel>(new ReleaseViewModel {Id = _releaseId})));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.UpdateReleaseSummaryAsync(new UpdateReleaseSummaryRequest(), _releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Equal(_releaseId, unboxed.Id);
        }

        [Fact]
        public async void Get_Release_Summary_Returns_Ok()
        {
            var mocks = Mocks();
            mocks.ReleaseService
                .Setup(s => s.GetReleaseSummaryAsync(It.IsAny<Guid>()))
                .Returns<Guid>(id => Task.FromResult(new Either<ActionResult, ReleaseSummaryViewModel>(new ReleaseSummaryViewModel {Id = id})));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.GetReleaseSummaryAsync(_releaseId);
            var unboxed = AssertOkResult(result);
            Assert.Equal(unboxed.Id, _releaseId);
        }

        [Fact]
        public async void Get_Releases_For_Publication_Returns_Ok()
        {
            var mocks = Mocks();
            mocks.ReleaseService
                .Setup(s => s.GetReleasesForPublicationAsync(It.Is<Guid>(id => id == _releaseId)))
                .Returns<Guid>(x => Task.FromResult(new List<ReleaseViewModel>()));
            var controller = ReleasesControllerWithMocks(mocks);

            // Method under test
            var result = await controller.GetReleaseForPublicationAsync(_releaseId);
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

        private static (Mock<IImportService> ImportService,
            Mock<IReleaseService> ReleaseService,
            Mock<IFileStorageService> FileStorageService,
            Mock<IImportStatusService> ImportStatusService,
            Mock<IPublishingService> PublishingService,
            Mock<ISubjectService> SubjectService,
            Mock<ITableStorageService> TableStorageService,
            Mock<UserManager<ApplicationUser>> UserManager,
            Mock<IPersistenceHelper<Release, Guid>> ReleaseHelper,
            Mock<IPersistenceHelper<Publication, Guid>> PublicationHelper) Mocks()
        {
            return (new Mock<IImportService>(),
                    new Mock<IReleaseService>(),
                    new Mock<IFileStorageService>(),
                    new Mock<IImportStatusService>(),
                    new Mock<IPublishingService>(),
                    new Mock<ISubjectService>(),
                    new Mock<ITableStorageService>(),
                    MockUserManager<ApplicationUser>(Users),
                    new Mock<IPersistenceHelper<Release, Guid>>(),
                    new Mock<IPersistenceHelper<Publication, Guid>>()
                );
        }

        private static ReleasesController ReleasesControllerWithMocks((
            Mock<IImportService> ImportService,
            Mock<IReleaseService> ReleaseService,
            Mock<IFileStorageService> FileStorageService,
            Mock<IImportStatusService> ImportStatusService,
            Mock<IPublishingService> PublishingService,
            Mock<ISubjectService> SubjectService,
            Mock<ITableStorageService> TableStorageService,
            Mock<UserManager<ApplicationUser>> UserManager,
            Mock<IPersistenceHelper<Release, Guid>> ReleaseHelper,
            Mock<IPersistenceHelper<Publication, Guid>> PublicationHelper) mocks)
        {
            return new ReleasesController(
                mocks.ImportService.Object,
                mocks.ReleaseService.Object,
                mocks.FileStorageService.Object,
                mocks.ImportStatusService.Object,
                mocks.PublishingService.Object,
                mocks.SubjectService.Object,
                mocks.TableStorageService.Object,
                mocks.UserManager.Object,
                mocks.ReleaseHelper.Object,
                mocks.PublicationHelper.Object
                );
        }
        
        private static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }
        
        private void SetupReleaseExistsResult(Mock<IPersistenceHelper<Release, Guid>> releaseHelper)
        {
            releaseHelper
                .Setup(s => s
                    .CheckEntityExistsActionResult(_releaseId, null))
                .Returns(_releaseExistsResult);
        }
        
        private void SetupReleaseNotFoundResult(Mock<IPersistenceHelper<Release, Guid>> releaseHelper)
        {
            releaseHelper
                .Setup(s => s
                    .CheckEntityExistsActionResult(_releaseId, null))
                .Returns(_releaseNotFoundResult);
        }

        private void SetupPublicationExistsResult(Mock<IPersistenceHelper<Publication, Guid>> publicationHelper)
        {
            publicationHelper
                .Setup(s => s
                    .CheckEntityExistsActionResult(_publicationId, null))
                .Returns(_publicationExistsResult);
        }

        private void SetupUser()
        {
            // TODO - This doesn't work
//            var claims = new List<Claim>() 
//            { 
//                new Claim(ClaimTypes.Name, "username"),
//                new Claim(ClaimTypes.NameIdentifier, "userId"),
//                new Claim("name", "John Doe"),
//            };
//            var identity = new ClaimsIdentity(claims, "TestAuthType");
//            var claimsPrincipal = new ClaimsPrincipal(identity);
//            AppDomain.CurrentDomain.SetThreadPrincipal(claimsPrincipal); 
        }
    }
}