using GovUk.Education.ExploreStatistics.Admin.Controllers;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreStatistics.Admin.Models;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Admin.Tests.Controllers
{
    public class UploadControllerTests
    {
        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfFiles()
        {
            // Arrange
            var mockRepo = new Mock<IFileStorageService>();
            var logger = new Mock<ILogger<FileController>>();

            mockRepo.Setup(repo => repo.ListFiles("releases")).Returns(GetTestFiles());

            var controller = new FileController(logger.Object, mockRepo.Object);

            // Act
            var result = await controller.List();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<FileViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Upload_ReturnsAViewResult_ForUploadingFiles()
        {
            // Arrange
            var mockRepo = new Mock<IFileStorageService>();
            var logger = new Mock<ILogger<FileController>>();

            mockRepo.Setup(repo => repo.ListFiles("releases")).Returns(GetTestFiles());

            var controller = new FileController(logger.Object, mockRepo.Object);

            // Act
            var result = await controller.Upload();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Post_ReturnsTheUploadedFiles()
        {
            // Arrange
            var mockRepo = new Mock<IFileStorageService>();
            var logger = new Mock<ILogger<FileController>>();

            mockRepo.Setup(repo => repo.UploadFileAsync("releases", "", new Guid()));

            var controller = new FileController(logger.Object, mockRepo.Object);

            // Act
            var result = await controller.Post(TestFiles());

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        private List<string> GetTestFiles()
        {
            var files = new List<string>();
            files.Add("drive/release-1/File1.csv");
            files.Add("drive/release-1/File2.csv");
            return files;
        }


        private List<IFormFile> TestFiles()
        {
            var fileList = new List<IFormFile>();
            var fileMock = new Mock<IFormFile>();

            //Setup mock file using a memory stream
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var file = fileMock.Object;

            fileList.Add(file);

            return fileList;
        }
    }
}
