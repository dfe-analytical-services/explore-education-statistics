using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

public class ReleaseCacheServiceTests : CacheServiceTestFixture
{
    private const string PublicationSlug = "publication-slug";
    private const string ReleaseSlug = "2022";
    private static readonly Guid ReleaseVersionId = Guid.NewGuid();

    private readonly ReleaseCacheViewModel _releaseViewModel = new(ReleaseVersionId)
    {
        NextReleaseDate = new PartialDate(),
        Published = DateTime.UtcNow,
        PublishingOrganisations =
        [
            new OrganisationViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Test Organisation",
                Url = "https://test-organisation",
            },
        ],
        Updates = [new ReleaseNoteViewModel { Id = Guid.NewGuid(), On = DateTime.UtcNow }],
        Content =
        [
            new ContentSectionViewModel
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Content =
                [
                    new HtmlBlockViewModel { Id = Guid.NewGuid() },
                    new DataBlockViewModel { Id = Guid.NewGuid(), Charts = [new LineChart()] },
                ],
            },
        ],
        SummarySection = ContentSectionWithHtmlBlock(),
        HeadlinesSection = ContentSectionWithHtmlBlock(),
        KeyStatistics = [new KeyStatisticTextViewModel(), new KeyStatisticDataBlockViewModel()],
        KeyStatisticsSecondarySection = ContentSectionWithHtmlBlock(),
        RelatedDashboardsSection = ContentSectionWithHtmlBlock(),
        DownloadFiles =
        [
            new FileInfo
            {
                Id = Guid.NewGuid(),
                Name = "Test file",
                FileName = "test-file.txt",
                Size = "10 Kb",
                Type = FileType.Ancillary,
            },
        ],
        Type = ReleaseType.AccreditedOfficialStatistics,
        RelatedInformation = [new LinkViewModel { Id = Guid.NewGuid() }],
    };

    [Fact]
    public async Task GetRelease_NoCachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync((object?)null);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, ReleaseSlug)).ReturnsAsync(_releaseViewModel);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_CachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService();

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_ReleaseNotFound()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync((object?)null);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, ReleaseSlug)).ReturnsAsync(new NotFoundResult());

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetRelease_LatestRelease_NoCachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync((object?)null);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, null)).ReturnsAsync(_releaseViewModel);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_LatestRelease_CachedRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService();

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_LatestRelease_ReleaseNotFound()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(ReleaseCacheViewModel)))
            .ReturnsAsync((object?)null);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(PublicationSlug, null)).ReturnsAsync(new NotFoundResult());

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug, ReleaseSlug);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(ReleaseVersionId, null)).ReturnsAsync(_releaseViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.UpdateRelease(
            ReleaseVersionId,
            publicationSlug: PublicationSlug,
            releaseSlug: ReleaseSlug
        );

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task UpdateRelease_LatestRelease()
    {
        var cacheKey = new ReleaseCacheKey(PublicationSlug);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(ReleaseVersionId, null)).ReturnsAsync(_releaseViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.UpdateRelease(ReleaseVersionId, publicationSlug: PublicationSlug);

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task UpdateReleaseStaged()
    {
        var expectedPublishDate = DateTime.UtcNow;
        var cacheKey = new ReleaseStagedCacheKey(PublicationSlug, ReleaseSlug);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(ReleaseVersionId, expectedPublishDate)).ReturnsAsync(_releaseViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.UpdateReleaseStaged(
            ReleaseVersionId,
            expectedPublishDate,
            publicationSlug: PublicationSlug,
            releaseSlug: ReleaseSlug
        );

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task UpdateReleaseStaged_LatestRelease()
    {
        var expectedPublishDate = DateTime.UtcNow;
        var cacheKey = new ReleaseStagedCacheKey(PublicationSlug);

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.GetRelease(ReleaseVersionId, expectedPublishDate)).ReturnsAsync(_releaseViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _releaseViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(releaseService: releaseService.Object);

        var result = await service.UpdateReleaseStaged(
            ReleaseVersionId,
            expectedPublishDate,
            publicationSlug: PublicationSlug
        );

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(releaseService, PublicBlobCacheService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public void ReleaseCacheViewModel_SerializeAndDeserialize()
    {
        var serializedObject = SerializeObject(_releaseViewModel);
        var converted = DeserializeObject<ReleaseCacheViewModel>(serializedObject);
        converted.AssertDeepEqualTo(_releaseViewModel);
    }

    private static ContentSectionViewModel ContentSectionWithHtmlBlock()
    {
        return new ContentSectionViewModel
        {
            Id = Guid.NewGuid(),
            Order = 1,
            Content = new List<IContentBlockViewModel> { new HtmlBlockViewModel { Id = Guid.NewGuid() } },
        };
    }

    private static ReleaseCacheService BuildService(
        IReleaseService? releaseService = null,
        IPublicBlobStorageService? publicBlobStorageService = null
    )
    {
        return new ReleaseCacheService(
            releaseService: releaseService ?? Mock.Of<IReleaseService>(Strict),
            publicBlobStorageService: publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict)
        );
    }
}
