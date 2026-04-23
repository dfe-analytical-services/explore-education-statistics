#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau.BauCacheController;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using Capture = Moq.Capture;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public class BauCacheControllerTests
{
    [Fact]
    public async Task ClearPrivateCache_SingleValidPath()
    {
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        privateBlobStorageService
            .Setup(s => s.DeleteBlobs(PrivateContent, null, Capture.With(match)))
            .Returns(Task.CompletedTask);

        var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

        var result = await controller.ClearPrivateCache(
            new ClearPrivateCachePathsViewModel { Paths = SetOf("subject-meta") }
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
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        privateBlobStorageService
            .Setup(s => s.DeleteBlobs(PrivateContent, null, Capture.With(match)))
            .Returns(Task.CompletedTask);

        var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

        var result = await controller.ClearPrivateCache(
            new ClearPrivateCachePathsViewModel { Paths = ["data-blocks", "subject-meta"] }
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
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

        var result = await controller.ClearPrivateCache(new ClearPrivateCachePathsViewModel());

        VerifyAllMocks(privateBlobStorageService);

        result.AssertNoContent();
    }

    [Fact]
    public async Task UpdatePublicCacheGlossary()
    {
        var glossaryCacheService = new Mock<IGlossaryCacheService>(Strict);

        glossaryCacheService.Setup(s => s.UpdateGlossary()).ReturnsAsync([]);

        var controller = BuildController(glossaryCacheService: glossaryCacheService.Object);

        var result = await controller.UpdatePublicCacheGlossary();

        VerifyAllMocks(glossaryCacheService);

        result.AssertNoContent();
    }

    [Fact]
    public async Task ClearPublicCachePublication()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        publicBlobStorageService
            .Setup(s => s.DeleteBlobs(PublicContent, null, Capture.With(match)))
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
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        publicBlobStorageService
            .Setup(s => s.DeleteBlobs(PublicContent, null, Capture.With(match)))
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
    public async Task ClearPrivateGeoJsonCacheJson()
    {
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        privateBlobStorageService
            .Setup(s => s.DeleteBlobs(PrivateContent, null, Capture.With(match)))
            .Returns(Task.CompletedTask);

        var controller = BuildController(privateBlobStorageService: privateBlobStorageService.Object);

        var result = await controller.ClearPrivateGeoJsonCacheJson();

        VerifyAllMocks(privateBlobStorageService);

        result.AssertNoContent();

        var guid = Guid.NewGuid();
        var regex = Assert.IsType<Regex>(options.IncludeRegex);
        Assert.Matches(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/{guid}-1.json");
        Assert.Matches(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/{guid}-12.json");
        Assert.Matches(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/{guid}-123.json");
        Assert.DoesNotMatch(regex, $"releases/{guid}/data-blocks/{guid}");
        Assert.DoesNotMatch(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/");
        Assert.DoesNotMatch(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/{guid}-.json");
        Assert.DoesNotMatch(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/{guid}-a.json");
        Assert.DoesNotMatch(regex, $"releases/{guid}/data-blocks/{guid}-boundary-levels/{guid}-1234.json");
    }

    [Fact]
    public async Task ClearPublicGeoJsonCacheJson()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        publicBlobStorageService
            .Setup(s => s.DeleteBlobs(PublicContent, null, Capture.With(match)))
            .Returns(Task.CompletedTask);

        var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

        var result = await controller.ClearPublicGeoJsonCacheJson();

        VerifyAllMocks(publicBlobStorageService);

        result.AssertNoContent();

        var guid = Guid.NewGuid();
        var regex = Assert.IsType<Regex>(options.IncludeRegex);
        Assert.Matches(
            regex,
            $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/{guid}-1.json"
        );
        Assert.Matches(
            regex,
            $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/{guid}-12.json"
        );
        Assert.Matches(
            regex,
            $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/{guid}-123.json"
        );
        Assert.DoesNotMatch(regex, $"publications/publication-1/releases/2020-01/data-blocks/{guid}");
        Assert.DoesNotMatch(regex, $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/");
        Assert.DoesNotMatch(
            regex,
            $"publications/publication-1/releases/abcd-ef/data-blocks/{guid}-boundary-levels/{guid}-1.json"
        );
        Assert.DoesNotMatch(
            regex,
            $"publications/publication-1/releases/2020-1/data-blocks/{guid}-boundary-levels/{guid}-.json"
        );
        Assert.DoesNotMatch(
            regex,
            $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/{guid}-.json"
        );
        Assert.DoesNotMatch(
            regex,
            $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/{guid}-a.json"
        );
        Assert.DoesNotMatch(
            regex,
            $"publications/publication-1/releases/2020-01/data-blocks/{guid}-boundary-levels/{guid}-1234.json"
        );
    }

    [Fact]
    public async Task ClearPublicCacheReleases_SingleValidPath()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        publicBlobStorageService
            .Setup(s => s.DeleteBlobs(PublicContent, null, Capture.With(match)))
            .Returns(Task.CompletedTask);

        var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

        var result = await controller.ClearPublicCacheReleases(
            new ClearPublicCacheReleasePathsViewModel { Paths = SetOf("subject-meta") }
        );

        VerifyAllMocks(publicBlobStorageService);

        result.AssertNoContent();

        var regex = Assert.IsType<Regex>(options.IncludeRegex);
        Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
        Assert.DoesNotMatch(regex, "publications/releases/release-1/subject-meta/something");
        Assert.DoesNotMatch(regex, "publications/publication-1/releases/subject-meta/something");
        Assert.DoesNotMatch(regex, "something/publications/publication-1/releases/release-1/subject-meta/something");
        Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/data-blocks/something");
        Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/invalid/something");
        Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/something");
    }

    [Fact]
    public async Task ClearPublicCacheReleases_AllValidPaths()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        DeleteBlobsOptions options = null!;
        var match = new CaptureMatch<DeleteBlobsOptions>(param => options = param);

        publicBlobStorageService
            .Setup(s => s.DeleteBlobs(PublicContent, null, Capture.With(match)))
            .Returns(Task.CompletedTask);

        var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

        var result = await controller.ClearPublicCacheReleases(
            new ClearPublicCacheReleasePathsViewModel { Paths = ["data-blocks", "subject-meta"] }
        );

        VerifyAllMocks(publicBlobStorageService);

        result.AssertNoContent();

        var regex = Assert.IsType<Regex>(options.IncludeRegex);
        Assert.Matches(regex, "publications/publication-1/releases/release-1/data-blocks/something");
        Assert.Matches(regex, "publications/publication-1/releases/release-1/subject-meta/something");
        Assert.DoesNotMatch(regex, "something/publications/publication-1/releases/release-1/data-blocks/something");
        Assert.DoesNotMatch(regex, "something/publications/publication-1/releases/release-1/subject-meta/something");
        Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/invalid/something");
        Assert.DoesNotMatch(regex, "publications/publication-1/releases/release-1/something");
    }

    [Fact]
    public async Task ClearPublicCacheReleases_Empty()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

        var result = await controller.ClearPublicCacheReleases(new ClearPublicCacheReleasePathsViewModel());

        VerifyAllMocks(publicBlobStorageService);

        result.AssertNoContent();
    }

    [Fact]
    public async Task UpdatePublicCacheTrees_SingleValidCacheEntry()
    {
        var publicationTreeService = new Mock<IPublicationsTreeService>(Strict);

        publicationTreeService
            .Setup(s => s.UpdateCachedPublicationsTree(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var controller = BuildController(publicationsTreeService: publicationTreeService.Object);

        const string publicationTreeOption = nameof(UpdatePublicCacheTreePathsViewModel.CacheEntry.PublicationTree);

        var result = await controller.UpdatePublicCacheTrees(
            new UpdatePublicCacheTreePathsViewModel { CacheEntries = SetOf(publicationTreeOption) }
        );

        VerifyAllMocks(publicationTreeService);

        result.AssertNoContent();
    }

    [Fact]
    public async Task UpdatePublicCacheTrees_AllValidCacheEntries()
    {
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationsTreeService = new Mock<IPublicationsTreeService>(Strict);

        publicationsTreeService
            .Setup(s => s.UpdateCachedPublicationsTree(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        methodologyCacheService
            .Setup(s => s.UpdateSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>([]));

        var controller = BuildController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationsTreeService: publicationsTreeService.Object
        );

        const string publicationTreeOption = nameof(UpdatePublicCacheTreePathsViewModel.CacheEntry.PublicationTree);
        const string methodologyTreeOption = nameof(UpdatePublicCacheTreePathsViewModel.CacheEntry.MethodologyTree);

        var result = await controller.UpdatePublicCacheTrees(
            new UpdatePublicCacheTreePathsViewModel { CacheEntries = [publicationTreeOption, methodologyTreeOption] }
        );

        VerifyAllMocks(publicationsTreeService, methodologyCacheService);

        result.AssertNoContent();
    }

    [Fact]
    public async Task UpdatePublicCacheTrees_Empty()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        var controller = BuildController(publicBlobStorageService: publicBlobStorageService.Object);

        var result = await controller.UpdatePublicCacheTrees(new UpdatePublicCacheTreePathsViewModel());

        VerifyAllMocks(publicBlobStorageService);

        result.AssertNoContent();
    }

    private static BauCacheController BuildController(
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IPublicBlobStorageService? publicBlobStorageService = null,
        IGlossaryCacheService? glossaryCacheService = null,
        IMethodologyCacheService? methodologyCacheService = null,
        IPublicationsTreeService? publicationsTreeService = null
    )
    {
        return new BauCacheController(
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
            publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict),
            glossaryCacheService ?? Mock.Of<IGlossaryCacheService>(Strict),
            methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            publicationsTreeService ?? Mock.Of<IPublicationsTreeService>(Strict)
        );
    }
}
