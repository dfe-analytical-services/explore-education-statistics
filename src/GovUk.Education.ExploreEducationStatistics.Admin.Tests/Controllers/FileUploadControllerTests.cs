using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Tools.Controllers;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
{
    public class FileUploadControllerTests
    {
        [Fact]
        public async Task Upload_ReturnsAViewResult_ForUploadingFiles()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var importService = new Mock<IImportService>();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("FileUploadGet");

            var options = builder.Options;

            var releases = new List<Release>
            {
                new Release
                {
                    Id = new Guid("c023de5b-0432-49ee-9768-fe7bf8b7711b"), 
                },
                new Release
                {
                    Id = new Guid("69a6cb4f-1ec7-4b34-a2cf-0a8114dfe15f"),
                }
            };

            using (var context = new ApplicationDbContext(options))
            {
                context.Releases.AddRange(releases);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new FileUploadController(context, fileStorageService.Object, importService.Object);

                var actionResult = controller.Upload();
                var viewResult = Assert.IsType<ViewResult>(actionResult);

                var selectList =
                    Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["ReleaseId"]);
                
                Assert.Equal(releases.Select(r => r.Id), selectList.Items.Cast<Release>().Select(r => r.Id));
            }
        }

        [Fact]
        public async Task Post_UploadsTheFileAndRedirects()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("FileUploadPost");

            var options = builder.Options;

            var publication = new Publication
            {
                Id = new Guid("6afce66b-4353-4a56-8c27-616a688a58b9"), Title = "Publication", Slug = "publication"
            };

            var release = new Release
            {
                Id = new Guid("7d00da4b-8948-4db6-bf33-aa1091df9d02"), Slug = "release",
                Publication = publication
            };

            using (var context = new ApplicationDbContext(options))
            {
                context.Releases.Add(release);
                context.SaveChanges();
            }

            var fileStorageService = new Mock<IFileStorageService>();
            var importService = new Mock<IImportService>();

            var dataFile = MockFile("datafile.csv");
            var metaFile = MockFile("metafile.csv");

            fileStorageService.Setup(service =>
                    service.UploadFilesAsync(publication.Slug, release.Slug, dataFile, metaFile, "Subject name"))
                .Returns(Task.CompletedTask);

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new FileUploadController(context, fileStorageService.Object, importService.Object);

                var actionResult = await controller.Post(release.Id, "Subject name", dataFile, metaFile);

                var redirect = Assert.IsAssignableFrom<RedirectToActionResult>(actionResult);

                Assert.Equal("File", redirect.ControllerName);
                Assert.Equal("List", redirect.ActionName);
            }
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