using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
{
    public class ReleaseControllerTests
    {
        [Fact]
        public async void Create_Release_Returns_Ok()
        {
            var releaseService = new Mock<IReleaseService>();
            var fileStorageService = new Mock<IFileStorageService>();
            var importService = new Mock<IImportService>();


            releaseService.Setup(s => s.CreateReleaseAsync(It.IsAny<CreateReleaseViewModel>()))
                .Returns(Task.FromResult(new ReleaseViewModel()));
            var controller =
                new ReleasesController(releaseService.Object, fileStorageService.Object, importService.Object);

            // Method under test
            var result = await controller.CreateReleaseAsync(new CreateReleaseViewModel(), Guid.NewGuid());
            Assert.IsAssignableFrom<ReleaseViewModel>(result.Value);
        }

        [Fact]
        public async Task UploadFilesAsync_UploadsTheFiles_Returns_Ok()
        {
            var releaseId = Guid.NewGuid();

            var releaseService = new Mock<IReleaseService>();
            var fileStorageService = new Mock<IFileStorageService>();
            var importService = new Mock<IImportService>();

            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            releaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            fileStorageService
                .Setup(service => service.UploadFilesAsync(releaseId, dataFile, metaFile, "Subject name"))
                .Returns(Task.CompletedTask);

            var controller = new ReleasesController(releaseService.Object, fileStorageService.Object,
                importService.Object);

            var actionResult = await controller.AddDataFiles(releaseId, "Subject name", dataFile, metaFile);

            Assert.IsAssignableFrom<OkResult>(actionResult);
        }

        [Fact]
        public async Task UploadFilesAsync_UploadsTheFiles_Returns_NotFound()
        {
            var releaseService = new Mock<IReleaseService>();
            var fileStorageService = new Mock<IFileStorageService>();
            var importService = new Mock<IImportService>();

            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            var controller = new ReleasesController(releaseService.Object, fileStorageService.Object,
                importService.Object);

            var actionResult = await controller.AddDataFiles(Guid.NewGuid(), "Subject name", dataFile, metaFile);

            Assert.IsAssignableFrom<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task GetDataFiles_Returns_A_List_Of_Files()
        {
            var releaseId = Guid.NewGuid();
            var testFiles = new List<FileInfo>
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

            var releaseService = new Mock<IReleaseService>();
            var fileStorageService = new Mock<IFileStorageService>();
            var importService = new Mock<IImportService>();


            releaseService.Setup(s => s.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Release {Id = releaseId}));
            fileStorageService.Setup(s => s.ListFiles(releaseId)).Returns(testFiles);


            var controller = new ReleasesController(releaseService.Object, fileStorageService.Object,
                importService.Object);

            var actionResult = await controller.GetDataFiles(releaseId);

            Assert.IsAssignableFrom<OkObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetDataFiles_Returns_NotFound()
        {
            var releaseService = new Mock<IReleaseService>();
            var fileStorageService = new Mock<IFileStorageService>();

            var importService = new Mock<IImportService>();
            var controller = new ReleasesController(releaseService.Object, fileStorageService.Object,
                importService.Object);

            var actionResult = await controller.GetDataFiles(Guid.NewGuid());

            Assert.Null(actionResult.Value);
            Assert.IsAssignableFrom<NotFoundResult>(actionResult.Result);
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
    }
}