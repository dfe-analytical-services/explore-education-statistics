#nullable enable
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau.BauCacheController;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;
using static Moq.MockBehavior;
using Capture = Moq.Capture;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau
{
    public class BauCacheControllerTests
    {
        [Fact]
        public async Task ClearPrivateCache()
        {
            var privateBlobStorageService = new Mock<IBlobStorageService>(Strict);

            privateBlobStorageService
                .Setup(s => s.DeleteBlobs(PrivateContent, default, default))
                .Returns(Task.CompletedTask);

            var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

            var result = await controller.ClearPrivateCache();

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(privateBlobStorageService);
        }

        [Fact]
        public async Task ClearPublicCacheReleases_SingleValidPath()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            DeleteBlobsOptions options = null!;
            var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

            publicBlobStorageService
                .Setup(
                    s =>
                        s.DeleteBlobs(PublicContent, null, Capture.With(match)))
                .Returns(Task.CompletedTask);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheReleases(
                new ClearCachePathsViewModel
                {
                    Paths = ListOf("subject-meta")
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/data-blocks/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/fast-track-results/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/invalid/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/something");
        }

        [Fact]
        public async Task ClearPublicCacheReleases_AllValidPaths()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            DeleteBlobsOptions options = null!;
            var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

            publicBlobStorageService
                .Setup(
                    s =>
                        s.DeleteBlobs(PublicContent, null, Capture.With(match)))
                .Returns(Task.CompletedTask);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheReleases(
                new ClearCachePathsViewModel
                {
                    Paths = new List<string>
                    {
                        "data-blocks",
                        "subject-meta",
                        "fast-track-results"
                    }
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/releases/release-1/data-blocks/something");
            Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
            Assert.Matches(regex, "publications/publication-1/releases/release-1/fast-track-results/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/invalid/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/something");
        }

        [Fact]
        public async Task ClearPublicCacheReleases_InvalidPaths()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheReleases(
                new ClearCachePathsViewModel
                {
                    Paths = new List<string>
                    {
                        "not a path",
                        "datablocks",
                        "Data-Blocks",
                        "SubjectMeta"
                    }
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);
        }

        [Fact]
        public async Task ClearPublicCacheReleases_Empty()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheReleases(new ClearCachePathsViewModel());

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);
        }

        [Fact]
        public async Task ClearPublicCacheTrees_SingleValidPath()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var paths = new List<string>();

            publicBlobStorageService
                .Setup(s => s.DeleteBlob(PublicContent, Capture.In(paths)))
                .Returns(Task.CompletedTask);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheTrees(
                new ClearCachePathsViewModel
                {
                    Paths = ListOf("publication-tree-any-data.json"),
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Single(paths);
            Assert.Equal("publication-tree-any-data.json", paths[0]);
        }

        [Fact]
        public async Task ClearPublicCacheTrees_AllValidPaths()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var paths = new List<string>();

            publicBlobStorageService
                .Setup(s => s.DeleteBlob(PublicContent, Capture.In(paths)))
                .Returns(Task.CompletedTask);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheTrees(
                new ClearCachePathsViewModel
                {
                    Paths = new List<string>
                    {
                        "publication-tree.json",
                        "publication-tree-any-data.json",
                        "publication-tree-latest-data.json"
                    }
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(3, paths.Count);
            Assert.Equal("publication-tree.json", paths[0]);
            Assert.Equal("publication-tree-any-data.json", paths[1]);
            Assert.Equal("publication-tree-latest-data.json", paths[2]);
        }

        [Fact]
        public async Task ClearPublicCacheTrees_InvalidPaths()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheTrees(
                new ClearCachePathsViewModel
                {
                    Paths = new List<string>
                    {
                        "publication-tree",
                        "publication-treee.json",
                        "publication-tree-some-data.json",
                    }
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);
        }

        [Fact]
        public async Task ClearPublicCacheTrees_Empty()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheTrees(new ClearCachePathsViewModel());

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);
        }

        private static BauCacheController BuildController(
            IBlobStorageService? privateBlobStorageService = null,
            IBlobStorageService? publicBlobStorageService = null)
        {
            return new BauCacheController(
                privateBlobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
                publicBlobStorageService ?? Mock.Of<IBlobStorageService>(Strict)
            );
        }
    }
}