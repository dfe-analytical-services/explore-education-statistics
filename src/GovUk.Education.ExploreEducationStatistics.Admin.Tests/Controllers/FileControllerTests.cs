using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Tools.Controllers;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
{
    public class FileControllerTests
    {
        [Fact]
        public void Index_ReturnsAViewResult_WithADictionaryOfFiles()
        {
            const string slugPublicationA = "publication-a";
            const string slugPublicationB = "publication-b";
            const string slugReleaseA = "slug-a";
            const string slugReleaseB = "slug-b";

            var filesReleaseA = new List<FileInfo>
            {
                new FileInfo
                {
                    Extension = "csv",
                    Name = "Release a file 1",
                    Path = slugPublicationA + "/" + slugReleaseA + "/" + "file1.csv",
                    Size = "1 Kb"
                },
                new FileInfo
                {
                    Extension = "csv",
                    Name = "Release a file 2",
                    Path = slugPublicationA + "/" + slugReleaseA + "/" + "file2.csv",
                    Size = "1 Kb"
                }
            };

            var filesReleaseB = new List<FileInfo>
            {
                new FileInfo
                {
                    Extension = "csv",
                    Name = "Release b file 1",
                    Path = slugPublicationB + "/" + slugReleaseB + "/" + "file1.csv",
                    Size = "1 Kb"
                },
                new FileInfo
                {
                    Extension = "csv",
                    Name = "Release b file 2",
                    Path = slugPublicationB + "/" + slugReleaseB + "/" + "file2.csv",
                    Size = "1 Kb"
                }
            };

            var publications = new List<Publication>
            {
                new Publication
                {
                    Id = new Guid("fab2000f-aa11-4a5f-843f-9bc8a5f72ad9"), Title = "Publication A",
                    Slug = slugPublicationA
                },
                new Publication
                {
                    Id = new Guid("e92a987b-7d2a-4098-804b-e27c0f7a7b1b"), Title = "Publication B",
                    Slug = slugPublicationB
                }
            };

            var releases = new List<Release>
            {
                new Release
                {
                    Id = new Guid("3ca848be-0da2-470f-beee-bd7b57f521e8"), Slug = slugReleaseA,
                    Publication = publications[0]
                },
                new Release
                {
                    Id = new Guid("386ee741-67e2-4e2c-89e4-51faf46fd362"), Slug = slugReleaseB,
                    Publication = publications[1]
                }
            };

            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(service => service.ListFiles(slugPublicationA, slugReleaseA))
                .Returns(filesReleaseA);

            fileStorageService.Setup(service => service.ListFiles(slugPublicationB, slugReleaseB))
                .Returns(filesReleaseB);

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("FileControllerList");

            var options = builder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.AddRange(releases);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new FileController(context, fileStorageService.Object);

                var result = controller.List();

                var viewResult = Assert.IsType<ViewResult>(result);
                var model =
                    Assert.IsAssignableFrom<Dictionary<Release, IEnumerable<FileInfo>>>(viewResult.ViewData.Model);

                var expected = new Dictionary<Release, IEnumerable<FileInfo>>
                {
                    {
                        releases[0], filesReleaseA
                    },
                    {
                        releases[1], filesReleaseB
                    }
                };
                
                CollectionAssert.AreEquivalent(expected.ToDictionary(p => p.Key.Id, p => p.Value),
                    model.ToDictionary(p => p.Key.Id, p => p.Value));
            }
        }
    }
}