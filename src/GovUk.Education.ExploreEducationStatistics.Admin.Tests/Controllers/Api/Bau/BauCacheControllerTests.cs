#nullable enable
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau.BauCacheController;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
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

            VerifyAllMocks(privateBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "releases/subject-meta/something");
            Assert.DoesNotMatch(regex, "release-1/subject-meta/something");
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

            VerifyAllMocks(privateBlobStorageService);

            result.AssertNoContent();

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

            VerifyAllMocks(privateBlobStorageService);

            result.AssertNoContent();
        }

        [Fact]
        public async Task UpdatePublicCacheGlossary()
        {
            var glossaryCacheService = new Mock<IGlossaryCacheService>(Strict);

            glossaryCacheService.Setup(s => s.UpdateGlossary())
                .ReturnsAsync(new List<GlossaryCategoryViewModel>());

            var controller = BuildController(glossaryCacheService: glossaryCacheService.Object);

            var result = await controller.UpdatePublicCacheGlossary();

            VerifyAllMocks(glossaryCacheService);

            result.AssertNoContent();
        }

        [Fact]
        public async Task ClearPublicCachePublication()
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

            var result = await controller.ClearPublicCachePublication("publication-1");

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/something");
            Assert.Matches(regex, "publications/publication-1/releases/something");
            Assert.Matches(regex, "publications/publication-1/releases/release-1/something");
            Assert.DoesNotMatch(regex, "publication-1/something");
            Assert.DoesNotMatch(regex, "publications/publication-2/something");
            Assert.DoesNotMatch(regex, "publications/publication-2/releases/something");
            Assert.DoesNotMatch(regex, "publications/publication-2/releases/release-1/something");
            Assert.DoesNotMatch(regex, "staging/publications/publication-1/something");
            Assert.DoesNotMatch(regex, "staging/publications/publication-1/releases/something");
            Assert.DoesNotMatch(regex, "staging/publications/publication-1/releases/release-1/something");
        }

        [Fact]
        public async Task ClearPublicCachePublications()
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

            var result = await controller.ClearPublicCachePublications();

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/something");
            Assert.Matches(regex, "publications/publication-1/releases/something");
            Assert.Matches(regex, "publications/publication-1/releases/release-1/something");
            Assert.DoesNotMatch(regex, "staging/publications/publication-1/something");
            Assert.DoesNotMatch(regex, "staging/publications/publication-1/releases/something");
            Assert.DoesNotMatch(regex, "staging/publications/publication-1/releases/release-1/something");
        }

        [Fact]
        public async Task ClearPublicCachePublicationJson()
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

            var result = await controller.ClearPublicCachePublicationJson();

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);

            Assert.Matches(regex, "publications/publication-1/publication.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/publication_json");
            Assert.DoesNotMatch(regex, "something/publications/publication-1/publication.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/publications/publication.json");
        }

        [Fact]
        public async Task ClearPublicCacheReleaseJson()
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

            var result = await controller.ClearPublicCacheReleaseJson();

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/latest-release.json");
            Assert.Matches(regex, "publications/publication-1/releases/1234-56.json");
            Assert.Matches(regex, "publications/publication-1/releases/1234-56-fy.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/latest-release_json");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/1234-56_json");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/1234-56/data-block-id.json");
            Assert.DoesNotMatch(regex, "publications/latest-release.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/1234-56.json");
            Assert.DoesNotMatch(regex, "publications/1234-56.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/latest-release.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/12-56.json");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/latest-release.json.bak");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/1234-56.json.bak");
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

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "publications/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/subject-meta/something");
            Assert.DoesNotMatch(regex,
                "something/publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/data-blocks/something");
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
                        "subject-meta"
                    }
                }
            );

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();

            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Matches(regex, "publications/publication-1/releases/release-1/data-blocks/something");
            Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "something/publications/publication-1/releases/release-1/data-blocks/something");
            Assert.DoesNotMatch(regex,
                "something/publications/publication-1/releases/release-1/subject-meta/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/invalid/something");
            Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/something");
        }

        [Fact]
        public async Task ClearPublicCacheReleases_Empty()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.ClearPublicCacheReleases(new ClearPublicCacheReleasePathsViewModel());

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();
        }

        [Fact]
        public async Task UpdatePublicCacheTrees_SingleValidCacheEntry()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationCacheService
                .Setup(s => s.UpdatePublicationTree())
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>());

            var controller = BuildController(publicationCacheService: publicationCacheService.Object);

            var publicationTreeOption = UpdatePublicCacheTreePathsViewModel.CacheEntry.PublicationTree.ToString();

            var result = await controller.UpdatePublicCacheTrees(
                new UpdatePublicCacheTreePathsViewModel
                {
                    CacheEntries = SetOf(publicationTreeOption)
                }
            );

            VerifyAllMocks(publicationCacheService);

            result.AssertNoContent();
        }

        [Fact]
        public async Task UpdatePublicCacheTrees_AllValidCacheEntries()
        {
            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationCacheService
                .Setup(s => s.UpdatePublicationTree())
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>());

            methodologyCacheService
                .Setup(s => s.UpdateSummariesTree())
                .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                    new List<AllMethodologiesThemeViewModel>()));

            var controller = BuildController(
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object);

            var publicationTreeOption = UpdatePublicCacheTreePathsViewModel.CacheEntry.PublicationTree.ToString();
            var methodologyTreeOption = UpdatePublicCacheTreePathsViewModel.CacheEntry.MethodologyTree.ToString();

            var result = await controller.UpdatePublicCacheTrees(
                new UpdatePublicCacheTreePathsViewModel
                {
                    CacheEntries = new HashSet<string>
                    {
                        publicationTreeOption,
                        methodologyTreeOption
                    }
                }
            );

            VerifyAllMocks(publicationCacheService, methodologyCacheService);

            result.AssertNoContent();
        }

        [Fact]
        public async Task UpdatePublicCacheTrees_Empty()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(Strict);

            var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

            var result = await controller.UpdatePublicCacheTrees(new UpdatePublicCacheTreePathsViewModel());

            VerifyAllMocks(publicBlobStorageService);

            result.AssertNoContent();
        }

        private static BauCacheController BuildController(
            IBlobStorageService? privateBlobStorageService = null,
            IBlobStorageService? publicBlobStorageService = null,
            IGlossaryCacheService? glossaryCacheService = null,
            IMethodologyCacheService? methodologyCacheService = null,
            IPublicationCacheService? publicationCacheService = null)
        {
            return new BauCacheController(
                privateBlobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
                publicBlobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
                glossaryCacheService ?? Mock.Of<IGlossaryCacheService>(Strict),
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict)
            );
        }
    }
}
