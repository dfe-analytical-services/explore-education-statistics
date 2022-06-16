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
        public async Task ClearPrivateCache_SingleValidPath()
        {
            var privateBlobStorageService = new Mock<IBlobStorageService>(Strict);

            DeleteBlobsOptions options = null!;
            var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

            privateBlobStorageService
                .Setup(
                    s =>
                        s.DeleteBlobs(PrivateContent, null, Capture.With(match)))
                .Returns(Task.CompletedTask);

            var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

            var result = await controller.ClearPrivateCache(
                new ClearPrivateCachePathsViewModel
                {
                    Paths = SetOf("subject-meta")
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "something/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "releases/release-1/data-blocks/something");
            Assert.DoesNotMatch(regex, "releases/release-1/invalid/something");
            Assert.DoesNotMatch(regex, "releases/release-1/something");
        }

        [Fact]
        public async Task ClearPrivateCacheReleases_AllValidPaths()
        {
            var privateBlobStorageService = new Mock<IBlobStorageService>(Strict);

            DeleteBlobsOptions options = null!;
            var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

            privateBlobStorageService
                .Setup(
                    s =>
                        s.DeleteBlobs(PrivateContent, null, Capture.With(match)))
                .Returns(Task.CompletedTask);

            var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

            var result = await controller.ClearPrivateCache(
                new ClearPrivateCachePathsViewModel
                {
                    Paths = new HashSet<string>
                    {
                        "data-blocks",
                        "subject-meta"
                    }
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "releases/release-1/data-blocks/something");
            Assert.Matches(regex, "releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "something/releases/release-1/data-blocks/something");
            Assert.DoesNotMatch(regex, "something/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "releases/release-1/invalid/something");
            Assert.DoesNotMatch(regex, "releases/release-1/something");
        }

        [Fact]
        public async Task ClearPrivateCache_Empty()
        {
            var privateBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

            var result = await controller.ClearPrivateCache(new ClearPrivateCachePathsViewModel());

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
                new ClearPublicCacheReleasePathsViewModel
                {
                    Paths = SetOf("subject-meta")
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex,
                "something/publications/publication-1/releases/release-1/subject-meta/something");
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
                new ClearPublicCacheReleasePathsViewModel
                {
                    Paths = new HashSet<string>
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
            Assert.DoesNotMatch(regex, "something/publications/publication-1/releases/release-1/data-blocks/something");
            Assert.DoesNotMatch(regex,
                "something/publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex,
                "something/publications/publication-1/releases/release-1/fast-track-results/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/invalid/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/something");
        }

        [Fact]
        public async Task ClearPublicCacheReleases_Empty()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheReleases(new ClearPublicCacheReleasePathsViewModel());

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
                new ClearPublicCacheTreePathsViewModel
                {
                    Paths = SetOf("publication-tree.json")
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Single(paths);
            Assert.Equal("publication-tree.json", paths[0]);
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
                new ClearPublicCacheTreePathsViewModel
                {
                    Paths = new HashSet<string>
                    {
                        "publication-tree.json",
                        "methodology-tree.json"
                    }
                }
            );

            result.AssertNoContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(2, paths.Count);
            Assert.Equal("publication-tree.json", paths[0]);
            Assert.Equal("methodology-tree.json", paths[1]);
        }

        [Fact]
        public async Task ClearPublicCacheTrees_Empty()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheTrees(new ClearPublicCacheTreePathsViewModel());

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
